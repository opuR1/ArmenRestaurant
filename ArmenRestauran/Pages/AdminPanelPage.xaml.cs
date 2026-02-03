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
    /// Логика взаимодействия для AdminPanelPage.xaml
    /// </summary>
    public partial class AdminPanelPage : Page
    {
        public AdminPanelPage()
        {
            InitializeComponent();
        }
        private void BtnMenu_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new MenuManagementPage());
        private void BtnUsers_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new UserManagementPage());
        private void BtnIngredients_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new IngredientsManagementPage());
    }
}
