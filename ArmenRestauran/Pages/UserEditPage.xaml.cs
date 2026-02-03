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
    /// Логика взаимодействия для UserEditPage.xaml
    /// </summary>
    public partial class UserEditPage : Page
    {
        private User _user;
        private DatabaseService _db = new DatabaseService();

        public UserEditPage(User selectedUser)
        {
            InitializeComponent();
            _user = selectedUser;

            TBoxLogin.Text = _user.Login;
            TBoxFirstName.Text = _user.FirstName;
            TBoxLastName.Text = _user.LastName;
            TBoxSurName.Text = _user.SurName;


            var roles = new[] {
            new { ID = 1, Name = "Администратор" },
            new { ID = 2, Name = "Официант" },
            new { ID = 3, Name = "Менеджер" },
            new { ID = 4, Name = "Клиент" }
        };
            CbRoles.ItemsSource = roles;
            CbRoles.SelectedValue = _user.RoleID;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _user.Login = TBoxLogin.Text;
                _user.FirstName = TBoxFirstName.Text;
                _user.LastName = TBoxLastName.Text;
                _user.SurName = TBoxSurName.Text;
                _user.RoleID = (int)CbRoles.SelectedValue;
                if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    _user.Password = Hash.HashPassword(PasswordBox.Password);
                }
                _db.UpdateUser(_user);
                MessageBox.Show("Данные пользователя обновлены!");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => NavigationService.GoBack();
    }
}
