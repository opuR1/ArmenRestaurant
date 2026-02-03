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
    /// Логика взаимодействия для CategoriesPage.xaml
    /// </summary>
    public partial class CategoriesPage : Page
    {
        public CategoriesPage()
        {
            InitializeComponent();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DatabaseService db = new DatabaseService();
            LBoxCategories.ItemsSource = db.GetCategories();
        }

        private void LBoxCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LBoxCategories.SelectedItem is Category selectedCategory)
            {
                
                NavigationService.Navigate(new DishesPage(selectedCategory.CategoryID));
            }
        }
    }
}
