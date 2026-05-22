using ConstructionMaterialsManager.Models;
using ConstructionMaterialsManager.Services;
using System.Windows;
using System.Windows.Controls;

namespace ConstructionMaterialsManager.Views.Windows
{
    public partial class QualityCheckWindow : Window
    {
        private readonly IDatabaseService _databaseService;
        private readonly IEventAggregator _eventAggregator;
        private QualityCheck _qualityCheck = null!;
        private bool _isEditMode;

        public QualityCheckWindow(IDatabaseService databaseService, IEventAggregator eventAggregator)
        {
            InitializeComponent();
            _databaseService = databaseService;
            _eventAggregator = eventAggregator;

            MaterialComboBox.ItemsSource = _databaseService.GetMaterials();
            CheckDatePicker.SelectedDate = DateTime.Today;
            StatusComboBox.SelectedIndex = 0;
        }

        public void SetQualityCheck(QualityCheck qualityCheck)
        {
            _qualityCheck = qualityCheck;
            _isEditMode = true;

            MaterialComboBox.SelectedValue = qualityCheck.MaterialId;
            BatchNumberTextBox.Text = qualityCheck.BatchNumber;
            CheckDatePicker.SelectedDate = qualityCheck.CheckDate;
            InspectorTextBox.Text = qualityCheck.InspectorName ?? "";
            TestResultsTextBox.Text = qualityCheck.TestResults ?? "";
            CommentsTextBox.Text = qualityCheck.Comments ?? "";

            foreach (ComboBoxItem item in StatusComboBox.Items)
            {
                if (item.Content.ToString() == qualityCheck.Status)
                {
                    StatusComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialComboBox.SelectedValue == null ||
                string.IsNullOrEmpty(BatchNumberTextBox.Text) ||
                CheckDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.");
                return;
            }

            var statusItem = StatusComboBox.SelectedItem as ComboBoxItem;
            var status = statusItem?.Content?.ToString() ?? "На проверке";

            if (_isEditMode)
            {
                _qualityCheck.MaterialId = (int)MaterialComboBox.SelectedValue;
                _qualityCheck.BatchNumber = BatchNumberTextBox.Text;
                _qualityCheck.CheckDate = CheckDatePicker.SelectedDate.Value;
                _qualityCheck.Status = status;
                _qualityCheck.InspectorName = InspectorTextBox.Text;
                _qualityCheck.TestResults = TestResultsTextBox.Text;
                _qualityCheck.Comments = CommentsTextBox.Text;
                _databaseService.UpdateQualityCheck(_qualityCheck);
            }
            else
            {
                _qualityCheck = new QualityCheck
                {
                    MaterialId = (int)MaterialComboBox.SelectedValue,
                    BatchNumber = BatchNumberTextBox.Text,
                    CheckDate = CheckDatePicker.SelectedDate.Value,
                    Status = status,
                    InspectorName = InspectorTextBox.Text,
                    TestResults = TestResultsTextBox.Text,
                    Comments = CommentsTextBox.Text
                };
                _databaseService.AddQualityCheck(_qualityCheck);
            }

            _eventAggregator.Publish(new QualityCheckChangedMessage(_qualityCheck.QualityCheckId, _isEditMode ? ChangeType.Modified : ChangeType.Added));
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
