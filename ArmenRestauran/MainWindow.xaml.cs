using ArmenRestauran.Pages;
using ArmenRestauran.Services;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArmenRestauran
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new LoginPage());

            MainFrame.Navigated += MainFrame_Navigated;
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            var currentPage = e.Content;

            // Смена заголовка под текущее окно
            if (e.Content is Page page)
            {
                this.Title = $"Араратская долина - {page.Title}";
            }

            if (currentPage is LoginPage || currentPage is RegisterPage)
            {
                BtnBack.Visibility = Visibility.Collapsed;
            }
            else
            {
                
                BtnBack.Visibility = Visibility.Visible;
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            
            if (MainFrame.Content is MenuPage menuPage && menuPage.ChildFrame.CanGoBack)
            {
                menuPage.ChildFrame.GoBack();
            }
            
            else if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }
    }
}