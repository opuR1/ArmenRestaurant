using ArmenRestauran.Models;
using ArmenRestauran.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ModelMenuItem = ArmenRestauran.Models.MenuItem;

namespace ArmenRestauran.Pages
{
    public partial class ReservationEditPage : Page
    {
        private Reservation _currentReservation;
        private DatabaseService _db = new DatabaseService();
        private List<OrderItem> _orderItems = new List<OrderItem>();
        private bool _isEditMode = false;

        public ReservationEditPage(BookingDTO selected = null)
        {
            InitializeComponent();
            LoadInitialData();

            if (selected != null && selected.BookingID > 0)
            {
                _isEditMode = true;
                LoadReservationData(selected.BookingID);
            }
            else
            {
                // Новое бронирование
                _currentReservation = new Reservation
                {
                    ReservationDate = DateTime.Now,
                    Status = "Подтверждена"
                };
                DpDate.SelectedDate = DateTime.Now;
            }
        }

        private void LoadInitialData()
        {
            try
            {
                CbTables.ItemsSource = _db.GetTables();
                CbMenu.ItemsSource = _db.GetMenu();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void LoadReservationData(int reservationId)
        {
            try
            {
                var reservations = _db.GetReservationById(reservationId);
                _currentReservation = reservations.FirstOrDefault();

                if (_currentReservation != null)
                {
                    FillFields();
                    LoadOrderItems();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки бронирования: {ex.Message}");
            }
        }

        private void FillFields()
        {
            TBoxUserId.Text = _currentReservation.UserID.ToString();

            if (CbTables.ItemsSource != null)
            {
                var tables = CbTables.ItemsSource as List<Table>;
                var selectedTable = tables?.FirstOrDefault(t => t.TableID == _currentReservation.TableID);
                if (selectedTable != null)
                    CbTables.SelectedItem = selectedTable;
            }

            DpDate.SelectedDate = _currentReservation.ReservationDate;
            TBoxGuests.Text = _currentReservation.GuestCount.ToString();
        }

        private void LoadOrderItems()
        {
            try
            {
                var orderItems = _db.GetOrderItemsByReservation(_currentReservation.ReservationID);
                DGridOrderItems.ItemsSource = orderItems;

                // Конвертируем в список OrderItem для редактирования
                _orderItems = orderItems.Select(oi => new OrderItem
                {
                    ItemID = oi.ItemID,
                    Quantity = oi.Quantity
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки блюд: {ex.Message}");
            }
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CbMenu.SelectedItem is ModelMenuItem selectedMenu && int.TryParse(TBoxQty.Text, out int qty) && qty > 0)
                {
                    // Проверяем, есть ли уже такое блюдо в списке
                    var existingItem = _orderItems.FirstOrDefault(oi => oi.ItemID == selectedMenu.ItemID);

                    if (existingItem != null)
                    {
                        // Увеличиваем количество
                        existingItem.Quantity += qty;
                    }
                    else
                    {
                        // Добавляем новое блюдо
                        _orderItems.Add(new OrderItem
                        {
                            ItemID = selectedMenu.ItemID,
                            Quantity = qty
                        });
                    }

                    RefreshOrderGrid();
                    TBoxQty.Text = "1";
                }
                else
                {
                    MessageBox.Show("Выберите блюдо и введите корректное количество");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void RefreshOrderGrid()
        {
            try
            {
                var menuItems = CbMenu.ItemsSource as List<ModelMenuItem>;
                var orderDetails = new List<OrderItemDetailDTO>();

                foreach (var item in _orderItems)
                {
                    var menuItem = menuItems?.FirstOrDefault(m => m.ItemID == item.ItemID);
                    if (menuItem != null)
                    {
                        orderDetails.Add(new OrderItemDetailDTO
                        {
                            ItemID = item.ItemID,
                            ItemName = menuItem.ItemName,
                            Quantity = item.Quantity,
                            Price = menuItem.Price,
                            Subtotal = menuItem.Price * item.Quantity
                        });
                    }
                }

                DGridOrderItems.ItemsSource = orderDetails;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления списка блюд: {ex.Message}");
            }
        }

        private void BtnRemoveProduct_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).DataContext is OrderItemDetailDTO item)
            {
                var orderItem = _orderItems.FirstOrDefault(oi => oi.ItemID == item.ItemID);
                if (orderItem != null)
                {
                    _orderItems.Remove(orderItem);
                    RefreshOrderGrid();
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (!ValidateInput())
                    return;

                // Создаем/обновляем объект бронирования
                var reservation = new Reservation
                {
                    ReservationID = _isEditMode ? _currentReservation.ReservationID : 0,
                    UserID = int.Parse(TBoxUserId.Text),
                    TableID = (CbTables.SelectedItem as Table)?.TableID ?? 0,
                    ReservationDate = DpDate.SelectedDate ?? DateTime.Now,
                    GuestCount = int.Parse(TBoxGuests.Text),
                    Status = "Подтверждена"
                };

                // Сохраняем в зависимости от режима
                if (_isEditMode)
                {
                    _db.UpdateReservationWithOrder(reservation, _orderItems);
                    MessageBox.Show("Бронирование успешно обновлено!");
                }
                else
                {
                    var reservationId = _db.CreateReservationWithOrder(reservation, _orderItems);
                    MessageBox.Show($"Бронирование #{reservationId} успешно создано!");
                }

                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(TBoxUserId.Text) || !int.TryParse(TBoxUserId.Text, out int userId) || userId <= 0)
            {
                MessageBox.Show("Введите корректный ID клиента");
                return false;
            }

            if (CbTables.SelectedItem == null)
            {
                MessageBox.Show("Выберите стол");
                return false;
            }

            if (!DpDate.SelectedDate.HasValue || DpDate.SelectedDate.Value < DateTime.Now.Date)
            {
                MessageBox.Show("Выберите корректную дату бронирования");
                return false;
            }

            if (string.IsNullOrWhiteSpace(TBoxGuests.Text) || !int.TryParse(TBoxGuests.Text, out int guests) || guests < 1 || guests > 50)
            {
                MessageBox.Show("Введите корректное количество гостей (1-50)");
                return false;
            }

            return true;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => NavigationService.GoBack();
    }
}