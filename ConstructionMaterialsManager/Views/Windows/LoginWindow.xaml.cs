using ConstructionMaterialsManager.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace ConstructionMaterialsManager.Views.Windows
{
    public partial class LoginWindow : Window
    {
        private readonly IDatabaseService _databaseService;
        private readonly IServiceProvider _serviceProvider;

        public LoginWindow(IDatabaseService databaseService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _databaseService = databaseService;
            _serviceProvider = serviceProvider;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ErrorTextBlock.Text = "Логин и пароль не могут быть пустыми!";
                ErrorTextBlock.Visibility = Visibility.Visible;
                return;
            }

            var user = _databaseService.GetUserByLogin(login);

            if (user != null && user.Password == password)
            {
                App.CurrentUser = user;
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
                Close();
            }
            else
            {
                ErrorTextBlock.Text = "Неверный логин или пароль!";
                ErrorTextBlock.Visibility = Visibility.Visible;
            }
        }
    }
}
