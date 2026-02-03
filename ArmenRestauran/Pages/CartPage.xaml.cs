using ArmenRestauran.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ArmenRestauran.Pages
{
    public partial class CartPage : Page
    {
        private DatabaseService _db = new DatabaseService();

        public CartPage()
        {
            InitializeComponent();

        }

        private void Page_Loaded(object sender, RoutedEventArgs e) => RefreshCart();

        private void RefreshCart()
        {
            LBoxCart.ItemsSource = null;
            LBoxCart.ItemsSource = CartService.Items;
            TblTotalSum.Text = $"Итого: {CartService.Items.Sum(x => x.Total)} ₽";
        }

        private void BtnPlus_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).Tag as CartService.CartItem;
            item.Quantity++;
            RefreshCart();
        }

        private void BtnMinus_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).Tag as CartService.CartItem;
            if (item.Quantity > 1) item.Quantity--;
            else CartService.Items.Remove(item);
            RefreshCart();
        }

        private void Date_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (DpOrderDate.SelectedDate.HasValue)
            {
                DateTime selectedDate = DpOrderDate.SelectedDate.Value;
                if (selectedDate < DateTime.Today)
                {
                    MessageBox.Show("Нельзя забронировать стол на прошедшую дату!");
                    DpOrderDate.SelectedDate = DateTime.Today;
                    return;
                }
                try
                {
                    
                    var availableTables = _db.GetAvailableTables(selectedDate);
                    CbTables.ItemsSource = availableTables;
                    if (availableTables.Count == 0) MessageBox.Show("На эту дату нет свободных столов.");
                    else CbTables.SelectedIndex = 0;
                }
                catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
            }
        }

        private void BtnOrder_Click(object sender, RoutedEventArgs e)
        {
            if (CartService.Items.Count == 0 || CbTables.SelectedItem == null ||
                !DpOrderDate.SelectedDate.HasValue || CbOrderTime.SelectedItem == null)
            {
                MessageBox.Show("Заполните все данные (блюда, дату, время и стол)!");
                return;
            }

            try
            {
                
                DateTime baseDate = DpOrderDate.SelectedDate.Value;
                TimeSpan time = TimeSpan.Parse(CbOrderTime.SelectedItem.ToString());
                DateTime fullResDate = baseDate.Date.Add(time);

                if (fullResDate < DateTime.Now)
                {
                    MessageBox.Show("Выбранное время уже прошло!");
                    return;
                }

                
                int userId = Convert.ToInt32(AuthService.CurrentUser.UserID);
                int tableId = Convert.ToInt32(CbTables.SelectedValue);
                int guests = Convert.ToInt32(SldGuests.Value);

                
                _db.CreateFullOrder(userId, tableId, fullResDate, guests, CartService.Items);

                MessageBox.Show($"Заказ успешно оформлен на {fullResDate:f}!");
                CartService.Items.Clear();
                NavigationService.Navigate(new CategoriesPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка оформления: {ex.Message}");
            }
        }
    }
}