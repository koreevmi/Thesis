using ConstructionMaterialsManager.Models;
using ConstructionMaterialsManager.Services;
using ConstructionMaterialsManager.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ConstructionMaterialsManager.Views.Pages
{
    public partial class QualityChecksPage : UserControl
    {
        private readonly IDatabaseService _databaseService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventAggregator _eventAggregator;
        private ObservableCollection<QualityCheck> _qualityChecks = new();
        private List<QualityCheck> _allQualityChecks = new();
        private List<Material> _materials = new();

        public QualityChecksPage(IDatabaseService databaseService, IServiceProvider serviceProvider, IEventAggregator eventAggregator)
        {
            InitializeComponent();

            _databaseService = databaseService;
            _serviceProvider = serviceProvider;
            _eventAggregator = eventAggregator;

            QualityChecksDataGrid.ItemsSource = _qualityChecks;

            _eventAggregator.Subscribe<QualityCheckChangedMessage>(OnQualityCheckChanged);
            _eventAggregator.Subscribe<MaterialChangedMessage>(OnMaterialChanged);

            LoadMaterials();
            LoadQualityChecks();
            UpdateUI();

            Unloaded += QualityChecksPage_Unloaded;
        }

        private void QualityChecksPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _eventAggregator.Unsubscribe<QualityCheckChangedMessage>(OnQualityCheckChanged);
            _eventAggregator.Unsubscribe<MaterialChangedMessage>(OnMaterialChanged);
        }

        private void OnQualityCheckChanged(QualityCheckChangedMessage msg)
        {
            Application.Current.Dispatcher.Invoke(() => LoadQualityChecks());
        }

        private void OnMaterialChanged(MaterialChangedMessage msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LoadMaterials();
                LoadQualityChecks();
            });
        }

        private void UpdateUI()
        {
            bool isGuest = App.CurrentUser?.Role == "Гость";
            AddCheckBtn.Visibility = isGuest ? Visibility.Collapsed : Visibility.Visible;
            EditCheckBtn.Visibility = isGuest ? Visibility.Collapsed : Visibility.Visible;
            DeleteCheckBtn.Visibility = isGuest ? Visibility.Collapsed : Visibility.Visible;
        }

        private void LoadMaterials()
        {
            _materials.Clear();
            _materials.Add(new Material { MaterialId = -1, Name = "Все материалы" });
            var materials = _databaseService.GetMaterials() ?? new List<Material>();
            foreach (var m in materials) _materials.Add(m);

            MaterialFilter.ItemsSource = _materials;
            if (_materials.Count > 0) MaterialFilter.SelectedIndex = 0;
        }

        private void LoadQualityChecks()
        {
            _qualityChecks.Clear();
            _allQualityChecks = _databaseService.GetQualityChecks() ?? new List<QualityCheck>();
            foreach (var qc in _allQualityChecks) _qualityChecks.Add(qc);
            ApplyFilters();
        }

        private void MaterialFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filtered = _allQualityChecks.AsQueryable();

            if (MaterialFilter.SelectedItem is Material selectedMaterial && selectedMaterial.MaterialId != -1)
            {
                filtered = filtered.Where(qc => qc.MaterialId == selectedMaterial.MaterialId);
            }

            if (StatusFilter.SelectedItem is ComboBoxItem selectedStatus && selectedStatus.Content.ToString() != "Все статусы")
            {
                var statusText = selectedStatus.Content.ToString();
                filtered = filtered.Where(qc => qc.Status == statusText);
            }

            _qualityChecks.Clear();
            foreach (var qc in filtered.ToList()) _qualityChecks.Add(qc);
        }

        private void AddCheckBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = _serviceProvider.GetRequiredService<QualityCheckWindow>();
            if (window.ShowDialog() == true) LoadQualityChecks();
        }

        private void EditCheckBtn_Click(object sender, RoutedEventArgs e)
        {
            if (QualityChecksDataGrid.SelectedItem is QualityCheck selected)
            {
                var window = _serviceProvider.GetRequiredService<QualityCheckWindow>();
                window.SetQualityCheck(selected);
                if (window.ShowDialog() == true) LoadQualityChecks();
            }
            else
            {
                MessageBox.Show("Выберите проверку для редактирования.");
            }
        }

        private void DeleteCheckBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (App.CurrentUser?.Role != "Администратор")
                {
                    MessageBox.Show("Только администратор может удалять записи.");
                    return;
                }

                if (QualityChecksDataGrid.SelectedItem is QualityCheck selected)
                {
                    var result = MessageBox.Show("Удалить эту запись о проверке?", "Подтверждение", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        _databaseService.DeleteQualityCheck(selected.QualityCheckId);
                        _eventAggregator.Publish(new QualityCheckChangedMessage(selected.QualityCheckId, ChangeType.Deleted));
                        LoadQualityChecks();
                    }
                }
                else
                {
                    MessageBox.Show("Выберите проверку для удаления.");
                }
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка при удалении проверки: {msg}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
