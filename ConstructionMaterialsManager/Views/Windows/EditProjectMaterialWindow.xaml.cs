using ConstructionMaterialsManager.Models;
using ConstructionMaterialsManager.Services;
using System.Windows;

namespace ConstructionMaterialsManager.Views.Windows
{
    public partial class EditProjectMaterialWindow : Window
    {
        private readonly IDatabaseService _databaseService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventAggregator _eventAggregator;
        private ProjectMaterial _projectMaterial;

        public EditProjectMaterialWindow(IDatabaseService databaseService, IServiceProvider serviceProvider, IEventAggregator eventAggregator)
        {
            InitializeComponent();
            _databaseService = databaseService;
            _serviceProvider = serviceProvider;
            _eventAggregator = eventAggregator;
        }

        public void SetProjectMaterial(ProjectMaterial projectMaterial)
        {
            _projectMaterial = projectMaterial;
            MaterialNameTextBox.Text = projectMaterial.Material?.Name ?? "Неизвестно";
            PlannedQuantityTextBox.Text = projectMaterial.PlannedQuantity.ToString();
            UsedQuantityTextBox.Text = (projectMaterial.UsedQuantity ?? 0).ToString();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(PlannedQuantityTextBox.Text, out decimal planned) ||
                !decimal.TryParse(UsedQuantityTextBox.Text, out decimal used))
            {
                MessageBox.Show("Введите корректные числовые значения.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (planned <= 0)
            {
                MessageBox.Show("Планируемое количество должно быть больше нуля.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _projectMaterial.PlannedQuantity = planned;
                _projectMaterial.UsedQuantity = used;
                _databaseService.UpdateProjectMaterial(_projectMaterial);

                _eventAggregator.Publish(new ProjectMaterialChangedMessage(
                    _projectMaterial.ProjectId,
                    _projectMaterial.ProjectMaterialId,
                    ChangeType.Modified));

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
