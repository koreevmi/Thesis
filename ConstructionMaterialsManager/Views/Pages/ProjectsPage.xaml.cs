using ConstructionMaterialsManager.Models;
using ConstructionMaterialsManager.Services;
using ConstructionMaterialsManager.Views.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ConstructionMaterialsManager.Views.Pages
{
    public partial class ProjectsPage : UserControl
    {
        private readonly IDatabaseService _databaseService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventAggregator _eventAggregator;
        private ObservableCollection<Project> _projects = new ObservableCollection<Project>();
        private List<Project> _allProjects = new List<Project>();

        public ProjectsPage(IDatabaseService databaseService, IServiceProvider serviceProvider, IEventAggregator eventAggregator)
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
                _eventAggregator.Subscribe<ProjectChangedMessage>(OnProjectChanged);
                _eventAggregator.Subscribe<ProjectMaterialChangedMessage>(OnProjectMaterialChanged);

                ProjectsDataGrid.ItemsSource = _projects;
                LoadProjects();
                UpdateUI();

                Unloaded += ProjectsPage_Unloaded;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации страницы: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProjectsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _eventAggregator.Unsubscribe<ProjectChangedMessage>(OnProjectChanged);
            _eventAggregator.Unsubscribe<ProjectMaterialChangedMessage>(OnProjectMaterialChanged);
        }

        #region Обработчики событий

        private void OnProjectChanged(ProjectChangedMessage msg)
        {
            Application.Current.Dispatcher.Invoke(() => LoadProjects());
        }

        private void OnProjectMaterialChanged(ProjectMaterialChangedMessage msg)
        {
            Application.Current.Dispatcher.Invoke(() => LoadProjects());
        }

        #endregion

        private void UpdateUI()
        {
            try
            {
                bool isGuest = App.CurrentUser?.Role == "Гость";
                AddProjectBtn.Visibility = isGuest ? Visibility.Collapsed : Visibility.Visible;
                EditProjectBtn.Visibility = isGuest ? Visibility.Collapsed : Visibility.Visible;
                DeleteProjectBtn.Visibility = isGuest ? Visibility.Collapsed : Visibility.Visible;
                ViewMaterialsBtn.Visibility = isGuest ? Visibility.Collapsed : Visibility.Visible;
            }
            catch
            {
                // Игнорируем ошибки обновления UI
            }
        }

        private void LoadProjects()
        {
            try
            {
                _projects.Clear();
                _allProjects = _databaseService.GetProjects() ?? new List<Project>();

                foreach (var project in _allProjects)
                {
                    if (project != null)
                    {
                        _projects.Add(project);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки проектов: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProjectFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ProjectFilterWatermark.Visibility = string.IsNullOrEmpty(ProjectFilterTextBox.Text)
                    ? Visibility.Visible : Visibility.Collapsed;
                ApplyFilters();
            }
            catch
            {
                // Игнорируем ошибки при изменении текста фильтра
            }
        }

        private void ProjectStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ApplyFilters();
            }
            catch
            {
                // Игнорируем ошибки при изменении фильтра статусов
            }
        }

        private void ApplyFilters()
        {
            try
            {
                // Если нет загруженных проектов, выходим
                if (_allProjects == null || _allProjects.Count == 0)
                {
                    _projects.Clear();
                    return;
                }

                // Фильтруем по тексту
                var filteredProjects = _allProjects.AsQueryable();

                if (!string.IsNullOrEmpty(ProjectFilterTextBox.Text))
                {
                    string filterText = ProjectFilterTextBox.Text.ToLower();
                    filteredProjects = filteredProjects.Where(p =>
                        p != null && !string.IsNullOrEmpty(p.Name) &&
                        p.Name.ToLower().Contains(filterText));
                }

                // Фильтруем по статусу
                if (ProjectStatusFilter.SelectedItem is ComboBoxItem selectedItem &&
                    selectedItem.Tag != null)
                {
                    string statusTag = selectedItem.Tag.ToString();
                    if (!string.IsNullOrEmpty(statusTag))
                    {
                        filteredProjects = filteredProjects.Where(p =>
                            p != null && !string.IsNullOrEmpty(p.Status) &&
                            p.Status.Equals(statusTag, StringComparison.OrdinalIgnoreCase));
                    }
                }

                // Обновляем список
                _projects.Clear();
                foreach (var project in filteredProjects.ToList())
                {
                    if (project != null)
                    {
                        _projects.Add(project);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddProjectBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var projectWindow = _serviceProvider.GetRequiredService<ProjectWindow>();
                if (projectWindow.ShowDialog() == true)
                {
                    LoadProjects();
                }
            }
            catch (DbUpdateException dbEx)
            {
                MessageBox.Show($"Ошибка при добавлении проекта: {dbEx.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                var addMsg = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка при добавлении проекта: {addMsg}");
            }
        }

        private void EditProjectBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedProject = ProjectsDataGrid.SelectedItem as Project;
                if (selectedProject != null)
                {
                    var projectWindow = _serviceProvider.GetRequiredService<ProjectWindow>();
                    projectWindow.SetProject(selectedProject);
                    if (projectWindow.ShowDialog() == true)
                    {
                        LoadProjects();
                    }
                }
                else
                {
                    MessageBox.Show("Выберите проект для редактирования.");
                }
            }
            catch (DbUpdateException dbEx)
            {
                MessageBox.Show($"Ошибка при редактировании проекта: {dbEx.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                var editMsg = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка при редактировании проекта: {editMsg}");
            }
        }

        private void DeleteProjectBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (App.CurrentUser?.Role != "Администратор")
                {
                    MessageBox.Show("Только администратор может удалять проекты.");
                    return;
                }

                var selectedProject = ProjectsDataGrid.SelectedItem as Project;
                if (selectedProject != null)
                {
                    var result = MessageBox.Show("Вы уверены, что хотите удалить этот проект?",
                        "Подтверждение", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        var projectId = selectedProject.ProjectId;
                        // Передаем ProjectId типа int
                        _databaseService.DeleteProject(projectId);
                        _eventAggregator.Publish(new ProjectChangedMessage(projectId, ChangeType.Deleted));
                        LoadProjects();
                    }
                }
                else
                {
                    MessageBox.Show("Выберите проект для удаления.");
                }
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка при удалении проекта: {msg}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewMaterialsBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedProject = ProjectsDataGrid.SelectedItem as Project;
                if (selectedProject != null && selectedProject.ProjectId > 0)
                {
                    var projectMaterialsWindow = _serviceProvider.GetRequiredService<ProjectMaterialsWindow>();
                    projectMaterialsWindow.SetProject(selectedProject.ProjectId);
                    projectMaterialsWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Выберите проект для просмотра материалов.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при просмотре материалов проекта: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
