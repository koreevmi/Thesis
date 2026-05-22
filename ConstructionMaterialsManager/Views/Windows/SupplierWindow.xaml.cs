using ConstructionMaterialsManager.Models;
using ConstructionMaterialsManager.Services;
using System.Windows;

namespace ConstructionMaterialsManager.Views.Windows
{
    public partial class SupplierWindow : Window
    {
        private readonly IDatabaseService _databaseService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventAggregator _eventAggregator;
        private Supplier _supplier;
        private bool _isEditMode;

        public SupplierWindow(IDatabaseService databaseService, IServiceProvider serviceProvider, IEventAggregator eventAggregator)
        {
            InitializeComponent();
            _databaseService = databaseService;
            _serviceProvider = serviceProvider;
            _eventAggregator = eventAggregator;
        }

        public void SetSupplier(Supplier supplier)
        {
            _supplier = supplier;
            _isEditMode = true;

            NameTextBox.Text = supplier.Name;
            ContactPersonTextBox.Text = supplier.ContactPerson;
            PhoneTextBox.Text = supplier.Phone;
            EmailTextBox.Text = supplier.Email;
            AddressTextBox.Text = supplier.Address;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NameTextBox.Text) ||
                string.IsNullOrEmpty(ContactPersonTextBox.Text) ||
                string.IsNullOrEmpty(PhoneTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.");
                return;
            }

            if (_isEditMode)
            {
                _supplier.Name = NameTextBox.Text;
                _supplier.ContactPerson = ContactPersonTextBox.Text;
                _supplier.Phone = PhoneTextBox.Text;
                _supplier.Email = EmailTextBox.Text;
                _supplier.Address = AddressTextBox.Text;
                _databaseService.UpdateSupplier(_supplier);
            }
            else
            {
                _supplier = new Supplier
                {
                    Name = NameTextBox.Text,
                    ContactPerson = ContactPersonTextBox.Text,
                    Phone = PhoneTextBox.Text,
                    Email = EmailTextBox.Text,
                    Address = AddressTextBox.Text
                };
                _databaseService.AddSupplier(_supplier);
            }

            _eventAggregator.Publish(new SupplierChangedMessage(_supplier.SupplierId, _isEditMode ? ChangeType.Modified : ChangeType.Added));
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
