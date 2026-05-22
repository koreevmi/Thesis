using ConstructionMaterialsManager.Models;
using ConstructionMaterialsManager.Services;
using System.Windows;
using System.Windows.Controls;

namespace ConstructionMaterialsManager.Views.Windows
{
    public partial class MaterialWindow : Window
    {
        private readonly IDatabaseService _databaseService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventAggregator _eventAggregator;
        private Material _material = null!;
        private bool _isEditMode;

        public MaterialWindow(IDatabaseService databaseService, IServiceProvider serviceProvider, IEventAggregator eventAggregator)
        {
            InitializeComponent();
            _databaseService = databaseService;
            _serviceProvider = serviceProvider;
            _eventAggregator = eventAggregator;

            SupplierComboBox.ItemsSource = _databaseService.GetSuppliers();
            MaterialTypeComboBox.ItemsSource = _databaseService.GetMaterialTypes();
        }

        public void SetMaterial(Material material)
        {
            _material = material;
            _isEditMode = true;

            NameTextBox.Text = material.Name;
            UnitTextBox.Text = material.Unit;
            CostPerUnitTextBox.Text = material.CostPerUnit.ToString();
            SupplierComboBox.SelectedValue = material.SupplierId;
            CurrentStockTextBox.Text = material.CurrentStock.ToString();
            MaterialTypeComboBox.SelectedValue = material.MaterialTypeId;
            DensityTextBox.Text = material.Density?.ToString() ?? "";
            FractionTextBox.Text = material.Fraction ?? "";
            GostTextBox.Text = material.Gost ?? "";
            StrengthGradeTextBox.Text = material.StrengthGrade ?? "";
            FrostResistanceTextBox.Text = material.FrostResistance ?? "";
            WaterResistanceTextBox.Text = material.WaterResistance ?? "";
            LeshchadnessTextBox.Text = material.Leshchadness ?? "";
            FinenessModuleTextBox.Text = material.FinenessModule ?? "";
            NotesTextBox.Text = material.Notes ?? "";
            ShelfLifeTextBox.Text = material.ShelfLifeDays?.ToString() ?? "";

            if (!string.IsNullOrEmpty(material.RadioactivityClass))
            {
                foreach (ComboBoxItem item in RadioactivityComboBox.Items)
                {
                    if (item.Content.ToString() == material.RadioactivityClass)
                    {
                        RadioactivityComboBox.SelectedItem = item;
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(material.StorageType))
            {
                foreach (ComboBoxItem item in StorageTypeComboBox.Items)
                {
                    if (item.Content.ToString() == material.StorageType)
                    {
                        StorageTypeComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NameTextBox.Text) ||
                string.IsNullOrEmpty(UnitTextBox.Text) ||
                !decimal.TryParse(CostPerUnitTextBox.Text, out decimal costPerUnit) ||
                SupplierComboBox.SelectedValue == null ||
                !decimal.TryParse(CurrentStockTextBox.Text, out decimal currentStock))
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля корректно.");
                return;
            }

            int? materialTypeId = null;
            if (MaterialTypeComboBox.SelectedValue != null)
            {
                materialTypeId = (int)MaterialTypeComboBox.SelectedValue;
            }

            decimal? density = null;
            if (!string.IsNullOrEmpty(DensityTextBox.Text) && decimal.TryParse(DensityTextBox.Text, out decimal d))
            {
                density = d;
            }

            int? shelfLifeDays = null;
            if (!string.IsNullOrEmpty(ShelfLifeTextBox.Text) && int.TryParse(ShelfLifeTextBox.Text, out int s))
            {
                shelfLifeDays = s;
            }

            var radioactivityItem = RadioactivityComboBox.SelectedItem as ComboBoxItem;
            var radioactivity = radioactivityItem?.Content?.ToString();

            var storageItem = StorageTypeComboBox.SelectedItem as ComboBoxItem;
            var storageType = storageItem?.Content?.ToString() ?? "Открытый";

            if (_isEditMode)
            {
                _material.Name = NameTextBox.Text;
                _material.Unit = UnitTextBox.Text;
                _material.CostPerUnit = costPerUnit;
                _material.SupplierId = (int)SupplierComboBox.SelectedValue;
                _material.CurrentStock = currentStock;
                _material.MaterialTypeId = materialTypeId;
                _material.Density = density;
                _material.Fraction = FractionTextBox.Text;
                _material.Gost = GostTextBox.Text;
                _material.StrengthGrade = StrengthGradeTextBox.Text;
                _material.FrostResistance = FrostResistanceTextBox.Text;
                _material.WaterResistance = WaterResistanceTextBox.Text;
                _material.RadioactivityClass = radioactivity;
                _material.Leshchadness = LeshchadnessTextBox.Text;
                _material.FinenessModule = FinenessModuleTextBox.Text;
                _material.StorageType = storageType;
                _material.ShelfLifeDays = shelfLifeDays;
                _material.Notes = NotesTextBox.Text;
                _databaseService.UpdateMaterial(_material);
            }
            else
            {
                _material = new Material
                {
                    Name = NameTextBox.Text,
                    Unit = UnitTextBox.Text,
                    CostPerUnit = costPerUnit,
                    SupplierId = (int)SupplierComboBox.SelectedValue,
                    CurrentStock = currentStock,
                    MaterialTypeId = materialTypeId,
                    Density = density,
                    Fraction = FractionTextBox.Text,
                    Gost = GostTextBox.Text,
                    StrengthGrade = StrengthGradeTextBox.Text,
                    FrostResistance = FrostResistanceTextBox.Text,
                    WaterResistance = WaterResistanceTextBox.Text,
                    RadioactivityClass = radioactivity,
                    Leshchadness = LeshchadnessTextBox.Text,
                    FinenessModule = FinenessModuleTextBox.Text,
                    StorageType = storageType,
                    ShelfLifeDays = shelfLifeDays,
                    Notes = NotesTextBox.Text
                };
                _databaseService.AddMaterial(_material);
            }

            _eventAggregator.Publish(new MaterialChangedMessage(_material.MaterialId, _isEditMode ? ChangeType.Modified : ChangeType.Added));
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
