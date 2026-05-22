using ConstructionMaterialsManager.Models;
using ConstructionMaterialsManager.Services;
using ConstructionMaterialsManager.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ConstructionMaterialsManager.Views.Pages
{
    public partial class DeliveriesPage : UserControl
    {
        private readonly IDatabaseService _databaseService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventAggregator _eventAggregator;
        private ObservableCollection<Delivery> _deliveries = new ObservableCollection<Delivery>();
        private List<Supplier> _suppliers = new List<Supplier>();

        public DeliveriesPage(IDatabaseService databaseService, IServiceProvider serviceProvider, IEventAggregator eventAggregator)
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

                // Инициализируем привязку данных
                DeliveriesDataGrid.ItemsSource = _deliveries;

                // Подписка на события изменения данных
                _eventAggregator.Subscribe<DeliveryChangedMessage>(OnDeliveryChanged);
                _eventAggregator.Subscribe<SupplierChangedMessage>(OnSupplierChanged);
                _eventAggregator.Subscribe<MaterialChangedMessage>(OnMaterialChanged);

                // Загружаем данные
                LoadDeliveries();
                LoadSuppliers();
                UpdateUI();

                Unloaded += DeliveriesPage_Unloaded;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации страницы: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeliveriesPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _eventAggregator.Unsubscribe<DeliveryChangedMessage>(OnDeliveryChanged);
            _eventAggregator.Unsubscribe<SupplierChangedMessage>(OnSupplierChanged);
            _eventAggregator.Unsubscribe<MaterialChangedMessage>(OnMaterialChanged);
        }

        #region Обработчики событий

        private void OnDeliveryChanged(DeliveryChangedMessage msg)
        {
            Application.Current.Dispatcher.Invoke(() => LoadDeliveries());
        }

        private void OnSupplierChanged(SupplierChangedMessage msg)
        {
            Application.Current.Dispatcher.Invoke(() => LoadSuppliers());
        }

        private void OnMaterialChanged(MaterialChangedMessage msg)
        {
            Application.Current.Dispatcher.Invoke(() => LoadDeliveries());
        }

        #endregion

        private void UpdateUI()
        {
            try
            {
                bool isGuest = App.CurrentUser?.Role == "Гость";
                AddDeliveryBtn.Visibility = isGuest ? Visibility.Collapsed : Visibility.Visible;
                EditDeliveryBtn.Visibility = isGuest ? Visibility.Collapsed : Visibility.Visible;
                DeleteDeliveryBtn.Visibility = isGuest ? Visibility.Collapsed : Visibility.Visible;
            }
            catch
            {
                // Игнорируем ошибки обновления UI
            }
        }

        private void LoadDeliveries()
        {
            try
            {
                _deliveries.Clear();
                var deliveries = _databaseService.GetDeliveries();

                if (deliveries != null)
                {
                    foreach (var delivery in deliveries)
                    {
                        if (delivery != null)
                        {
                            _deliveries.Add(delivery);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки поставок: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSuppliers()
        {
            try
            {
                // Очищаем текущий список поставщиков
                _suppliers.Clear();

                // Добавляем элемент "Все поставщики"
                _suppliers.Add(new Supplier { SupplierId = -1, Name = "📋 Все поставщики" });

                // Получаем поставщиков из базы данных
                var suppliers = _databaseService.GetSuppliers();

                if (suppliers != null)
                {
                    foreach (var supplier in suppliers)
                    {
                        if (supplier != null)
                        {
                            _suppliers.Add(supplier);
                        }
                    }
                }

                // Устанавливаем источник данных для ComboBox
                SupplierFilter.ItemsSource = null; // Сначала очищаем
                SupplierFilter.ItemsSource = _suppliers;

                // Устанавливаем выбранным элемент "Все поставщики"
                if (_suppliers.Count > 0)
                {
                    SupplierFilter.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки поставщиков: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeliveryFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                DeliveryFilterWatermark.Visibility = string.IsNullOrEmpty(DeliveryFilterTextBox.Text)
                    ? Visibility.Visible : Visibility.Collapsed;
                ApplyFilters();
            }
            catch
            {
                // Игнорируем ошибки при изменении текста фильтра
            }
        }

        private void SupplierFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ApplyFilters();
            }
            catch
            {
                // Игнорируем ошибки при изменении фильтра поставщиков
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ApplyFilters();
            }
            catch
            {
                // Игнорируем ошибки при изменении даты
            }
        }

        private void ApplyFilters()
        {
            try
            {
                // Получаем все поставки
                var allDeliveries = _databaseService.GetDeliveries();

                if (allDeliveries == null)
                {
                    _deliveries.Clear();
                    return;
                }

                // Фильтруем по тексту
                var filteredDeliveries = allDeliveries.AsQueryable();

                if (!string.IsNullOrEmpty(DeliveryFilterTextBox.Text))
                {
                    string filterText = DeliveryFilterTextBox.Text.ToLower();
                    filteredDeliveries = filteredDeliveries.Where(d =>
                        d != null && d.Material != null && !string.IsNullOrEmpty(d.Material.Name) &&
                        d.Material.Name.ToLower().Contains(filterText));
                }

                // Фильтруем по поставщику
                if (SupplierFilter.SelectedItem is Supplier selectedSupplier &&
                    selectedSupplier.SupplierId != -1)
                {
                    filteredDeliveries = filteredDeliveries.Where(d =>
                        d != null && d.SupplierId == selectedSupplier.SupplierId);
                }

                // Фильтруем по датам
                if (StartDatePicker.SelectedDate != null)
                {
                    DateTime startDate = StartDatePicker.SelectedDate.Value;
                    filteredDeliveries = filteredDeliveries.Where(d =>
                        d != null && d.DeliveryDate >= startDate);
                }

                if (EndDatePicker.SelectedDate != null)
                {
                    DateTime endDate = EndDatePicker.SelectedDate.Value;
                    filteredDeliveries = filteredDeliveries.Where(d =>
                        d != null && d.DeliveryDate <= endDate);
                }

                // Обновляем список
                _deliveries.Clear();
                foreach (var delivery in filteredDeliveries.ToList())
                {
                    if (delivery != null)
                    {
                        _deliveries.Add(delivery);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddDeliveryBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var deliveryWindow = _serviceProvider.GetRequiredService<DeliveryWindow>();
                if (deliveryWindow.ShowDialog() == true)
                {
                    LoadDeliveries();
                }
            }
            catch (Exception ex)
            {
                var addMsg = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка при добавлении поставки: {addMsg}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditDeliveryBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedDelivery = DeliveriesDataGrid.SelectedItem as Delivery;
                if (selectedDelivery != null)
                {
                    var deliveryWindow = _serviceProvider.GetRequiredService<DeliveryWindow>();
                    deliveryWindow.SetDelivery(selectedDelivery);
                    if (deliveryWindow.ShowDialog() == true)
                    {
                        LoadDeliveries();
                    }
                }
                else
                {
                    MessageBox.Show("Выберите поставку для редактирования.");
                }
            }
            catch (Exception ex)
            {
                var editMsg = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка при редактировании поставки: {editMsg}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteDeliveryBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (App.CurrentUser?.Role != "Администратор")
                {
                    MessageBox.Show("Только администратор может удалять поставки.");
                    return;
                }

                var selectedDelivery = DeliveriesDataGrid.SelectedItem as Delivery;
                if (selectedDelivery != null)
                {
                    var result = MessageBox.Show("Вы уверены, что хотите удалить эту поставку?",
                        "Подтверждение", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        var deliveryId = selectedDelivery.DeliveryId;
                        _databaseService.DeleteDelivery(deliveryId);
                        _eventAggregator.Publish(new DeliveryChangedMessage(deliveryId, ChangeType.Deleted));
                        LoadDeliveries();
                    }
                }
                else
                {
                    MessageBox.Show("Выберите поставку для удаления.");
                }
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка при удалении поставки: {msg}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
