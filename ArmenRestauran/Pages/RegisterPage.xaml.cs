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
    /// Логика взаимодействия для RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }
        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string hashPassword = Hash.HashPassword(PbxPassword.Password);
            try
            {
                
                var newUser = new User
                {
                    FirstName = TbxName.Text,
                    LastName = TbxLastName.Text,
                    SurName = TbxSurName.Text,
                    Phone = TbxPhone.Text,
                    Login = TbxLogin.Text,
                    Password = hashPassword 
                };

                DatabaseService db = new DatabaseService();
                db.RegisterClient(newUser);

                MessageBox.Show("Регистрация успешна!", "Успех");
                NavigationService.Navigate(new LoginPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка регистрации: {ex.Message}");
            }
        }
    }
}
