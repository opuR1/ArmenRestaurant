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
    public partial class DishesPage : Page
    {
        private int _categoryId;
        private DatabaseService _db = new DatabaseService();

        public DishesPage(int categoryId)
        {
            InitializeComponent();
            _categoryId = categoryId;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void RefreshData()
        {
            var dishes = _db.GetMenu().Where(d => d.CategoryID == _categoryId).ToList();
            IcDishes.ItemsSource = dishes;
        }

        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var dish = button.DataContext as ArmenRestauran.Models.MenuItem;

            if (dish == null)
            {
                MessageBox.Show("Ошибка: данные блюда не найдены.");
                return;
            }

            var existing = CartService.Items.FirstOrDefault(x => x.Product.ItemID == dish.ItemID);

            if (existing != null)
            {
                existing.Quantity++;
            }
            else
            {
                CartService.Items.Add(new CartService.CartItem
                {
                    Product = dish,
                    Quantity = 1
                });
            }

            MessageBox.Show($"{dish.ItemName} добавлен в корзину!", "Успех");
        }
    }
}
