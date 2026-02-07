using ArmenRestauran.Models;
using ArmenRestauran.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ModelMenuItem = ArmenRestauran.Models.MenuItem;

namespace ArmenRestauran.Pages
{
    public partial class WaiterPage : Page
    {
        private DatabaseService _db = new DatabaseService();
        private int? _selectedTableId = null;
        private int? _selectedOrderId = null;
        private List<OrderItem> _orderItems = new List<OrderItem>();

        public WaiterPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var tables = _db.GetTables();
                CbTables.ItemsSource = tables;

                var menuItems = _db.GetMenu();
                CbMenu.ItemsSource = menuItems;

                SetOrderButtonsVisibility(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void CbTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbTables.SelectedItem is Table selectedTable)
            {
                _selectedTableId = selectedTable.TableID;
                TbTableInfo.Text = $"Стол №{selectedTable.TableNumber}";

                LoadTableOrders(selectedTable.TableID);
            }
            else
            {
                ClearOrderDetails();
                SetOrderButtonsVisibility(false);
            }
        }

        private void LoadTableOrders(int tableId)
        {
            try
            {
                var orders = _db.GetActiveOrdersByTable(tableId);

                if (orders.Any())
                {
                    var order = orders.First();
                    _selectedOrderId = order.OrderID;

                    TbOrderId.Text = $"Заказ #{order.OrderID}";
                    TbOrderStatus.Text = $"Статус: {order.Status}";

                    LoadOrderItems(order.OrderID);
                    SetOrderButtonsVisibility(true);
                }
                else
                {
                    _selectedOrderId = null;
                    TbOrderId.Text = "Нет активных заказов";
                    TbOrderStatus.Text = "";
                    DGridOrderItems.ItemsSource = null;
                    TbTotal.Text = "0";
                    SetOrderButtonsVisibility(false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}");
            }
        }

        private void LoadOrderItems(int orderId)
        {
            try
            {
                var items = _db.GetOrderItems(orderId);
                DGridOrderItems.ItemsSource = items;

                decimal total = items.Sum(i => i.Subtotal);
                TbTotal.Text = total.ToString("C");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки блюд: {ex.Message}");
            }
        }

        private void BtnNewOrder_Click(object sender, RoutedEventArgs e)
        {
            if (!_selectedTableId.HasValue)
            {
                MessageBox.Show("Выберите стол для создания заказа");
                return;
            }

            var dialog = new ClientSelectDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    int orderId = _db.CreateQuickOrder(dialog.SelectedClientId, _selectedTableId.Value);
                    MessageBox.Show($"Заказ #{orderId} создан");

                    LoadTableOrders(_selectedTableId.Value);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка создания заказа: {ex.Message}");
                }
            }
        }

        private void BtnAddItem_Click(object sender, RoutedEventArgs e)
        {
            if (!_selectedOrderId.HasValue)
            {
                MessageBox.Show("Сначала создайте или выберите заказ");
                return;
            }

            if (CbMenu.SelectedItem is ModelMenuItem selectedMenu && int.TryParse(TBoxQty.Text, out int qty) && qty > 0)
            {
                try
                {
                    _db.AddItemToExistingOrder(_selectedOrderId.Value, selectedMenu.ItemID, qty);
                    LoadOrderItems(_selectedOrderId.Value);
                    TBoxQty.Text = "1";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Выберите блюдо и введите количество");
            }
        }

        private void BtnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (!_selectedOrderId.HasValue) return;

            if ((sender as Button).DataContext is OrderItemDetailDTO item)
            {
                try
                {
                    _db.DeleteOrderItem(item.OrderItemID);
                    LoadOrderItems(_selectedOrderId.Value);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }

        private void BtnReady_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrderId.HasValue)
            {
                DecreaseIngredientsForOrder(_selectedOrderId.Value);

                UpdateOrderStatus("Готовится");
            }
        }
        private void DecreaseIngredientsForOrder(int orderId)
        {
            try
            {
                var orderItems = _db.GetOrderItems(orderId);

                foreach (var item in orderItems)
                {
                    _db.DecreaseStock(item.ItemID, item.Quantity);
                }

                MessageBox.Show("Ингредиенты списаны со склада");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка списания: {ex.Message}");
                throw;
            }
        }

        private void BtnServed_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrderId.HasValue)
                UpdateOrderStatus("Подано");
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrderId.HasValue)
            {
                if (MessageBox.Show("Закрыть заказ?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        _db.CloseOrder(_selectedOrderId.Value);
                        MessageBox.Show("Заказ закрыт");

                        if (_selectedTableId.HasValue)
                            LoadTableOrders(_selectedTableId.Value);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}");
                    }
                }
            }
        }

        private void UpdateOrderStatus(string status)
        {
            try
            {
                _db.UpdateOrderStatus(_selectedOrderId.Value, status);
                TbOrderStatus.Text = $"Статус: {status}";
                MessageBox.Show($"Статус обновлен на: {status}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void SetOrderButtonsVisibility(bool isVisible)
        {
            BtnReady.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            BtnServed.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            BtnClose.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ClearOrderDetails()
        {
            TbTableInfo.Text = "";
            TbOrderId.Text = "";
            TbOrderStatus.Text = "";
            DGridOrderItems.ItemsSource = null;
            TbTotal.Text = "0";
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }
}