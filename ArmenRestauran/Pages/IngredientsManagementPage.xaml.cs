using ArmenRestauran.Models;
using ArmenRestauran.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArmenRestauran.Pages
{
    public partial class IngredientsManagementPage : Page
    {
        private DatabaseService _db = new DatabaseService();
        private Ingredient _selectedIngredient = null;

        public IngredientsManagementPage()
        {
            InitializeComponent();
            RefreshData();
        }

        private void RefreshData()
        {
            DGridIngredients.ItemsSource = _db.GetIngredients().OrderBy(i => i.IngredientName).ToList();
        }

        private void DGridIngredients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedIngredient = DGridIngredients.SelectedItem as Ingredient;

            if (_selectedIngredient != null)
            {
                TblHeader.Text = "РЕДАКТИРОВАНИЕ";
                TBoxName.Text = _selectedIngredient.IngredientName;
                TBoxStock.Text = _selectedIngredient.StockQuantity.ToString();
                CbUnits.Text = _selectedIngredient.Unit;
                BtnDelete.Visibility = Visibility.Visible;
            }
            else
            {
                ResetFields();
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TBoxName.Text)) throw new Exception("Введите название!");

                var item = _selectedIngredient ?? new Ingredient();
                item.IngredientName = TBoxName.Text;
                item.Unit = (CbUnits.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "кг";

                if (float.TryParse(TBoxStock.Text, out float stock))
                    item.StockQuantity = stock;
                else
                    throw new Exception("Введите корректное количество");

                if (_selectedIngredient == null)
                    _db.AddIngredient(item);
                else
                    _db.UpdateIngredient(item);

                MessageBox.Show("Готово!");
                RefreshData();
                ResetFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedIngredient != null && MessageBox.Show($"Удалить {_selectedIngredient.IngredientName}?", "Вопрос", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    _db.DeleteIngredient(_selectedIngredient.IngredientID);
                    RefreshData();
                    ResetFields();
                }
                catch
                {
                    MessageBox.Show("Нельзя удалить: ингредиент используется в рецептах!");
                }
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e) => ResetFields();

        private void ResetFields()
        {
            _selectedIngredient = null;
            DGridIngredients.SelectedItem = null;
            TblHeader.Text = "ДОБАВИТЬ НОВЫЙ";
            TBoxName.Clear();
            TBoxStock.Clear();
            CbUnits.SelectedIndex = -1;
            BtnDelete.Visibility = Visibility.Collapsed;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => NavigationService.GoBack();
    }
}