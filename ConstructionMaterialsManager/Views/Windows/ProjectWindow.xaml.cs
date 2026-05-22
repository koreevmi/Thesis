using ConstructionMaterialsManager.Models;
using ConstructionMaterialsManager.Services;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;

namespace ConstructionMaterialsManager.Views.Windows
{
    public partial class ProjectWindow : Window
    {
        private readonly IDatabaseService _databaseService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventAggregator _eventAggregator;
        private Project _project;
        private bool _isEditMode;

        public ProjectWindow(IDatabaseService databaseService, IServiceProvider serviceProvider, IEventAggregator eventAggregator)
        {
            InitializeComponent();
            _databaseService = databaseService;
            _serviceProvider = serviceProvider;
            _eventAggregator = eventAggregator;
            _project = new Project();
        }

        public void SetProject(Project project)
        {
            _project = project;
            _isEditMode = true;

            ProjectNameTextBox.Text = project.Name;
            ProjectDescriptionTextBox.Text = project.Description;
            ProjectStartDatePicker.SelectedDate = project.StartDate;
            ProjectEndDatePicker.SelectedDate = project.EndDate;
            ProjectBudgetTextBox.Text = project.Budget.ToString();
            ProjectStatusComboBox.Text = project.Status;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ProjectNameTextBox.Text) ||
                string.IsNullOrEmpty(ProjectDescriptionTextBox.Text) ||
                ProjectStartDatePicker.SelectedDate == null ||
                ProjectEndDatePicker.SelectedDate == null ||
                !decimal.TryParse(ProjectBudgetTextBox.Text, out decimal budget) ||
                ProjectStatusComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, заполните все поля корректно.");
                return;
            }

            var startDate = (DateTime)ProjectStartDatePicker.SelectedDate;
            var endDate = (DateTime)ProjectEndDatePicker.SelectedDate;

            if (endDate < startDate)
            {
                MessageBox.Show("Дата окончания не может быть раньше даты начала.", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_isEditMode)
                {
                    _project.Name = ProjectNameTextBox.Text;
                    _project.Description = ProjectDescriptionTextBox.Text;
                    _project.StartDate = startDate;
                    _project.EndDate = endDate;
                    _project.Budget = budget;
                    _project.Status = ((ComboBoxItem)ProjectStatusComboBox.SelectedItem).Content.ToString()!;
                    _databaseService.UpdateProject(_project);
                }
                else
                {
                    _project = new Project
                    {
                        Name = ProjectNameTextBox.Text,
                        Description = ProjectDescriptionTextBox.Text,
                        StartDate = startDate,
                        EndDate = endDate,
                        Budget = budget,
                        Status = ((ComboBoxItem)ProjectStatusComboBox.SelectedItem).Content.ToString()!
                    };
                    _databaseService.AddProject(_project);
                }

                _eventAggregator.Publish(new ProjectChangedMessage(_project.ProjectId, _isEditMode ? ChangeType.Modified : ChangeType.Added));
                DialogResult = true;
                Close();
            }
            catch (DbUpdateException dbEx)
            {
                var msg = dbEx.InnerException?.Message ?? dbEx.Message;
                MessageBox.Show($"Ошибка при сохранении проекта: {msg}");
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка при сохранении проекта: {msg}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
