using ClosedXML.Excel;
using ConstructionMaterialsManager.Models;

namespace ConstructionMaterialsManager.Services
{
    public class ExcelService : IExcelService
    {
        private void ApplyHeaderStyle(IXLWorksheet worksheet, int columnCount)
        {
            var headerRange = worksheet.Range(1, 1, 1, columnCount);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E293B");
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        private void ApplyDataStyle(IXLWorksheet worksheet, int rowCount, int columnCount)
        {
            var dataRange = worksheet.Range(2, 1, rowCount, columnCount);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#E2E8F0");
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorderColor = XLColor.FromHtml("#E2E8F0");

            for (int row = 2; row <= rowCount; row++)
            {
                if (row % 2 == 0)
                {
                    worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#F8FAFC");
                }
            }

            worksheet.Columns().AdjustToContents(10, 40);
        }

        public void GenerateMaterialsReport(IEnumerable<Material> materials, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Материалы");

                worksheet.Cell(1, 1).Value = "Наименование";
                worksheet.Cell(1, 2).Value = "Тип";
                worksheet.Cell(1, 3).Value = "Фракция";
                worksheet.Cell(1, 4).Value = "Плотность";
                worksheet.Cell(1, 5).Value = "Марка";
                worksheet.Cell(1, 6).Value = "ГОСТ";
                worksheet.Cell(1, 7).Value = "Ед. изм.";
                worksheet.Cell(1, 8).Value = "Стоимость";
                worksheet.Cell(1, 9).Value = "Поставщик";
                worksheet.Cell(1, 10).Value = "Остаток";

                int row = 2;
                foreach (var material in materials)
                {
                    worksheet.Cell(row, 1).Value = material.Name;
                    worksheet.Cell(row, 2).Value = material.MaterialType?.Name ?? "";
                    worksheet.Cell(row, 3).Value = material.Fraction ?? "";
                    worksheet.Cell(row, 4).Value = material.Density?.ToString() ?? "";
                    worksheet.Cell(row, 5).Value = material.StrengthGrade ?? "";
                    worksheet.Cell(row, 6).Value = material.Gost ?? "";
                    worksheet.Cell(row, 7).Value = material.Unit;
                    worksheet.Cell(row, 8).Value = material.CostPerUnit;
                    worksheet.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00 ₽";
                    worksheet.Cell(row, 9).Value = material.Supplier?.Name ?? "";
                    worksheet.Cell(row, 10).Value = material.CurrentStock ?? 0;
                    row++;
                }

                ApplyHeaderStyle(worksheet, 10);
                ApplyDataStyle(worksheet, row - 1, 10);

                workbook.SaveAs(filePath);
            }
        }

        public void GenerateProjectsReport(IEnumerable<Project> projects, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Проекты");

                worksheet.Cell(1, 1).Value = "Название";
                worksheet.Cell(1, 2).Value = "Описание";
                worksheet.Cell(1, 3).Value = "Дата начала";
                worksheet.Cell(1, 4).Value = "Дата окончания";
                worksheet.Cell(1, 5).Value = "Бюджет";
                worksheet.Cell(1, 6).Value = "Статус";

                int row = 2;
                foreach (var project in projects)
                {
                    worksheet.Cell(row, 1).Value = project.Name;
                    worksheet.Cell(row, 2).Value = project.Description;
                    worksheet.Cell(row, 3).Value = project.StartDate.ToString("dd.MM.yyyy");
                    worksheet.Cell(row, 4).Value = project.EndDate.ToString("dd.MM.yyyy");
                    worksheet.Cell(row, 5).Value = project.Budget;
                    worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00 ₽";
                    worksheet.Cell(row, 6).Value = project.Status;
                    row++;
                }

                ApplyHeaderStyle(worksheet, 6);
                ApplyDataStyle(worksheet, row - 1, 6);

                workbook.SaveAs(filePath);
            }
        }

        public void GenerateDeliveriesReport(IEnumerable<Delivery> deliveries, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Поставки");

                worksheet.Cell(1, 1).Value = "Материал";
                worksheet.Cell(1, 2).Value = "Партия";
                worksheet.Cell(1, 3).Value = "Сертификат";
                worksheet.Cell(1, 4).Value = "Количество";
                worksheet.Cell(1, 5).Value = "Дата поставки";
                worksheet.Cell(1, 6).Value = "Поставщик";
                worksheet.Cell(1, 7).Value = "Производитель";
                worksheet.Cell(1, 8).Value = "Стоимость";

                int row = 2;
                foreach (var delivery in deliveries)
                {
                    worksheet.Cell(row, 1).Value = delivery.Material?.Name ?? "";
                    worksheet.Cell(row, 2).Value = delivery.BatchNumber ?? "";
                    worksheet.Cell(row, 3).Value = delivery.CertificateNumber ?? "";
                    worksheet.Cell(row, 4).Value = delivery.Quantity;
                    worksheet.Cell(row, 5).Value = delivery.DeliveryDate.ToString("dd.MM.yyyy");
                    worksheet.Cell(row, 6).Value = delivery.Supplier?.Name ?? "";
                    worksheet.Cell(row, 7).Value = delivery.Manufacturer ?? "";
                    worksheet.Cell(row, 8).Value = delivery.Quantity * (delivery.Material?.CostPerUnit ?? 0);
                    worksheet.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00 ₽";
                    row++;
                }

                ApplyHeaderStyle(worksheet, 8);
                ApplyDataStyle(worksheet, row - 1, 8);

                workbook.SaveAs(filePath);
            }
        }

        public void GenerateMaterialMovementsReport(IEnumerable<MaterialMovement> movements, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Движения");

                worksheet.Cell(1, 1).Value = "Материал";
                worksheet.Cell(1, 2).Value = "Количество";
                worksheet.Cell(1, 3).Value = "Дата";
                worksheet.Cell(1, 4).Value = "Тип движения";

                int row = 2;
                foreach (var movement in movements)
                {
                    worksheet.Cell(row, 1).Value = movement.Material?.Name ?? "";
                    worksheet.Cell(row, 2).Value = movement.Quantity;
                    worksheet.Cell(row, 3).Value = movement.MovementDate.ToString("dd.MM.yyyy HH:mm");
                    worksheet.Cell(row, 4).Value = movement.MovementType;
                    row++;
                }

                ApplyHeaderStyle(worksheet, 4);
                ApplyDataStyle(worksheet, row - 1, 4);

                workbook.SaveAs(filePath);
            }
        }

        public void GenerateSuppliersReport(IEnumerable<Supplier> suppliers, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Поставщики");

                worksheet.Cell(1, 1).Value = "Название";
                worksheet.Cell(1, 2).Value = "Контактное лицо";
                worksheet.Cell(1, 3).Value = "Телефон";
                worksheet.Cell(1, 4).Value = "Email";
                worksheet.Cell(1, 5).Value = "Адрес";

                int row = 2;
                foreach (var supplier in suppliers)
                {
                    worksheet.Cell(row, 1).Value = supplier.Name;
                    worksheet.Cell(row, 2).Value = supplier.ContactPerson;
                    worksheet.Cell(row, 3).Value = supplier.Phone;
                    worksheet.Cell(row, 4).Value = supplier.Email;
                    worksheet.Cell(row, 5).Value = supplier.Address;
                    row++;
                }

                ApplyHeaderStyle(worksheet, 5);
                ApplyDataStyle(worksheet, row - 1, 5);

                workbook.SaveAs(filePath);
            }
        }

        public void GenerateQualityChecksReport(IEnumerable<QualityCheck> qualityChecks, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Контроль качества");

                worksheet.Cell(1, 1).Value = "Материал";
                worksheet.Cell(1, 2).Value = "Тип";
                worksheet.Cell(1, 3).Value = "Номер партии";
                worksheet.Cell(1, 4).Value = "Дата проверки";
                worksheet.Cell(1, 5).Value = "Статус";
                worksheet.Cell(1, 6).Value = "Инспектор";
                worksheet.Cell(1, 7).Value = "Результаты";
                worksheet.Cell(1, 8).Value = "Комментарий";

                int row = 2;
                foreach (var qc in qualityChecks)
                {
                    worksheet.Cell(row, 1).Value = qc.Material?.Name ?? "";
                    worksheet.Cell(row, 2).Value = qc.Material?.MaterialType?.Name ?? "";
                    worksheet.Cell(row, 3).Value = qc.BatchNumber;
                    worksheet.Cell(row, 4).Value = qc.CheckDate.ToString("dd.MM.yyyy");
                    worksheet.Cell(row, 5).Value = qc.Status;

                    switch (qc.Status)
                    {
                        case "Одобрено":
                            worksheet.Cell(row, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#D1FAE5");
                            worksheet.Cell(row, 5).Style.Font.FontColor = XLColor.FromHtml("#065F46");
                            break;
                        case "Бракован":
                            worksheet.Cell(row, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#FEE2E2");
                            worksheet.Cell(row, 5).Style.Font.FontColor = XLColor.FromHtml("#991B1B");
                            break;
                        default:
                            worksheet.Cell(row, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#FEF3C7");
                            worksheet.Cell(row, 5).Style.Font.FontColor = XLColor.FromHtml("#92400E");
                            break;
                    }

                    worksheet.Cell(row, 6).Value = qc.InspectorName ?? "";
                    worksheet.Cell(row, 7).Value = qc.TestResults ?? "";
                    worksheet.Cell(row, 8).Value = qc.Comments ?? "";
                    row++;
                }

                ApplyHeaderStyle(worksheet, 8);
                ApplyDataStyle(worksheet, row - 1, 8);

                workbook.SaveAs(filePath);
            }
        }
    }
}
