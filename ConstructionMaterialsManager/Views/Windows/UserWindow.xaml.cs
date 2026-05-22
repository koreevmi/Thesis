using ConstructionMaterialsManager.Models;
using ConstructionMaterialsManager.Services;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace ConstructionMaterialsManager.Views.Windows
{
    public partial class UserWindow : Window
    {
        private readonly IDatabaseService _databaseService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventAggregator _eventAggregator;
        private User _user;
        private bool _isEditMode;

        public UserWindow(IDatabaseService databaseService, IServiceProvider serviceProvider, IEventAggregator eventAggregator)
        {
            InitializeComponent();
            _databaseService = databaseService;
            _serviceProvider = serviceProvider;
            _eventAggregator = eventAggregator;
        }

        public void SetUser(User user)
        {
            _user = user;
            _isEditMode = true;

            LoginTextBox.Text = user.Login;
            FullNameTextBox.Text = user.FullName;
            EmailTextBox.Text = user.Email;
            RoleComboBox.Text = user.Role;
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true; // Email is optional

            // Simple RFC 5322 compliant regex
            return Regex.IsMatch(email,
                @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$");
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(LoginTextBox.Text) ||
                string.IsNullOrEmpty(FullNameTextBox.Text) ||
                string.IsNullOrEmpty(PasswordBox.Password) ||
                RoleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.");
                return;
            }

            var email = EmailTextBox.Text.Trim();
            if (!IsValidEmail(email))
            {
                MessageBox.Show("Введите корректный email-адрес.", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var role = ((ComboBoxItem)RoleComboBox.SelectedItem).Content.ToString()!;

            if (_isEditMode)
            {
                _user.Login = LoginTextBox.Text;
                _user.FullName = FullNameTextBox.Text;
                if (!string.IsNullOrEmpty(PasswordBox.Password))
                {
                    _user.Password = PasswordBox.Password;
                }
                _user.Email = email;
                _user.Role = role;
                _databaseService.UpdateUser(_user);
            }
            else
            {
                _user = new User
                {
                    Login = LoginTextBox.Text,
                    Password = PasswordBox.Password,
                    FullName = FullNameTextBox.Text,
                    Email = email,
                    Role = role
                };
                _databaseService.AddUser(_user);
            }

            _eventAggregator.Publish(new UserChangedMessage(_user.UserId, _isEditMode ? ChangeType.Modified : ChangeType.Added));
            DialogResult = true;
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
