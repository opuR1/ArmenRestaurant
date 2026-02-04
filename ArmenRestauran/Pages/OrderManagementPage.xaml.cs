using ArmenRestauran.Models;
using ArmenRestauran.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArmenRestauran.Pages
{
    public partial class OrderManagementPage : Page
    {
        private DatabaseService _db = new DatabaseService();
        private List<BookingDTO> _allReservations;

        public OrderManagementPage()
        {
            InitializeComponent();
            RefreshData();
        }

        private void RefreshData()
        {
            try
            {
                _allReservations = _db.GetAllBookings();
                DGridBookings.ItemsSource = _allReservations;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            if (_allReservations == null || TBoxSearch == null || CbStatusFilter == null) return;

            var filtered = _allReservations.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(TBoxSearch.Text))
                filtered = filtered.Where(r => r.CustomerLogin.ToLower().Contains(TBoxSearch.Text.ToLower()));

            if (CbStatusFilter.SelectedIndex > 0)
            {
                string status = (CbStatusFilter.SelectedItem as ComboBoxItem).Content.ToString();
                filtered = filtered.Where(r => r.Status == status);
            }

            if (DpFilter != null && DpFilter.SelectedDate.HasValue)
                filtered = filtered.Where(r => r.BookingDate.Date == DpFilter.SelectedDate.Value.Date);

            DGridBookings.ItemsSource = filtered.ToList();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).DataContext is BookingDTO selected)
            {
                if (MessageBox.Show($"Удалить бронь клиента {selected.CustomerLogin}?", "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _db.DeleteReservation(selected.BookingID);
                    RefreshData();
                }
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).DataContext is BookingDTO selected)
            {
                NavigationService.Navigate(new ReservationEditPage(selected));
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => NavigationService.GoBack();

        private void BtnAddBooking_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ReservationEditPage(null));
        }
    }
}