using ArmenRestauran.Models;
using ArmenRestauran.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ArmenRestauran.Pages
{
    public partial class OrdersPage : Page
    {
        private DatabaseService _db = new DatabaseService();

        public OrdersPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                
                int userId = Convert.ToInt32(AuthService.CurrentUser.UserID);

                
                var orders = _db.GetUserOrders(userId);
                LBoxOrders.ItemsSource = orders;

                if (orders.Count == 0)
                {
                    MessageBox.Show("У вас пока нет заказов.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки заказов: " + ex.Message);
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var order = (sender as Button).DataContext as UserOrder;
            if (order == null) return;

            var result = MessageBox.Show("Отменить заказ? Состав блюд будет удален, а столик освобожден.",
                                         "Подтверждение", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _db.CancelAndCleanupOrder(order.OrderID, order.ReservationID);
                    MessageBox.Show("Заказ отменен.");
                    Page_Loaded(null, null); // Перезагрузка списка
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }
    }
}