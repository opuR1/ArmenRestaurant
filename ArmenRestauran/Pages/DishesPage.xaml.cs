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
            // Явно указываем пространство имен вашей модели, чтобы не путать с системным MenuItem
            var button = sender as Button;
            if (button == null) return;

            // Замените "Models.MenuItem" на ваше реальное пространство имен (например, ArmenRestauran.Models.MenuItem)
            var dish = button.DataContext as ArmenRestauran.Models.MenuItem;

            if (dish == null)
            {
                MessageBox.Show("Ошибка: данные блюда не найдены.");
                return;
            }

            // Проверяем наличие товара в корзине
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
