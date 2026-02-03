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
using ModelMenuItem = ArmenRestauran.Models.MenuItem;



namespace ArmenRestauran.Pages
{
    /// <summary>
    /// Логика взаимодействия для MenuManagementPage.xaml
    /// </summary>
    public partial class MenuManagementPage : Page
    {
        private DatabaseService _db = new DatabaseService();

        public MenuManagementPage() => InitializeComponent();

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LBoxCategories.ItemsSource = _db.GetCategories();
        }

        private void Category_Selected(object sender, SelectionChangedEventArgs e)
        {
            if (LBoxCategories.SelectedItem is Category selected)
            {
                TblCategoryTitle.Text = selected.CategoryName.ToUpper();
                RefreshMenu();
            }
        }

        private void RefreshMenu()
        {
            var selected = LBoxCategories.SelectedItem as Category;
            if (selected != null)
            {
                DGridMenu.ItemsSource = _db.GetProductsByCategory(selected.CategoryID);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)(sender as Button).Tag;
            if (MessageBox.Show("Удалить это блюдо?", "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _db.DeleteProduct(id);
                RefreshMenu();
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (LBoxCategories.SelectedItem is Category selected)
            {
                NavigationService.Navigate(new MenuEditPage(null, selected.CategoryID));
            }
            else
            {
                MessageBox.Show("Сначала выберите категорию слева!");
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new AdminPanelPage());

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as ModelMenuItem;
            if (item != null)
            {
                NavigationService.Navigate(new MenuEditPage(item));
            }
        }
    }
}
