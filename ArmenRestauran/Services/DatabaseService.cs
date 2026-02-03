using ArmenRestauran.Models;
using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Automation;

namespace ArmenRestauran.Services
{
    public class DatabaseService
    {
        
        private readonly string _connectionString = "Host=localhost;Username=postgres;Password=Passw0rd;Database=ArmenianRestaurant";

        private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

        public List<User> GetAllUsers()
        {
            using var db = CreateConnection();
            return db.Query<User>("SELECT * FROM Users").ToList();
        }
        public List<Table> GetTables() 
        {
            using var db = CreateConnection();
            return db.Query<Table>("SELECT * FROM Tables").ToList();
        }
        public List<Role> GetRoles() 
        {
            using var db = CreateConnection();
            return db.Query<Role>("SELECT * FROM Roles").ToList();
        }

        public List<MenuItem> GetMenu() 
        {
            using var db = CreateConnection();
            return db.Query<MenuItem>("SELECT * FROM Menu").ToList();
        }

        public List<Ingredient> GetIngredients() 
        {
            using var db = CreateConnection();
            return db.Query<Ingredient>("SELECT * FROM Ingredients").ToList();
        }

        public List<Order> GetOrders() 
        {
            using var db = CreateConnection();
            return db.Query<Order>("SELECT * FROM Orders ORDER BY CreatedAt DESC").ToList();
        }
        public List<Category> GetCategories()
        {
            using var db = CreateConnection();
            return db.Query<Category>("SELECT * FROM Categories").ToList();
        }

        public List<MenuItem> GetDishesByCategory(int categoryId)
        {
            using var db = CreateConnection();
            return db.Query<MenuItem>("SELECT * FROM Menu WHERE CategoryID = @categoryId", new { categoryId }).ToList();
        }
        public void UpdateUser(User user)
        {
            using var db = CreateConnection();
            const string sql = @"
                UPDATE users 
                SET firstname = @FirstName, 
                    lastname = @LastName,
                    surname = @SurName,
                    phone = @Phone, 
                    password = @Password,
                    login = @Login,
                    roleId = @RoleID
                WHERE userid = @UserID";

            db.Execute(sql, user);
        }

        
        public void RegisterClient(User user)
        {
            using var db = CreateConnection();
            var parameters = new
            {
                p_role_id = 4,
                p_first_name = user.FirstName,
                p_last_name = user.LastName,
                p_surname = user.SurName,
                p_phone = user.Phone,
                p_login = user.Login,
                p_password = user.Password
            };
            db.Execute("CALL register_new_client(@p_first_name, @p_last_name, @p_surname, @p_phone, @p_login, @p_password)", parameters);
        }

        
        public void AddItemToOrder(int orderId, int itemId, int quantity)
        {
            using var db = CreateConnection();
            db.Execute("CALL add_item_to_order(@p_order_id, @p_item_id, @p_quantity)",
                new { p_order_id = orderId, p_item_id = itemId, p_quantity = quantity });
        }

        public void DecreaseStock(int itemId, int servings)
        {
            using var db = CreateConnection();
            db.Execute("CALL decrease_stock_by_recipe(@p_item_id, @p_servings)",
                new { p_item_id = itemId, p_servings = servings });
        }

        public void CreateFullOrder(long userId, long tableId, DateTime resDate, int guestCount, List<CartService.CartItem> items)
        {
            using (var db = CreateConnection())
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        
                        const string sqlRes = @"
                            INSERT INTO Reservations (UserID, TableID, ReservationDate, GuestCount, Status) 
                            VALUES (@userId, @tableId, @resDate, @guestCount, 'Подтверждена') 
                            RETURNING ReservationID";

                        var resId = db.QuerySingle<int>(sqlRes, new { userId, tableId, resDate, guestCount }, transaction);

                        
                        const string sqlOrder = @"
                            INSERT INTO Orders (ReservationID, UserID, TableID, Status, CreatedAt) 
                            VALUES (@resId, @userId, @tableId, 'Заказан', @createdAt) 
                            RETURNING OrderID";

                        var orderId = db.QuerySingle<int>(sqlOrder, new
                        {
                            resId,
                            userId,
                            tableId,
                            createdAt = DateTime.Now
                        }, transaction);

                       
                        const string sqlItems = @"
                            INSERT INTO OrderItems (OrderID, ItemID, Quantity, Subtotal) 
                            VALUES (@orderId, @itemId, @quantity, @subtotal)";

                        foreach (var item in items)
                        {
                            db.Execute(sqlItems, new
                            {
                                orderId,
                                itemId = item.Product.ItemID,
                                quantity = item.Quantity,
                                subtotal = item.Total
                            }, transaction);
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public List<Table> GetAvailableTables(DateTime date)
        {
            using var db = CreateConnection();
            const string sql = @"
                SELECT * FROM tables 
                WHERE tableid NOT IN (
                    SELECT tableid FROM reservations 
                    WHERE CAST(reservationdate AS DATE) = @date::date
                    AND Status != 'Отменена'
                )";

            return db.Query<Table>(sql, new { date }).ToList();
        }
        public List<UserOrder> GetUserOrders(int userId)
        {
            using (var db = CreateConnection())
            {
                const string sql = @"
                    SELECT 
                        r.ReservationID,
                        o.OrderID, 
                        r.ReservationDate, 
                        t.TableNumber, 
                        r.Status as ReservationStatus,
                        COALESCE(SUM(oi.Subtotal), 0) as TotalAmount,
                        COALESCE(STRING_AGG(m.ItemName || ' (x' || oi.Quantity || ')', ', '), 'Состав заказа удален (заказ отменен)') as ItemsSummary
                    FROM Reservations r
                    JOIN Tables t ON r.TableID = t.TableID
                    LEFT JOIN Orders o ON r.ReservationID = o.ReservationID
                    LEFT JOIN OrderItems oi ON o.OrderID = oi.OrderID
                    LEFT JOIN Menu m ON oi.ItemID = m.ItemID
                    WHERE r.UserID = @userId
                    GROUP BY r.ReservationID, o.OrderID, r.ReservationDate, t.TableNumber, r.Status
                    ORDER BY 
                        (r.ReservationDate < NOW()) ASC, 
                        (r.Status = 'Отменена') ASC,
                        r.ReservationDate ASC";

                return db.Query<UserOrder>(sql, new { userId }).ToList();
            }
        }
        public void CancelAndCleanupOrder(int? orderId, int reservationId)
        {
            using var db = CreateConnection();
            db.Open();
            using var transaction = db.BeginTransaction();
            try
            {
                
                db.Execute("UPDATE Reservations SET Status = 'Отменена' WHERE ReservationID = @reservationId",
                           new { reservationId }, transaction);

                if (orderId.HasValue)
                {
                   
                    db.Execute("DELETE FROM OrderItems WHERE OrderID = @orderId", new { orderId }, transaction);
                    db.Execute("DELETE FROM Orders WHERE OrderID = @orderId", new { orderId }, transaction);
                }
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void SaveMenu(MenuItem menu)
        {
            using var db = CreateConnection();
            if (menu.ItemID == 0)
            {
                db.Execute("INSERT INTO Menu (ItemName, CategoryID, Price, Description) VALUES (@ItemName, @CategoryID, @Price, @Description)", menu);
            }
            else
            {
                db.Execute("UPDATE Menu SET ItemName=@ItemName, CategoryID=@CategoryID, Price=@Price, Description=@Description WHERE ItemID=@ItemID", menu);
            }
        }
        public void DeleteMenu(int id)
        {
            using var db = CreateConnection();
            db.Execute("DELETE FROM Menu WHERE ItemID = @id", new { id });
        }

        public List<MenuItem> GetProductsByCategory(int categoryId)
        {
            using var db = CreateConnection();
            const string sql = "SELECT * FROM Menu WHERE CategoryID = @categoryId ORDER BY ItemName";

            return db.Query<MenuItem>(sql, new { categoryId }).ToList();
        }
        public List<RecipeItem> GetRecipeForMenuItem(int id)
        {
            using var db = CreateConnection();
            const string sql = "SELECT * FROM Recipe WHERE ItemID = @id";

            return db.Query<RecipeItem>(sql, new { id }).ToList();
        }

        public void DeleteProduct(int id)
        {
            using var db = CreateConnection();
            db.Open();
            using var transaction = db.BeginTransaction();

            try
            {
                db.Execute("DELETE FROM Recipe WHERE ItemID = @id", new { id }, transaction);

                db.Execute("DELETE FROM OrderItems WHERE ItemID = @id", new { id }, transaction);

                db.Execute("DELETE FROM Menu WHERE ItemID = @id", new { id }, transaction);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception("Не удалось удалить блюдо. Причина: " + ex.Message);
            }
        }

        public List<RecipeDetailDTO> GetRecipeDetails(int itemId)
        {
            using var db = CreateConnection();
            const string sql = @"
                SELECT r.RecipeID, r.IngredientID, i.IngredientName, r.QuantityRequired, i.Unit
                FROM Recipe r
                JOIN Ingredients i ON r.IngredientID = i.IngredientID
                WHERE r.ItemID = @itemId";
            return db.Query<RecipeDetailDTO>(sql, new { itemId }).ToList();
        }

        public void AddRecipeItem(int itemId, int ingredientId, float qty)
        {
            using var db = CreateConnection();
            db.Execute("INSERT INTO Recipe (ItemID, IngredientID, QuantityRequired) VALUES (@itemId, @ingredientId, @qty)",
                new { itemId, ingredientId, qty });
        }

        public void DeleteRecipeItem(int recipeId)
        {
            using var db = CreateConnection();
            db.Execute("DELETE FROM Recipe WHERE RecipeID = @recipeId", new { recipeId });
        }

        public void UpdateMenuItem(MenuItem item)
        {
            using var db = CreateConnection();
            const string sql = "UPDATE Menu SET ItemName = @ItemName, Price = @Price, Description = @Description WHERE ItemID = @ItemID";
            db.Execute(sql, item);
        }

    }
}
