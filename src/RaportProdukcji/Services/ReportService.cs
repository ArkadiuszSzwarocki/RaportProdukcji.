using System.Text;
using RaportProdukcji.Models;

namespace RaportProdukcji.Services;

public interface IReportService
{
    Task<byte[]> ExportOrderToCsvAsync(ProductionOrder order);
    Task<byte[]> ExportOrdersToExcelAsync(List<ProductionOrder> orders);
    string GenerateOrderSummaryReport(ProductionOrder order);
}

public class ReportService : IReportService
{
    private readonly ILogger<ReportService> _logger;

    public ReportService(ILogger<ReportService> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> ExportOrderToCsvAsync(ProductionOrder order)
    {
        var csv = new StringBuilder();
        
        // Header
        csv.AppendLine("RAPORT PRODUKCJI - DANE ZLECENIA");
        csv.AppendLine($"Data wygenerowania,{DateTime.Now:dd.MM.yyyy HH:mm:ss}");
        csv.AppendLine();

        // Order info
        csv.AppendLine("INFORMACJE O ZLECENIU");
        csv.AppendLine($"ID Zlecenia,{order.Id}");
        csv.AppendLine($"Nazwa Produktu,{order.ProductName}");
        csv.AppendLine($"Numer Receptury,{order.RecipeNumber}");
        csv.AppendLine($"Typ Worka,{order.BagType.GetDisplayName()}");
        csv.AppendLine($"Planowana Waga (kg),{order.PlannedWeightKg}");
        csv.AppendLine($"Wymieszana Waga (kg),{order.TotalMixedWeight}");
        csv.AppendLine($"Pozostała Waga (kg),{order.RemainingWeight}");
        csv.AppendLine($"Status,{GetStatusText(order.Status)}");
        csv.AppendLine($"Data Utworzenia,{order.CreatedDate:dd.MM.yyyy HH:mm}");
        csv.AppendLine();

        // Batches
        if (order.Batches.Any())
        {
            csv.AppendLine("SZARŻE (WYMIESZANE PARTIE)");
            csv.AppendLine("ID Szarży,Waga (kg),Operator,Data");
            foreach (var batch in order.Batches.OrderBy(b => b.MixedDateTime))
            {
                csv.AppendLine($"{batch.Id},{batch.ActualWeightKg},{batch.Operator},{batch.MixedDateTime:dd.MM.yyyy HH:mm}");
            }
            csv.AppendLine();
        }

        // Pallets
        if (order.Pallets.Any())
        {
            csv.AppendLine("PALETY");
            csv.AppendLine("Nr Palety,Liczba Worków,Waga (kg),Status,Data Utworzenia");
            foreach (var pallet in order.Pallets.OrderBy(p => p.CreatedDate))
            {
                csv.AppendLine($"{pallet.PalletNumber},{pallet.BagCount},{pallet.TotalWeightKg},{GetPalletStatusText(pallet.Status)},{pallet.CreatedDate:dd.MM.yyyy HH:mm}");
            }
            csv.AppendLine();
        }

        // Summary
        csv.AppendLine("PODSUMOWANIE");
        csv.AppendLine($"Liczba szarż,{order.BatchCount}");
        csv.AppendLine($"Liczba palet,{order.Pallets.Count}");
        csv.AppendLine($"Łączna waga palet (kg),{order.Pallets.Sum(p => p.TotalWeightKg)}");
        csv.AppendLine($"Procent gotowości,{Math.Round((order.TotalMixedWeight / (decimal)order.PlannedWeightKg * 100), 2)}%");

        _logger.LogInformation("Generated CSV report for order {OrderId}", order.Id);

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<byte[]> ExportOrdersToExcelAsync(List<ProductionOrder> orders)
    {
        var csv = new StringBuilder();

        csv.AppendLine("Raport Produkcji - Wszystkie Zlecenia");
        csv.AppendLine($"Data generowania,{DateTime.Now:dd.MM.yyyy HH:mm:ss}");
        csv.AppendLine();

        csv.AppendLine("ID,Produkt,Receptura,Typ Worka,Plan (kg),Wymieszano (kg),Pozostało (kg),Status,Szarże,Palety,Data");
        
        foreach (var order in orders.OrderBy(o => o.CreatedDate))
        {
            csv.AppendLine($"{order.Id}," +
                $"{order.ProductName}," +
                $"{order.RecipeNumber}," +
                $"{order.BagType.GetDisplayName()}," +
                $"{order.PlannedWeightKg}," +
                $"{order.TotalMixedWeight}," +
                $"{order.RemainingWeight}," +
                $"{GetStatusText(order.Status)}," +
                $"{order.BatchCount}," +
                $"{order.Pallets.Count}," +
                $"{order.CreatedDate:dd.MM.yyyy HH:mm}");
        }

        _logger.LogInformation("Generated CSV report for {OrderCount} orders", orders.Count);

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public string GenerateOrderSummaryReport(ProductionOrder order)
    {
        var report = new StringBuilder();

        report.AppendLine("=== RAPORT PRODUKCJI ===");
        report.AppendLine($"Data: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
        report.AppendLine();

        report.AppendLine("INFORMACJE O ZLECENIU:");
        report.AppendLine($"  Produkt: {order.ProductName}");
        report.AppendLine($"  Receptura: {order.RecipeNumber}");
        report.AppendLine($"  Typ worka: {order.BagType.GetDisplayName()}");
        report.AppendLine($"  Status: {GetStatusText(order.Status)}");
        report.AppendLine();

        report.AppendLine("POSTĘP REALIZACJI:");
        report.AppendLine($"  Planowana waga: {order.PlannedWeightKg} kg");
        report.AppendLine($"  Wymieszana waga: {order.TotalMixedWeight} kg");
        report.AppendLine($"  Pozostała waga: {order.RemainingWeight} kg");
        report.AppendLine($"  Procent ukończenia: {Math.Round((order.TotalMixedWeight / (decimal)order.PlannedWeightKg * 100), 2)}%");
        report.AppendLine();

        report.AppendLine("STATYSTYKI:");
        report.AppendLine($"  Liczba szarż: {order.BatchCount}");
        report.AppendLine($"  Liczba palet: {order.Pallets.Count}");
        if (order.Pallets.Any())
        {
            var packedPallets = order.Pallets.Count(p => p.Status == PalletStatus.Packed);
            var shippedPallets = order.Pallets.Count(p => p.Status == PalletStatus.Shipped);
            report.AppendLine($"    - Oczekujące: {order.Pallets.Count(p => p.Status == PalletStatus.Pending)}");
            report.AppendLine($"    - Spakowane: {packedPallets}");
            report.AppendLine($"    - Wysłane: {shippedPallets}");
            report.AppendLine($"    - Zakończone: {order.Pallets.Count(p => p.Status == PalletStatus.Completed)}");
        }
        report.AppendLine();

        return report.ToString();
    }

    private string GetStatusText(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Planned => "Planowane",
            OrderStatus.InProgress => "W trakcie",
            OrderStatus.ReadyForBagging => "Gotowe do workowania",
            OrderStatus.Bagging => "Workowanie",
            OrderStatus.Completed => "Zakończone",
            _ => status.ToString()
        };
    }

    private string GetPalletStatusText(PalletStatus status)
    {
        return status switch
        {
            PalletStatus.Pending => "Oczekujące",
            PalletStatus.Packed => "Spakowane",
            PalletStatus.Shipped => "Wysłane",
            PalletStatus.Completed => "Zakończone",
            _ => status.ToString()
        };
    }
}
