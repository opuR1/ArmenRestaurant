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
    public partial class ClientSelectDialog : Window
    {
        private DatabaseService _db = new DatabaseService();
        public int SelectedClientId { get; private set; }

        public ClientSelectDialog()
        {
            InitializeComponent();
            LoadClients();
        }

        private void LoadClients()
        {
            try
            {
                var clients = _db.GetAllClients();
                LbClients.ItemsSource = clients;

                if (clients.Any())
                    LbClients.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки клиентов: {ex.Message}");
            }
        }

        private void BtnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (LbClients.SelectedItem is User selectedClient)
            {
                SelectedClientId = selectedClient.UserID;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Выберите клиента из списка");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnNewClient_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Для создания нового клиента воспользуйтесь регистрацией в системе");
        }
    }
}