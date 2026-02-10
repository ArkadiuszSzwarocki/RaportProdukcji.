namespace RaportProdukcji.Models;

public class Batch
{
    public int Id { get; set; }
    public int ProductionOrderId { get; set; }
    public decimal ActualWeightKg { get; set; }
    public DateTime MixedDateTime { get; set; } = DateTime.Now;
    public string Operator { get; set; } = string.Empty;
}
