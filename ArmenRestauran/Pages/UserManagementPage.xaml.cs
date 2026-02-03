using ArmenRestauran.Models;
using ArmenRestauran.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArmenRestauran.Pages
{
    /// <summary>
    /// Логика взаимодействия для UserManagementPage.xaml
    /// </summary>
    public partial class UserManagementPage : Page
    {
        private List<User> _allUsers;
        private DatabaseService _db = new DatabaseService();

        public UserManagementPage() => InitializeComponent();

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _allUsers = _db.GetAllUsers();
            DGridUsers.ItemsSource = _allUsers;
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            var filtered = _allUsers.Where(u =>
                (string.IsNullOrEmpty(TBoxSearch.Text) ||
                 u.FirstName.ToLower().Contains(TBoxSearch.Text.ToLower()) ||
                 u.LastName.ToLower().Contains(TBoxSearch.Text.ToLower()) ||
                 (u.SurName != null && u.SurName.ToLower().Contains(TBoxSearch.Text.ToLower())) ||
                 u.Login.ToLower().Contains(TBoxSearch.Text.ToLower())) &&
                (CbRoleFilter.SelectedIndex <= 0 || u.RoleName == (CbRoleFilter.SelectedItem as ComboBoxItem).Content.ToString())).ToList();
            DGridUsers.ItemsSource = filtered;
}

        private void DGridUsers_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DGridUsers.SelectedItem is User u) NavigationService.Navigate(new UserEditPage(u));
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e) => NavigationService.GoBack();
    }
}
