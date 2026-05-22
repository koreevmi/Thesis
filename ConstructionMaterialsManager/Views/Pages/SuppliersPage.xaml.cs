using ConstructionMaterialsManager.Models;
using ConstructionMaterialsManager.Services;
using ConstructionMaterialsManager.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ConstructionMaterialsManager.Views.Pages
{
    public partial class SuppliersPage : UserControl
    {
        private readonly IDatabaseService _databaseService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventAggregator _eventAggregator;
        private ObservableCollection<Supplier> _suppliers = new ObservableCollection<Supplier>();
        private List<Supplier> _allSuppliers = new List<Supplier>();

        public SuppliersPage(IDatabaseService databaseService, IServiceProvider serviceProvider, IEventAggregator eventAggregator)
        {
            InitializeComponent();

            try
            {
                if (databaseService == null) throw new ArgumentNullException(nameof(databaseService));
                if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
                if (eventAggregator == null) throw new ArgumentNullException(nameof(eventAggregator));

                _databaseService = databaseService;
                _serviceProvider = serviceProvider;
                _eventAggregator = eventAggregator;

                // Подписка на события изменения данных
                _eventAggregator.Subscribe<SupplierChangedMessage>(OnSupplierChanged);

                SuppliersDataGrid.ItemsSource = _suppliers;
                LoadSuppliers();
                UpdateUI();

                Unloaded += SuppliersPage_Unloaded;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации страницы: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SuppliersPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _eventAggregator.Unsubscribe<SupplierChangedMessage>(OnSupplierChanged);
        }

        #region Обработчики событий

        private void OnSupplierChanged(SupplierChangedMessage msg)
        {
            Application.Current.Dispatcher.Invoke(() => LoadSuppliers());
        }

        #endregion

        private void UpdateUI()
        {
            try
            {
                bool isGuest = App.CurrentUser?.Role == "Гость";
                AddSupplierBtn.Visibility = isGuest ? Visibility.Collapsed : Visibility.Visible;
                EditSupplierBtn.Visibility = isGuest ? Visibility.Collapsed : Visibility.Visible;
                DeleteSupplierBtn.Visibility = isGuest ? Visibility.Collapsed : Visibility.Visible;
            }
            catch
            {
                // Игнорируем ошибки обновления UI
            }
        }

        private void LoadSuppliers()
        {
            try
            {
                _suppliers.Clear();
                _allSuppliers = _databaseService.GetSuppliers() ?? new List<Supplier>();

                foreach (var supplier in _allSuppliers)
                {
                    if (supplier != null)
                    {
                        _suppliers.Add(supplier);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки поставщиков: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SupplierFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SupplierFilterWatermark.Visibility = string.IsNullOrEmpty(SupplierFilterTextBox.Text)
                    ? Visibility.Visible : Visibility.Collapsed;
                ApplyFilters();
            }
            catch
            {
            }
        }

        private void ApplyFilters()
        {
            try
            {
                // Если нет загруженных поставщиков, выходим
                if (_allSuppliers == null || _allSuppliers.Count == 0)
                {
                    _suppliers.Clear();
                    return;
                }

                // Фильтруем по тексту
                var filteredSuppliers = _allSuppliers.AsQueryable();

                if (!string.IsNullOrEmpty(SupplierFilterTextBox.Text))
                {
                    string filterText = SupplierFilterTextBox.Text.ToLower();
                    filteredSuppliers = filteredSuppliers.Where(s =>
                        s != null && !string.IsNullOrEmpty(s.Name) &&
                        s.Name.ToLower().Contains(filterText));
                }

                // Обновляем список
                _suppliers.Clear();
                foreach (var supplier in filteredSuppliers.ToList())
                {
                    if (supplier != null)
                    {
                        _suppliers.Add(supplier);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddSupplierBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var supplierWindow = _serviceProvider.GetRequiredService<SupplierWindow>();
                if (supplierWindow.ShowDialog() == true)
                {
                    LoadSuppliers();
                }
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка при добавлении поставщика: {msg}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditSupplierBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedSupplier = SuppliersDataGrid.SelectedItem as Supplier;
                if (selectedSupplier != null)
                {
                    var supplierWindow = _serviceProvider.GetRequiredService<SupplierWindow>();
                    supplierWindow.SetSupplier(selectedSupplier);
                    if (supplierWindow.ShowDialog() == true)
                    {
                        LoadSuppliers();
                    }
                }
                else
                {
                    MessageBox.Show("Выберите поставщика для редактирования.");
                }
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка при редактировании поставщика: {msg}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteSupplierBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (App.CurrentUser?.Role != "Администратор")
                {
                    MessageBox.Show("Только администратор может удалять поставщиков.");
                    return;
                }

                var selectedSupplier = SuppliersDataGrid.SelectedItem as Supplier;
                if (selectedSupplier != null)
                {
                    var result = MessageBox.Show("Вы уверены, что хотите удалить этого поставщика?",
                        "Подтверждение", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        var supplierId = selectedSupplier.SupplierId;
                        _databaseService.DeleteSupplier(supplierId);
                        _eventAggregator.Publish(new SupplierChangedMessage(supplierId, ChangeType.Deleted));
                        LoadSuppliers();
                    }
                }
                else
                {
                    MessageBox.Show("Выберите поставщика для удаления.");
                }
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка при удалении поставщика: {msg}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
