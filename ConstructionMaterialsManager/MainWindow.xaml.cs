using ConstructionMaterialsManager.Services;
using ConstructionMaterialsManager.Views.Controls;
using ConstructionMaterialsManager.Views.Pages;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ConstructionMaterialsManager.Views.Windows
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly INotificationService _notificationService;

        public MainWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _notificationService = serviceProvider.GetRequiredService<INotificationService>();
            UpdateUI();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем уведомления при загрузке
                _notificationService.CheckMaterialShortages();
                UpdateNotificationBadge();
            }
            catch
            {
                // Уведомления не доступны (таблица не создана) — продолжаем работу
            }

            MaterialsNavBtn.IsChecked = true;

            // Показываем toast-уведомления о проблемах
            try
            {
                ShowWarningToasts();
            }
            catch
            {
                // Toast'ы не доступны — продолжаем работу
            }
        }

        private void UpdateUI()
        {
            if (App.CurrentUser != null)
            {
                UserNameLabel.Text = App.CurrentUser.FullName;
                UserRoleLabel.Text = App.CurrentUser.Role;

                if (App.CurrentUser.Role == "Администратор")
                {
                    UsersNavBtn.Visibility = Visibility.Visible;
                }
            }
        }

        private void UpdateNotificationBadge()
        {
            var count = _notificationService.GetUnreadCount();
            if (count > 0)
            {
                NotificationBadge.Visibility = Visibility.Visible;
                UnreadCountLabel.Text = count.ToString();
            }
            else
            {
                NotificationBadge.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowWarningToasts()
        {
            var warnings = _notificationService.GetNotifications(onlyUnread: true)
                .Where(n => n.Type == "Warning")
                .Take(3)
                .ToList();

            foreach (var warning in warnings)
            {
                ShowToast(warning);
            }
        }

        public void ShowToast(Models.Notification notification)
        {
            var toast = new NotificationToast(notification);
            ToastContainer.Children.Add(toast);
            toast.AnimateIn();
        }

        private void NavigateToPage(UserControl page)
        {
            MainFrame.Content = page;
        }

        private void MaterialsNavBtn_Checked(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetRequiredService<MaterialsPage>();
            NavigateToPage(page);
        }

        private void SuppliersNavBtn_Checked(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetRequiredService<SuppliersPage>();
            NavigateToPage(page);
        }

        private void ProjectsNavBtn_Checked(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetRequiredService<ProjectsPage>();
            NavigateToPage(page);
        }

        private void DeliveriesNavBtn_Checked(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetRequiredService<DeliveriesPage>();
            NavigateToPage(page);
        }

        private void ReportsNavBtn_Checked(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetRequiredService<ReportsPage>();
            NavigateToPage(page);
        }

        private void NotificationsNavBtn_Checked(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetRequiredService<NotificationsPage>();
            NavigateToPage(page);
        }

        private void QualityChecksNavBtn_Checked(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetRequiredService<QualityChecksPage>();
            NavigateToPage(page);
        }

        private void UsersNavBtn_Checked(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetRequiredService<UsersPage>();
            NavigateToPage(page);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentUser = null;
            var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
            Close();
        }

        private void NotificationBadge_Click(object sender, MouseButtonEventArgs e)
        {
            NotificationsNavBtn.IsChecked = true;
        }
    }
}
