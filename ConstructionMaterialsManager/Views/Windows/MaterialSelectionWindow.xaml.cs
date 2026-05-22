using ConstructionMaterialsManager.Models;
using ConstructionMaterialsManager.Services;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace ConstructionMaterialsManager.Views.Windows
{
    public partial class MaterialSelectionWindow : Window
    {
        private readonly IDatabaseService _databaseService;
        private readonly IEventAggregator _eventAggregator;
        private int _projectId;

        public MaterialSelectionWindow(IDatabaseService databaseService, IEventAggregator eventAggregator)
        {
            InitializeComponent();
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            LoadMaterials();
        }

        public void SetProject(int projectId)
        {
            if (projectId <= 0)
            {
                throw new ArgumentException("ProjectId должен быть больше нуля.", nameof(projectId));
            }

            _projectId = projectId;
        }

        private void LoadMaterials()
        {
            MaterialsDataGrid.ItemsSource = _databaseService.GetMaterials();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedMaterial = MaterialsDataGrid.SelectedItem as Material;
                if (selectedMaterial != null && decimal.TryParse(QuantityTextBox.Text, out decimal quantity))
                {
                    var projectMaterial = new ProjectMaterial
                    {
                        ProjectId = _projectId,
                        MaterialId = selectedMaterial.MaterialId,
                        PlannedQuantity = quantity,
                        UsedQuantity = 0
                    };

                    _databaseService.AddProjectMaterial(projectMaterial);
                    _eventAggregator.Publish(new ProjectMaterialChangedMessage(_projectId, projectMaterial.ProjectMaterialId, ChangeType.Added));
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Выберите материал для добавления и укажите количество.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (DbUpdateException dbEx)
            {
                MessageBox.Show($"Ошибка при добавлении материала: {dbEx.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении материала: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
