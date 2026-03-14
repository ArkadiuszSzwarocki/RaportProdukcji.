namespace RaportProdukcji.Models;

public class Pallet
{
    public int Id { get; set; }
    public int ProductionOrderId { get; set; }
    public string PalletNumber { get; set; } = string.Empty;
    public decimal TotalWeightKg { get; set; }
    public int BagCount { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public PalletStatus Status { get; set; } = PalletStatus.Pending;
    
    // Navigation property
    public virtual ProductionOrder? ProductionOrder { get; set; }
}

public enum PalletStatus
{
    Pending,
    Packed,
    Shipped,
    Completed
}
