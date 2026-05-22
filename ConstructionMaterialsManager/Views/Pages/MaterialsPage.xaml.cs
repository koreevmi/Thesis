using ConstructionMaterialsManager.Models;
using ConstructionMaterialsManager.Services;
using ConstructionMaterialsManager.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ConstructionMaterialsManager.Views.Pages
{
    public partial class MaterialsPage : UserControl
    {
        private readonly IDatabaseService _databaseService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventAggregator _eventAggregator;
        private ObservableCollection<Material> _materials = new ObservableCollection<Material>();
        private List<Material> _allMaterials = new List<Material>();
        private List<Supplier> _suppliers = new List<Supplier>();

        public MaterialsPage(IDatabaseService databaseService, IServiceProvider serviceProvider, IEventAggregator eventAggregator)
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

                MaterialsDataGrid.ItemsSource = _materials;
                SupplierFilter.ItemsSource = _suppliers;

                // Подписка на события изменения данных
                _eventAggregator.Subscribe<MaterialChangedMessage>(OnMaterialChanged);
                _eventAggregator.Subscribe<SupplierChangedMessage>(OnSupplierChanged);
                _eventAggregator.Subscribe<DeliveryChangedMessage>(OnDeliveryChanged);

                LoadMaterials();
                LoadSuppliers();
                UpdateUI();

                Unloaded += MaterialsPage_Unloaded;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации страницы: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MaterialsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _eventAggregator.Unsubscribe<MaterialChangedMessage>(OnMaterialChanged);
            _eventAggregator.Unsubscribe<SupplierChangedMessage>(OnSupplierChanged);
            _eventAggregator.Unsubscribe<DeliveryChangedMessage>(OnDeliveryChanged);
        }

        #region Обработчики событий

        private void OnMaterialChanged(MaterialChangedMessage msg)
        {
            Application.Current.Dispatcher.Invoke(() => LoadMaterials());
        }

        private void OnSupplierChanged(SupplierChangedMessage msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LoadSuppliers();
                LoadMaterials();
            });
        }

        private void OnDeliveryChanged(DeliveryChangedMessage msg)
        {
            Application.Current.Dispatcher.Invoke(() => LoadMaterials());
        }

        #endregion

        private void UpdateUI()
        {
            try
            {
                bool isGuest = App.CurrentUser?.Role == "Гость";
                AddMaterialBtn.Visibility = isGuest ? Visibility.Collapsed : Visibility.Visible;
                EditMaterialBtn.Visibility = isGuest ? Visibility.Collapsed : Visibility.Visible;
                DeleteMaterialBtn.Visibility = isGuest ? Visibility.Collapsed : Visibility.Visible;
            }
            catch
            {
            }
        }

        private void LoadMaterials()
        {
            try
            {
                _materials.Clear();
                _allMaterials = _databaseService.GetMaterials() ?? new List<Material>();

                foreach (var material in _allMaterials)
                {
                    if (material != null)
                    {
                        _materials.Add(material);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки материалов: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSuppliers()
        {
            try
            {
                _suppliers.Clear();
                var suppliers = _databaseService.GetSuppliers() ?? new List<Supplier>();

                _suppliers.Add(new Supplier { SupplierId = -1, Name = "📋 Все поставщики" });

                foreach (var supplier in suppliers)
                {
                    if (supplier != null)
                    {
                        _suppliers.Add(supplier);
                    }
                }

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

        private void MaterialFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                MaterialFilterWatermark.Visibility = string.IsNullOrEmpty(MaterialFilterTextBox.Text)
                    ? Visibility.Visible : Visibility.Collapsed;
                ApplyFilters();
            }
            catch
            {
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
            }
        }

        private void ApplyFilters()
        {
            try
            {
                if (_allMaterials == null || _allMaterials.Count == 0)
                {
                    _materials.Clear();
                    return;
                }

                var filteredMaterials = _allMaterials.AsQueryable();

                if (!string.IsNullOrEmpty(MaterialFilterTextBox.Text))
                {
                    string filterText = MaterialFilterTextBox.Text.ToLower();
                    filteredMaterials = filteredMaterials.Where(m =>
                        m != null && !string.IsNullOrEmpty(m.Name) &&
                        m.Name.ToLower().Contains(filterText));
                }

                if (SupplierFilter.SelectedItem is Supplier selectedSupplier &&
                    selectedSupplier.SupplierId != -1)
                {
                    filteredMaterials = filteredMaterials.Where(m =>
                        m != null && m.SupplierId == selectedSupplier.SupplierId);
                }

                _materials.Clear();
                foreach (var material in filteredMaterials.ToList())
                {
                    if (material != null)
                    {
                        _materials.Add(material);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddMaterialBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var materialWindow = _serviceProvider.GetRequiredService<MaterialWindow>();
                if (materialWindow.ShowDialog() == true)
                {
                    LoadMaterials();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении материала: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditMaterialBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedMaterial = MaterialsDataGrid.SelectedItem as Material;
                if (selectedMaterial != null)
                {
                    var materialWindow = _serviceProvider.GetRequiredService<MaterialWindow>();
                    materialWindow.SetMaterial(selectedMaterial);
                    if (materialWindow.ShowDialog() == true)
                    {
                        LoadMaterials();
                    }
                }
                else
                {
                    MessageBox.Show("Выберите материал для редактирования.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании материала: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteMaterialBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (App.CurrentUser?.Role != "Администратор")
                {
                    MessageBox.Show("Только администратор может удалять материалы.");
                    return;
                }

                var selectedMaterial = MaterialsDataGrid.SelectedItem as Material;
                if (selectedMaterial != null)
                {
                    var result = MessageBox.Show("Вы уверены, что хотите удалить этот материал?",
                        "Подтверждение", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        var materialId = selectedMaterial.MaterialId;
                        _databaseService.DeleteMaterial(materialId);
                        _eventAggregator.Publish(new MaterialChangedMessage(materialId, ChangeType.Deleted));
                        LoadMaterials();
                    }
                }
                else
                {
                    MessageBox.Show("Выберите материал для удаления.");
                }
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка при удалении материала: {msg}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculatorBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var calculatorWindow = _serviceProvider.GetRequiredService<MaterialCalculatorWindow>();
                calculatorWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия калькулятора: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
