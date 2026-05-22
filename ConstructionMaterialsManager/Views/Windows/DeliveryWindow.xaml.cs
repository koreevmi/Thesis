using ConstructionMaterialsManager.Models;
using ConstructionMaterialsManager.Services;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace ConstructionMaterialsManager.Views.Windows
{
    public partial class DeliveryWindow : Window
    {
        private readonly IDatabaseService _databaseService;
        private readonly IEventAggregator _eventAggregator;
        private Delivery _delivery = null!;
        private bool _isEditMode;

        public DeliveryWindow(IDatabaseService databaseService, IEventAggregator eventAggregator)
        {
            InitializeComponent();
            _databaseService = databaseService;
            _eventAggregator = eventAggregator;
            LoadMaterials();
            LoadSuppliers();
            DeliveryDatePicker.SelectedDate = DateTime.Today;
        }

        private void LoadMaterials()
        {
            try
            {
                MaterialComboBox.ItemsSource = _databaseService.GetMaterials().ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке материалов: {ex.Message}");
            }
        }

        private void LoadSuppliers()
        {
            try
            {
                SupplierComboBox.ItemsSource = _databaseService.GetSuppliers().ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке поставщиков: {ex.Message}");
            }
        }

        public void SetDelivery(Delivery delivery)
        {
            if (delivery == null)
            {
                throw new ArgumentNullException(nameof(delivery));
            }

            _delivery = delivery;
            _isEditMode = true;
            MaterialComboBox.SelectedValue = delivery.MaterialId;
            QuantityTextBox.Text = delivery.Quantity.ToString();
            DeliveryDatePicker.SelectedDate = delivery.DeliveryDate;
            SupplierComboBox.SelectedValue = delivery.SupplierId;
            BatchNumberTextBox.Text = delivery.BatchNumber ?? "";
            CertificateNumberTextBox.Text = delivery.CertificateNumber ?? "";
            CertificateDatePicker.SelectedDate = delivery.CertificateDate;
            ManufacturerTextBox.Text = delivery.Manufacturer ?? "";
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialComboBox.SelectedValue == null || !decimal.TryParse(QuantityTextBox.Text, out decimal quantity) ||
                DeliveryDatePicker.SelectedDate == null || SupplierComboBox.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, заполните все поля корректно.");
                return;
            }

            if (quantity <= 0)
            {
                MessageBox.Show("Количество должно быть больше нуля.");
                return;
            }

            try
            {
                if (_isEditMode)
                {
                    _delivery.MaterialId = (int)MaterialComboBox.SelectedValue;
                    _delivery.Quantity = quantity;
                    _delivery.DeliveryDate = (DateTime)DeliveryDatePicker.SelectedDate;
                    _delivery.SupplierId = (int)SupplierComboBox.SelectedValue;
                    _delivery.BatchNumber = BatchNumberTextBox.Text;
                    _delivery.CertificateNumber = CertificateNumberTextBox.Text;
                    _delivery.CertificateDate = CertificateDatePicker.SelectedDate;
                    _delivery.Manufacturer = ManufacturerTextBox.Text;
                    _databaseService.UpdateDelivery(_delivery);
                }
                else
                {
                    _delivery = new Delivery
                    {
                        MaterialId = (int)MaterialComboBox.SelectedValue,
                        Quantity = quantity,
                        DeliveryDate = (DateTime)DeliveryDatePicker.SelectedDate,
                        SupplierId = (int)SupplierComboBox.SelectedValue,
                        BatchNumber = BatchNumberTextBox.Text,
                        CertificateNumber = CertificateNumberTextBox.Text,
                        CertificateDate = CertificateDatePicker.SelectedDate,
                        Manufacturer = ManufacturerTextBox.Text
                    };

                    _databaseService.AddDelivery(_delivery);

                    var movement = new MaterialMovement
                    {
                        MaterialId = (int)MaterialComboBox.SelectedValue,
                        Quantity = quantity,
                        MovementDate = DateTime.Now,
                        MovementType = "Поступление"
                    };

                    _databaseService.AddMaterialMovement(movement);
                }

                _eventAggregator.Publish(new DeliveryChangedMessage(_delivery.DeliveryId, _isEditMode ? ChangeType.Modified : ChangeType.Added));
                DialogResult = true;
                Close();
            }
            catch (DbUpdateException dbEx)
            {
                var msg = dbEx.InnerException?.Message ?? dbEx.Message;
                MessageBox.Show($"Ошибка при сохранении поставки: {msg}");
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка при сохранении поставки: {msg}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
