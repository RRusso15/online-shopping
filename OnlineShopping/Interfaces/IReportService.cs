namespace OnlineShopping.Interfaces;

public interface IReportService
{
    string GenerateSalesReport(DateTime? fromDate = null, DateTime? toDate = null);
    string ExportSalesReportCsv(string outputDirectory, DateTime? fromDate = null, DateTime? toDate = null);
}
