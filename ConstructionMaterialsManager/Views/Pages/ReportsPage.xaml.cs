using ConstructionMaterialsManager.Services;
using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Controls;

namespace ConstructionMaterialsManager.Views.Pages
{
    public partial class ReportsPage : UserControl
    {
        private readonly IDatabaseService _databaseService;
        private readonly IExcelService _excelService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReportsPage> _logger;

        public ReportsPage(
            IDatabaseService databaseService,
            IExcelService excelService,
            IServiceProvider serviceProvider,
            ILogger<ReportsPage> logger)
        {
            InitializeComponent();
            _databaseService = databaseService;
            _excelService = excelService;
            _serviceProvider = serviceProvider;
            _logger = logger;

            Loaded += ReportsPage_Loaded;
        }

        private void ReportsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            try
            {
                var materials = _databaseService.GetMaterials();
                var projects = _databaseService.GetProjects();
                var deliveries = _databaseService.GetDeliveries();
                var suppliers = _databaseService.GetSuppliers();

                MaterialsCountLabel.Text = materials?.Count.ToString() ?? "0";
                ProjectsCountLabel.Text = projects?.Count.ToString() ?? "0";
                DeliveriesCountLabel.Text = deliveries?.Count.ToString() ?? "0";
                SuppliersCountLabel.Text = suppliers?.Count.ToString() ?? "0";
            }
            catch
            {
            }
        }

        private void GenerateExcelReport<T>(
            Func<IEnumerable<T>> getData,
            Action<IEnumerable<T>, string> generateReport,
            string defaultFileName,
            string reportType)
        {
            try
            {
                var data = getData();
                if (data == null || !data.Any())
                {
                    MessageBox.Show($"Нет данных для генерации отчёта по {reportType}.", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    FileName = defaultFileName,
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    generateReport(data, saveFileDialog.FileName);
                    MessageBox.Show($"Отчёт по {reportType} успешно сохранён:\n{saveFileDialog.FileName}",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при генерации Excel-отчёта по {reportType}");
                MessageBox.Show($"Ошибка при генерации отчёта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MaterialsExcelReportButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateExcelReport(
                () => _databaseService.GetMaterials(),
                (materials, filePath) => _excelService.GenerateMaterialsReport(materials, filePath),
                $"MaterialsReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                "материалам"
            );
        }

        private void ProjectsExcelReportButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateExcelReport(
                () => _databaseService.GetProjects(),
                (projects, filePath) => _excelService.GenerateProjectsReport(projects, filePath),
                $"ProjectsReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                "проектам"
            );
        }

        private void DeliveriesExcelReportButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateExcelReport(
                () => _databaseService.GetDeliveries(),
                (deliveries, filePath) => _excelService.GenerateDeliveriesReport(deliveries, filePath),
                $"DeliveriesReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                "поставкам"
            );
        }

        private void MaterialMovementsExcelReportButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateExcelReport(
                () => _databaseService.GetMaterialMovements(),
                (movements, filePath) => _excelService.GenerateMaterialMovementsReport(movements, filePath),
                $"MaterialMovementsReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                "движениям материалов"
            );
        }

        private void SuppliersExcelReportButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateExcelReport(
                () => _databaseService.GetSuppliers(),
                (suppliers, filePath) => _excelService.GenerateSuppliersReport(suppliers, filePath),
                $"SuppliersReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                "поставщикам"
            );
        }

        private void QualityChecksExcelReportButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateExcelReport(
                () => _databaseService.GetQualityChecks(),
                (checks, filePath) => _excelService.GenerateQualityChecksReport(checks, filePath),
                $"QualityChecksReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                "контролю качества"
            );
        }
    }
}
