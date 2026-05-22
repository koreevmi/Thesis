using ConstructionMaterialsManager.Models;
using ConstructionMaterialsManager.Services;
using ConstructionMaterialsManager.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ConstructionMaterialsManager.Views.Pages
{
    public partial class UsersPage : UserControl
    {
        private readonly IDatabaseService _databaseService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventAggregator _eventAggregator;
        private ObservableCollection<User> _users;

        public UsersPage(IDatabaseService databaseService, IServiceProvider serviceProvider, IEventAggregator eventAggregator)
        {
            InitializeComponent();
            _databaseService = databaseService;
            _serviceProvider = serviceProvider;
            _eventAggregator = eventAggregator;

            // Инициализируем коллекцию
            _users = new ObservableCollection<User>();
            DataGrid.ItemsSource = _users;

            _eventAggregator.Subscribe<UserChangedMessage>(OnUserChanged);

            LoadUsers();
            UpdateUI();

            Unloaded += UsersPage_Unloaded;
        }

        private void UsersPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _eventAggregator.Unsubscribe<UserChangedMessage>(OnUserChanged);
        }

        #region Обработчики событий

        private void OnUserChanged(UserChangedMessage msg)
        {
            Application.Current.Dispatcher.Invoke(() => LoadUsers());
        }

        #endregion

        private void UpdateUI()
        {
            if (App.CurrentUser?.Role != "Администратор")
            {
                AddBtn.Visibility = Visibility.Collapsed;
                EditBtn.Visibility = Visibility.Collapsed;
                DeleteBtn.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadUsers()
        {
            try
            {
                var users = _databaseService.GetUsers();
                if (users == null) return;

                _users.Clear();
                foreach (var user in users)
                {
                    _users.Add(user);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var userWindow = _serviceProvider.GetRequiredService<UserWindow>();
                if (userWindow.ShowDialog() == true)
                {
                    LoadUsers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении пользователя: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedUser = DataGrid.SelectedItem as User;
                if (selectedUser != null)
                {
                    var userWindow = _serviceProvider.GetRequiredService<UserWindow>();
                    userWindow.SetUser(selectedUser);
                    if (userWindow.ShowDialog() == true)
                    {
                        LoadUsers();
                    }
                }
                else
                {
                    MessageBox.Show("Выберите пользователя для редактирования.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании пользователя: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (App.CurrentUser?.Role != "Администратор")
                {
                    MessageBox.Show("Только администратор может удалять пользователей.");
                    return;
                }

                var selectedUser = DataGrid.SelectedItem as User;
                if (selectedUser != null)
                {
                    var result = MessageBox.Show("Вы уверены, что хотите удалить этого пользователя?",
                        "Подтверждение", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        var userId = selectedUser.UserId;
                        _databaseService.DeleteUser(userId);
                        _eventAggregator.Publish(new UserChangedMessage(userId, ChangeType.Deleted));
                        LoadUsers();
                    }
                }
                else
                {
                    MessageBox.Show("Выберите пользователя для удаления.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
