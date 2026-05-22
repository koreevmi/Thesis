using ConstructionMaterialsManager.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ConstructionMaterialsManager.Views.Controls
{
    public partial class NotificationToast : UserControl
    {
        public NotificationToast(Notification notification)
        {
            InitializeComponent();
            SetNotificationData(notification);
            StartAutoCloseTimer();
        }

        private void SetNotificationData(Notification notification)
        {
            TitleText.Text = notification?.Title ?? "Уведомление";
            MessageText.Text = notification?.Message ?? "";

            var type = notification?.Type ?? "";
            switch (type)
            {
                case "Warning":
                    IconText.Text = "⚠️";
                    ToastBorder.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FEF3C7");
                    ToastBorder.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#FCD34D");
                    break;
                case "Info":
                    IconText.Text = "ℹ️";
                    ToastBorder.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#DBEAFE");
                    ToastBorder.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#93C5FD");
                    break;
                case "Success":
                    IconText.Text = "✅";
                    ToastBorder.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#D1FAE5");
                    ToastBorder.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#6EE7B7");
                    break;
                default:
                    IconText.Text = "📢";
                    ToastBorder.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#F1F5F9");
                    break;
            }
        }

        private DispatcherTimer _autoCloseTimer;

        private void StartAutoCloseTimer()
        {
            _autoCloseTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(8)
            };
            _autoCloseTimer.Tick += (s, e) => AnimateOut();
            _autoCloseTimer.Start();
        }

        public void AnimateIn()
        {
            Opacity = 0;
            RenderTransform = new TranslateTransform(0, -20);

            var fadeAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            BeginAnimation(OpacityProperty, fadeAnimation);

            var slideAnimation = new DoubleAnimation
            {
                From = -20,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            ((TranslateTransform)RenderTransform).BeginAnimation(TranslateTransform.YProperty, slideAnimation);
        }

        private void AnimateOut()
        {
            _autoCloseTimer?.Stop();
            var animation = new DoubleAnimation
            {
                From = Opacity,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            animation.Completed += (s, e) =>
            {
                var parent = Parent as Panel;
                parent?.Children.Remove(this);
            };
            BeginAnimation(OpacityProperty, animation);
        }

        private void AnimateProperty(DependencyProperty property, double from, double to, int ms)
        {
            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(ms),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            BeginAnimation(property, animation);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            AnimateOut();
        }
    }
}
