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

        public void AddIngredient(Ingredient ingredient)
        {
            using var db = CreateConnection();
            const string sql = "INSERT INTO Ingredients (IngredientName, StockQuantity, Unit) VALUES (@IngredientName, @StockQuantity, @Unit)";
            db.Execute(sql, ingredient);
        }

        public void UpdateIngredient(Ingredient ingredient)
        {
            using var db = CreateConnection();
            const string sql = "UPDATE Ingredients SET IngredientName = @IngredientName, StockQuantity = @StockQuantity, Unit = @Unit WHERE IngredientID = @IngredientID";
            db.Execute(sql, ingredient);
        }

        public void DeleteIngredient(int id)
        {
            using var db = CreateConnection();
            db.Execute("DELETE FROM Ingredients WHERE IngredientID = @id", new { id });
        }
        public int AddMenuItem(MenuItem item)
        {
            using var db = CreateConnection();
            const string sql = @"
                INSERT INTO Menu (ItemName, Price, Description, CategoryID) 
                VALUES (@ItemName, @Price, @Description, @CategoryID) 
                RETURNING ItemID";
            return db.QuerySingle<int>(sql, item);
        }
        public List<OrderDTO> GetAllOrders()
        {
            using var db = CreateConnection();
            const string sql = @"
                SELECT o.OrderID, 
                       o.CreatedAt, 
                       u.Login as CustomerLogin, 
                       o.Status, 
                       COALESCE(SUM(oi.Subtotal), 0) as TotalAmount, 
                       t.TableNumber
                FROM Orders o
                JOIN Users u ON o.UserID = u.UserID
                LEFT JOIN Tables t ON o.TableID = t.TableID
                LEFT JOIN OrderItems oi ON o.OrderID = oi.OrderID
                GROUP BY o.OrderID, u.Login, o.Status, o.CreatedAt, t.TableNumber
                ORDER BY o.CreatedAt DESC";

            return db.Query<OrderDTO>(sql).ToList();
        }

        public void DeleteOrder(int orderId)
        {
            using var db = CreateConnection();
            db.Execute("DELETE FROM OrderItems WHERE OrderID = @orderId", new { orderId });
            db.Execute("DELETE FROM Orders WHERE OrderID = @orderId", new { orderId });
        }

        public bool IsTableBusy(int tableId, DateTime requestedTime)
        {
            using var db = CreateConnection();
            const string sql = @"
                SELECT COUNT(*) FROM Bookings 
                WHERE TableID = @tableId 
                AND Status = 'Подтверждена'
                AND BookingDate BETWEEN (@requestedTime - interval '2 hours') AND (@requestedTime + interval '2 hours')";
            return db.ExecuteScalar<int>(sql, new { tableId, requestedTime }) > 0;
        }
        public List<BookingDTO> GetAllBookings()
        {
            using var db = CreateConnection();
            const string sql = @"
                SELECT r.ReservationID as BookingID, 
                       r.ReservationDate as BookingDate, 
                       u.Login as CustomerLogin, 
                       t.TableNumber, 
                       t.Capacity, 
                       r.Status,
                       r.GuestCount,
                       u.UserID,
                       COALESCE(o.OrderID, 0) as OrderID
                FROM Reservations r
                JOIN Users u ON r.UserID = u.UserID
                JOIN Tables t ON r.TableID = t.TableID
                LEFT JOIN Orders o ON r.ReservationID = o.ReservationID
                ORDER BY r.ReservationDate DESC";

            return db.Query<BookingDTO>(sql).ToList();
        }

        public void DeleteReservation(int id)
        {
            using var db = CreateConnection();
            db.Open();
            using var transaction = db.BeginTransaction();

            try
            {
                var orderId = db.QueryFirstOrDefault<int?>(
                    "SELECT OrderID FROM Orders WHERE ReservationID = @id",
                    new { id }, transaction);

                db.Execute(
                    "UPDATE Reservations SET Status = 'Отменена' WHERE ReservationID = @id",
                    new { id }, transaction);
                if (orderId.HasValue && orderId > 0)
                {
                    db.Execute(
                        "DELETE FROM OrderItems WHERE OrderID = @orderId",
                        new { orderId = orderId.Value }, transaction);

                    db.Execute(
                        "DELETE FROM Orders WHERE OrderID = @orderId",
                        new { orderId = orderId.Value }, transaction);
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Ошибка при удалении бронирования: {ex.Message}");
            }
        }

        public int CreateReservationWithOrder(Reservation reservation, List<OrderItem> orderItems = null)
        {
            using var db = CreateConnection();
            db.Open();
            using var transaction = db.BeginTransaction();

            try
            {
                const string insertReservationSql = @"
                    INSERT INTO Reservations (UserID, TableID, ReservationDate, GuestCount, Status) 
                    VALUES (@UserID, @TableID, @ReservationDate, @GuestCount, 'Подтверждена') 
                    RETURNING ReservationID";

                var reservationId = db.ExecuteScalar<int>(insertReservationSql, reservation, transaction);


                const string insertOrderSql = @"
                    INSERT INTO Orders (ReservationID, UserID, TableID, Status, CreatedAt) 
                    VALUES (@ReservationID, @UserID, @TableID, 'Заказан', @CreatedAt) 
                    RETURNING OrderID";

                var orderId = db.ExecuteScalar<int>(insertOrderSql, new
                {
                    ReservationID = reservationId,
                    reservation.UserID,
                    reservation.TableID,
                    CreatedAt = DateTime.Now
                }, transaction);

                if (orderItems != null && orderItems.Any())
                {
                    foreach (var item in orderItems)
                    {
                        var price = db.ExecuteScalar<decimal>(
                            "SELECT Price FROM Menu WHERE ItemID = @ItemID",
                            new { item.ItemID }, transaction);

                        db.Execute(
                            @"INSERT INTO OrderItems (OrderID, ItemID, Quantity, Subtotal) 
                              VALUES (@OrderID, @ItemID, @Quantity, @Subtotal)",
                            new
                            {
                                OrderID = orderId,
                                item.ItemID,
                                item.Quantity,
                                Subtotal = price * item.Quantity
                            }, transaction);
                    }
                }

                transaction.Commit();
                return reservationId;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Ошибка при создании бронирования: {ex.Message}");
            }
        }

        public void UpdateReservationWithOrder(Reservation reservation, List<OrderItem> orderItems = null)
        {
            using var db = CreateConnection();
            db.Open();
            using var transaction = db.BeginTransaction();

            try
            {
                const string updateReservationSql = @"
                    UPDATE Reservations 
                    SET UserID = @UserID, 
                        TableID = @TableID, 
                        ReservationDate = @ReservationDate, 
                        GuestCount = @GuestCount,
                        Status = @Status
                    WHERE ReservationID = @ReservationID";

                db.Execute(updateReservationSql, reservation, transaction);

                var orderId = GetOrCreateOrderIdByReservation(reservation.ReservationID, transaction);
                db.Execute(
                    "DELETE FROM OrderItems WHERE OrderID = @OrderID",
                    new { OrderID = orderId }, transaction);

                if (orderItems != null && orderItems.Any())
                {
                    foreach (var item in orderItems)
                    {
                        // Получаем цену блюда
                        var price = db.ExecuteScalar<decimal>(
                            "SELECT Price FROM Menu WHERE ItemID = @ItemID",
                            new { item.ItemID }, transaction);

                        // Добавляем в OrderItems
                        db.Execute(
                            @"INSERT INTO OrderItems (OrderID, ItemID, Quantity, Subtotal) 
                              VALUES (@OrderID, @ItemID, @Quantity, @Subtotal)",
                            new
                            {
                                OrderID = orderId,
                                item.ItemID,
                                item.Quantity,
                                Subtotal = price * item.Quantity
                            }, transaction);
                    }
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Ошибка при обновлении бронирования: {ex.Message}");
            }
        }
        public List<OrderItemDetailDTO> GetOrderItemsByReservation(int reservationId)
        {
            using var db = CreateConnection();
            const string sql = @"
                SELECT oi.OrderItemID, 
                       oi.ItemID, 
                       m.ItemName, 
                       oi.Quantity, 
                       m.Price, 
                       oi.Subtotal
                FROM OrderItems oi
                JOIN Menu m ON oi.ItemID = m.ItemID
                JOIN Orders o ON oi.OrderID = o.OrderID
                WHERE o.ReservationID = @reservationId
                ORDER BY oi.OrderItemID";

            return db.Query<OrderItemDetailDTO>(sql, new { reservationId }).ToList();
        }
        public int GetOrCreateOrderIdByReservation(int reservationId, IDbTransaction transaction = null)
        {
            var db = transaction?.Connection;

            if (db == null)
            {
                db = CreateConnection();
                db.Open();
            }

            try
            {
                const string sqlCheck = @"
                    SELECT OrderID 
                    FROM Orders 
                    WHERE ReservationID = @reservationId
                    LIMIT 1";

                var existingId = db.QueryFirstOrDefault<int?>(sqlCheck, new { reservationId }, transaction);

                if (existingId.HasValue && existingId > 0)
                    return existingId.Value;

                const string getReservationSql = @"
                    SELECT UserID, TableID, ReservationDate 
                    FROM Reservations 
                    WHERE ReservationID = @reservationId";

                var reservation = db.QueryFirstOrDefault<Reservation>(getReservationSql, new { reservationId }, transaction);

                if (reservation == null)
                    throw new Exception("Бронирование не найдено");

                const string sqlCreate = @"
                    INSERT INTO Orders (ReservationID, UserID, TableID, Status, CreatedAt)
                    VALUES (@reservationId, @UserID, @TableID, 'Заказан', @CreatedAt)
                    RETURNING OrderID";

                return db.ExecuteScalar<int>(sqlCreate, new
                {
                    reservationId,
                    reservation.UserID,
                    reservation.TableID,
                    CreatedAt = DateTime.Now
                }, transaction);
            }
            finally
            {
                if (transaction == null && db.State == ConnectionState.Open)
                {
                    db.Close();
                }
            }
        }
        public List<Reservation> GetReservationById(int reservationId)
        {
            using var db = CreateConnection();
            const string sql = @"
                SELECT * FROM Reservations WHERE ReservationID = @reservationId";
            return db.Query<Reservation>(sql, new { reservationId }).ToList();
        }

        public List<OrderDTO> GetActiveOrdersByTable(int tableId)
        {
            using var db = CreateConnection();
            const string sql = @"
                SELECT o.OrderID, 
                       o.CreatedAt, 
                       u.Login as CustomerLogin, 
                       o.Status, 
                       COALESCE(SUM(oi.Subtotal), 0) as TotalAmount, 
                       t.TableNumber,
                       r.ReservationID
                FROM Orders o
                JOIN Users u ON o.UserID = u.UserID
                JOIN Tables t ON o.TableID = t.TableID
                LEFT JOIN Reservations r ON o.ReservationID = r.ReservationID
                LEFT JOIN OrderItems oi ON o.OrderID = oi.OrderID
                WHERE o.TableID = @tableId 
                AND o.Status IN ('Заказан', 'Готовится', 'Подается')
                GROUP BY o.OrderID, u.Login, o.Status, o.CreatedAt, t.TableNumber, r.ReservationID
                ORDER BY o.CreatedAt DESC";

            return db.Query<OrderDTO>(sql, new { tableId }).ToList();
        }

        public List<TableStatusDTO> GetActiveTables()
        {
            using var db = CreateConnection();
            const string sql = @"
                SELECT DISTINCT 
                    t.TableID,
                    t.TableNumber,
                    t.Capacity,
                    COUNT(o.OrderID) as ActiveOrderCount,
                    MAX(o.CreatedAt) as LastOrderTime,  // Используем CreatedAt вместо OrderDate
                    STRING_AGG(DISTINCT o.Status, ', ') as OrderStatuses
                FROM Tables t
                LEFT JOIN Orders o ON t.TableID = o.TableID 
                    AND o.Status IN ('Заказан', 'Готовится', 'Подается')
                GROUP BY t.TableID, t.TableNumber, t.Capacity
                ORDER BY t.TableNumber";

            return db.Query<TableStatusDTO>(sql).ToList();
        }

        public int CreateQuickOrder(int userId, int tableId, int? waiterId = null)
        {
            using var db = CreateConnection();
            db.Open();
            using var transaction = db.BeginTransaction();

            try
            {
                const string sql = @"
            INSERT INTO Orders (UserID, TableID, WaiterID, Status, CreatedAt) 
            VALUES (@userId, @tableId, @waiterId, 'Заказан', @createdAt) 
            RETURNING OrderID";

                var orderId = db.ExecuteScalar<int>(sql, new
                {
                    userId,
                    tableId,
                    waiterId,
                    createdAt = DateTime.Now
                }, transaction);

                transaction.Commit();
                return orderId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void AddItemToExistingOrder(int orderId, int itemId, int quantity)
        {
            using var db = CreateConnection();

            var price = db.ExecuteScalar<decimal>(
                "SELECT Price FROM Menu WHERE ItemID = @itemId",
                new { itemId });

            
            var existingItem = db.QueryFirstOrDefault<OrderItem>(
                "SELECT * FROM OrderItems WHERE OrderID = @orderId AND ItemID = @itemId",
                new { orderId, itemId });

            if (existingItem != null)
            {
                db.Execute(
                    @"UPDATE OrderItems 
              SET Quantity = Quantity + @quantity, 
                  Subtotal = Subtotal + (@price * @quantity)
              WHERE OrderID = @orderId AND ItemID = @itemId",
                    new { orderId, itemId, quantity, price });
            }
            else
            {
                db.Execute(
                    @"INSERT INTO OrderItems (OrderID, ItemID, Quantity, Subtotal) 
              VALUES (@orderId, @itemId, @quantity, @subtotal)",
                    new
                    {
                        orderId,
                        itemId,
                        quantity,
                        Subtotal = price * quantity
                    });
            }
        }
        public void UpdateOrderStatus(int orderId, string status)
        {
            using var db = CreateConnection();
            db.Execute(
                "UPDATE Orders SET Status = @status WHERE OrderID = @orderId",
                new { orderId, status });
        }

        public void CloseOrder(int orderId)
        {
            using var db = CreateConnection();
            db.Open();
            using var transaction = db.BeginTransaction();

            try
            {
                // Рассчитываем итоговую сумму из OrderItems
                var totalAmount = db.ExecuteScalar<decimal>(
                    "SELECT COALESCE(SUM(Subtotal), 0) FROM OrderItems WHERE OrderID = @orderId",
                    new { orderId }, transaction);

                // Обновляем только статус заказа (без ClosedAt)
                db.Execute(
                    @"UPDATE Orders 
              SET Status = 'Оплачен'
              WHERE OrderID = @orderId",
                    new { orderId }, transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public List<OrderItemDetailDTO> GetOrderItems(int orderId)
        {
            using var db = CreateConnection();
            const string sql = @"
                SELECT oi.OrderItemID, 
                       oi.ItemID, 
                       m.ItemName, 
                       oi.Quantity, 
                       m.Price, 
                       oi.Subtotal
                FROM OrderItems oi
                JOIN Menu m ON oi.ItemID = m.ItemID
                WHERE oi.OrderID = @orderId
                ORDER BY oi.OrderItemID";

            return db.Query<OrderItemDetailDTO>(sql, new { orderId }).ToList();
        }

        public void DeleteOrderItem(int orderItemId)
        {
            using var db = CreateConnection();
            db.Execute("DELETE FROM OrderItems WHERE OrderItemID = @orderItemId",
                       new { orderItemId });
        }
        public List<User> GetAllClients()
        {
            using var db = CreateConnection();
            return db.Query<User>("SELECT * FROM Users WHERE RoleID = 4 ORDER BY Login").ToList();
        }


    }
}
