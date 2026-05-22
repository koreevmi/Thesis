using ConstructionMaterialsManager.Models;
using ConstructionMaterialsManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace ConstructionMaterialsManager.Views.Windows
{
    public partial class ProjectMaterialsWindow : Window
    {
        private readonly IDatabaseService _databaseService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventAggregator _eventAggregator;
        private Project _project;
        private int _projectId;

        public ProjectMaterialsWindow(IDatabaseService databaseService, IServiceProvider serviceProvider, IEventAggregator eventAggregator)
        {
            InitializeComponent();
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }


        public void SetProject(int projectId)
        {
            if (projectId <= 0)
            {
                throw new ArgumentException("ProjectId должен быть больше нуля.", nameof(projectId));
            }

            _projectId = projectId;
            LoadProjectMaterials();
        }

        private void LoadProjectMaterials()
        {
            try
            {
                var projectMaterials = _databaseService.GetProjectMaterials(_projectId);
                ProjectMaterialsDataGrid.ItemsSource = projectMaterials;
            }
            catch (Exception ex)
            {
                var loadMsg = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка при загрузке материалов проекта: {loadMsg}");
            }
        }


        private void AddMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            if (_projectId <= 0)
            {
                MessageBox.Show("Проект не выбран.");
                return;
            }

            try
            {
                var materialSelectionWindow = _serviceProvider.GetRequiredService<MaterialSelectionWindow>();
                materialSelectionWindow.SetProject(_projectId);
                if (materialSelectionWindow.ShowDialog() == true)
                {
                    LoadProjectMaterials();
                }
            }
            catch (Exception ex)
            {
                var addMsg = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка при добавлении материала: {addMsg}");
            }
        }

        private void EditMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedProjectMaterial = ProjectMaterialsDataGrid.SelectedItem as ProjectMaterial;
            if (selectedProjectMaterial == null)
            {
                MessageBox.Show("Выберите материал для редактирования.");
                return;
            }

            try
            {
                var editWindow = _serviceProvider.GetRequiredService<EditProjectMaterialWindow>();
                editWindow.SetProjectMaterial(selectedProjectMaterial);
                if (editWindow.ShowDialog() == true)
                {
                    LoadProjectMaterials();
                }
            }
            catch (Exception ex)
            {
                var editMsg = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка при редактировании материала: {editMsg}");
            }
        }

        private void RemoveMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedProjectMaterial = ProjectMaterialsDataGrid.SelectedItem as ProjectMaterial;
                if (selectedProjectMaterial != null)
                {
                    var result = MessageBox.Show("Вы уверены, что хотите удалить этот материал из проекта?",
                        "Подтверждение", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        var materialId = selectedProjectMaterial.ProjectMaterialId;
                        var projectId = selectedProjectMaterial.ProjectId;
                        _databaseService.RemoveProjectMaterial(materialId);
                        _eventAggregator.Publish(new ProjectMaterialChangedMessage(projectId, materialId, ChangeType.Deleted));
                        LoadProjectMaterials();
                    }
                }
                else
                {
                    MessageBox.Show("Выберите материал для удаления.");
                }
            }
            catch (DbUpdateException dbEx)
            {
                MessageBox.Show($"Ошибка при удалении материала: {dbEx.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                var delMsg = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка при удалении материала: {delMsg}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
