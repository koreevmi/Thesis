using ConstructionMaterialsManager.Models;
using ConstructionMaterialsManager.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ConstructionMaterialsManager.Views.Windows
{
    public class OverrunToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percent)
            {
                if (percent > 10) return "#EF4444";
                if (percent > 5) return "#F59E0B";
                if (percent > 0) return "#3B82F6";
                return "#10B981";
            }
            return "#94A3B8";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class MaterialCalculatorWindow : Window
    {
        private readonly IDatabaseService _databaseService;
        private readonly ICalculatorService _calculatorService;
        private ObservableCollection<MaterialCalculationResult> _planFactResults = new();

        public MaterialCalculatorWindow(IDatabaseService databaseService, ICalculatorService calculatorService)
        {
            InitializeComponent();
            _databaseService = databaseService;
            _calculatorService = calculatorService;

            LoadMaterials();
            LoadProjects();
            PlanFactDataGrid.ItemsSource = _planFactResults;
        }

        private void LoadMaterials()
        {
            var materials = _databaseService.GetMaterials();
            MaterialComboBox.ItemsSource = materials;
        }

        private void LoadProjects()
        {
            var projects = _databaseService.GetProjects();
            ProjectComboBox.ItemsSource = projects;

            if (projects.Count > 0)
            {
                ProjectComboBox.SelectedIndex = 0;
            }
        }

        private void CalculateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialComboBox.SelectedItem is not Material material)
            {
                MessageBox.Show("Выберите материал для расчета.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(LengthTextBox.Text, out double length) || length <= 0)
            {
                MessageBox.Show("Укажите корректную длину участка (м).", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(WidthTextBox.Text, out double width) || width <= 0)
            {
                MessageBox.Show("Укажите корректную ширину участка (м).", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(ThicknessTextBox.Text, out double thickness) || thickness <= 0)
            {
                MessageBox.Show("Укажите корректную толщину слоя (м).", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double lossPercent = 5;
            if (!string.IsNullOrEmpty(LossTextBox.Text))
            {
                if (!double.TryParse(LossTextBox.Text, out lossPercent) || lossPercent < 0)
                {
                    MessageBox.Show("Укажите корректный процент потерь.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                var result = _calculatorService.CalculateMaterialUsage(material.MaterialId, length, width, thickness, lossPercent);

                VolumeResult.Text = $"{result.VolumeCubicMeters:N2} м³";
                MassResult.Text = $"{result.MassTonnes:N2} т";
                PlannedResult.Text = $"{result.PlannedQuantity:N2} {material.Unit}";
                CostResult.Text = $"{result.EstimatedCost:N2} ₽";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка расчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProjectComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LoadPlanFact();
        }

        private void RefreshPlanFactBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadPlanFact();
        }

        private void LoadPlanFact()
        {
            if (ProjectComboBox.SelectedItem is not Project project)
            {
                _planFactResults.Clear();
                return;
            }

            try
            {
                var results = _calculatorService.CalculatePlanVsFact(project.ProjectId);
                _planFactResults.Clear();
                foreach (var r in results)
                {
                    _planFactResults.Add(r);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
