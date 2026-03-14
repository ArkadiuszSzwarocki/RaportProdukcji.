namespace RaportProdukcji.Models;

public class ProductionOrder
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public BagType BagType { get; set; }
    public decimal PlannedWeightKg { get; set; }
    public string RecipeNumber { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public OrderStatus Status { get; set; } = OrderStatus.Planned;
    public List<Batch> Batches { get; set; } = new();
    public List<Pallet> Pallets { get; set; } = new();

    public decimal TotalMixedWeight => Batches.Sum(b => b.ActualWeightKg);
    public int BatchCount => Batches.Count;
    public decimal RemainingWeight => PlannedWeightKg - TotalMixedWeight;
    public double EstimatedBaggingTime => BagType.CalculateBaggingTime(TotalMixedWeight);
}

public enum OrderStatus
{
    Planned,
    InProgress,
    ReadyForBagging,
    Bagging,
    Completed
}
