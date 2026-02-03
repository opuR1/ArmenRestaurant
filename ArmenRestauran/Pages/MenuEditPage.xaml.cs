using ArmenRestauran.Models;
using ArmenRestauran.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ModelMenuItem = ArmenRestauran.Models.MenuItem;


namespace ArmenRestauran.Pages
{
    public partial class MenuEditPage : Page
    {
        private ObservableCollection<RecipeDetailDTO> _recipeItems;
        private ModelMenuItem _item;
        private DatabaseService _db = new DatabaseService();

        public MenuEditPage(ModelMenuItem selectedItem)
        {
            InitializeComponent();
            _item = selectedItem;

            TBoxName.Text = _item.ItemName;
            TBoxPrice.Text = _item.Price.ToString();

            CbIngredients.ItemsSource = _db.GetIngredients();

            RefreshRecipe();
        }

        private void RefreshRecipe()
        {
            var list = _db.GetRecipeDetails(_item.ItemID);
            _recipeItems = new ObservableCollection<RecipeDetailDTO>(list);
            DGridRecipe.ItemsSource = _recipeItems;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _item.ItemName = TBoxName.Text;
                _item.Description = TBoxDesc.Text;

                if (decimal.TryParse(TBoxPrice.Text, out decimal price))
                    _item.Price = price;
                else
                    throw new Exception("Введите корректную цену!");

                _db.UpdateMenuItem(_item);
                MessageBox.Show("Изменения сохранены");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }
        private void BtnAddIngredient_Click(object sender, RoutedEventArgs e)
        {
            if (CbIngredients.SelectedItem is Ingredient ing && float.TryParse(TBoxQty.Text, out float qty))
            {
                _db.AddRecipeItem(_item.ItemID, ing.IngredientID, qty);
                RefreshRecipe();
                TBoxQty.Clear();
            }
            else MessageBox.Show("Выберите ингредиент и укажите количество");
        }
        private void BtnDeleteIngredient_Click(object sender, RoutedEventArgs e)
        {
            var row = (sender as Button).DataContext as RecipeDetailDTO;
            if (row != null)
            {
                _db.DeleteRecipeItem(row.RecipeID);
                RefreshRecipe();
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => NavigationService.GoBack();
    }
}