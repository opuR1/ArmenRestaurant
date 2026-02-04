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
    /// Логика взаимодействия для MenuPage.xaml
    /// </summary>
    public partial class MenuPage : Page
    {
        private DatabaseService _db = new DatabaseService();

        public MenuPage()
        {
            InitializeComponent();
            LoadCategories();

            switch (AuthService.CurrentUser.RoleName)
            {
                case "Администратор":
                    ChildFrame.Navigate(new AdminPanelPage());
                    AdminPanel.Visibility = Visibility.Visible;
                    ManagerPanel.Visibility = Visibility.Visible;
                    WaiterPanel.Visibility = Visibility.Visible;
                    break;
                case "Менеджер":
                    ChildFrame.Navigate(new OrderManagementPage());
                    ManagerPanel.Visibility = Visibility.Visible;
                    WaiterPanel.Visibility = Visibility.Visible;
                    break;
                case "Клиент":
                    ChildFrame.Navigate(new CategoriesPage());
                    break;
                case "Официант":
                    ChildFrame.Navigate(new WaiterPage());
                    WaiterPanel.Visibility = Visibility.Visible;
                    break;
            }

        }

        private void LoadCategories()
        {
            
            var categories = _db.GetCategories();

            foreach (var cat in categories)
            {
                MenuItem subItem = new MenuItem { Header = cat.CategoryName, Tag = cat.CategoryID };
                subItem.Click += CategoryItem_Click;
                MiCategories.Items.Add(subItem);
            }
        }

        private void BtnMenu_Click(object sender, RoutedEventArgs e)
        {
            
            BtnMenu.ContextMenu.PlacementTarget = BtnMenu;
            BtnMenu.ContextMenu.IsOpen = true;
        }

        private void CategoryItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi)
            {
                int categoryId = (int)mi.Tag;
                ChildFrame.Navigate(new DishesPage(categoryId));
            }
        }
        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            AuthService auth = new AuthService();
            auth.Logout();

            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainFrame.Navigate(new LoginPage());
               
                while (mainWindow.MainFrame.CanGoBack)
                    mainWindow.MainFrame.RemoveBackEntry();
            }
        }


        private void MenuProfile_Click(object sender, RoutedEventArgs e) => ChildFrame.Navigate(new ProfilePage());
        private void MenuOrders_Click(object sender, RoutedEventArgs e) => ChildFrame.Navigate(new OrdersPage());
        private void MenuCart_Click(object sender, RoutedEventArgs e) => ChildFrame.Navigate(new CartPage());

        private void MenuAdmin_Click(object sender, RoutedEventArgs e)
        {
            ChildFrame.Navigate(new AdminPanelPage());
        }

        private void ManagerPanel_Click(object sender, RoutedEventArgs e)
        {
            ChildFrame.Navigate(new OrderManagementPage());
        }

        private void WaiterPanel_Click(object sender, RoutedEventArgs e)
        {
            ChildFrame.Navigate(new WaiterPage());
        }
    }
}
