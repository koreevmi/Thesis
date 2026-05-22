using ConstructionMaterialsManager.Models;
using ConstructionMaterialsManager.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ConstructionMaterialsManager.Views.Pages
{
    public partial class NotificationsPage : UserControl
    {
        private static INotificationService NotificationService =>
            ((App)Application.Current).ServiceProvider.GetRequiredService<INotificationService>();

        private readonly ObservableCollection<Notification> _notifications = new();
        private string _currentFilter = "";

        public NotificationsPage()
        {
            InitializeComponent();
            NotificationsList.ItemsSource = _notifications;
            Loaded += (s, e) => LoadNotifications();
        }

        private void LoadNotifications()
        {
            try
            {
                _notifications.Clear();
                var all = NotificationService.GetNotifications();
                if (all == null)
                    return;

                if (!string.IsNullOrEmpty(_currentFilter))
                    all = all.Where(n => n.Type == _currentFilter).ToList();

                foreach (var n in all)
                    _notifications.Add(n);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки уведомлений: {ex.InnerException?.Message ?? ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CheckNowBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NotificationService.CheckMaterialShortages();
                LoadNotifications();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки: {ex.InnerException?.Message ?? ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MarkAllReadBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NotificationService.MarkAllAsRead();
                LoadNotifications();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.InnerException?.Message ?? ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ClearAllBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var all = NotificationService.GetNotifications();
                if (all == null) return;
                foreach (var n in all)
                    NotificationService.DeleteNotification(n.NotificationId);
                LoadNotifications();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка очистки: {ex.InnerException?.Message ?? ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void TypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TypeFilter.SelectedItem is ComboBoxItem item && item.Tag is string tag)
                _currentFilter = tag;
            else
                _currentFilter = "";
            LoadNotifications();
        }

        private void DeleteNotification_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.Tag is int id)
                {
                    NotificationService.DeleteNotification(id);
                    LoadNotifications();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.InnerException?.Message ?? ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
