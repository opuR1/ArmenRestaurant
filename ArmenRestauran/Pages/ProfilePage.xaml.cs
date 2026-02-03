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
    public partial class ProfilePage : Page
    {
        private User _user;
        private DatabaseService _db = new DatabaseService();

        public ProfilePage()
        {
            InitializeComponent();
            _user = AuthService.CurrentUser;

            if (_user != null)
            {
                FillFields();
            }
        }

        private void FillFields()
        {
            TbxFirstName.Text = _user.FirstName;
            TbxLastName.Text = _user.LastName;
            TbxSurName.Text = _user.SurName;
            TbxLogin.Text = _user.Login;

            
            TblRole.Text = _user.RoleID switch
            {
                1 => "АДМИНИСТРАТОР",
                2 => "ОФИЦИАНТ",
                3 => "МЕНЕДЖЕР",
                4 => "КЛИЕНТ",
                _ => "ПОЛЬЗОВАТЕЛЬ"
            };
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            
            TbxFirstName.IsReadOnly = false;
            TbxLastName.IsReadOnly = false;
            TbxSurName.IsReadOnly = false;
            TbxLogin.IsReadOnly = false;

            
            LblPass.Visibility = Visibility.Visible;
            PbxPassword.Visibility = Visibility.Visible;

            BtnEdit.Visibility = Visibility.Collapsed;
            BtnSave.Visibility = Visibility.Visible;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _user.FirstName = TbxFirstName.Text;
                _user.LastName = TbxLastName.Text;
                _user.SurName = TbxSurName.Text;
                _user.Login = TbxLogin.Text;

                
                if (!string.IsNullOrWhiteSpace(PbxPassword.Password))
                {
                    _user.Password = Hash.HashPassword(PbxPassword.Password);
                }

                _db.UpdateUser(_user);

                MessageBox.Show("Данные успешно изменены!");

                
                TbxFirstName.IsReadOnly = TbxLastName.IsReadOnly = TbxSurName.IsReadOnly = TbxLogin.IsReadOnly = true;
                LblPass.Visibility = PbxPassword.Visibility = Visibility.Collapsed;
                BtnEdit.Visibility = Visibility.Visible;
                BtnSave.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}
