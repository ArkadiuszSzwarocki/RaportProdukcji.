namespace RaportProdukcji.Models;

public enum BagType
{
    WelledBag20kg,
    WelledBag25kg,
    SewnBag20kg,
    SewnBag25kg,
    BigBag
}

public static class BagTypeExtensions
{
    public static string GetDisplayName(this BagType bagType)
    {
        return bagType switch
        {
            BagType.WelledBag20kg => "Worki zgrzewane 20kg",
            BagType.WelledBag25kg => "Worki zgrzewane 25kg",
            BagType.SewnBag20kg => "Worki szyte 20kg",
            BagType.SewnBag25kg => "Worki szyte 25kg",
            BagType.BigBag => "BigBag",
            _ => bagType.ToString()
        };
    }

    public static int GetBaggingTimeForThousandKg(this BagType bagType)
    {
        return bagType switch
        {
            BagType.WelledBag20kg => 20,
            BagType.WelledBag25kg => 20,
            BagType.SewnBag20kg => 30,
            BagType.SewnBag25kg => 30,
            BagType.BigBag => 15,
            _ => 20
        };
    }

    public static double CalculateBaggingTime(this BagType bagType, decimal weightKg)
    {
        var timeForThousandKg = bagType.GetBaggingTimeForThousandKg();
        return (double)(weightKg / 1000m) * timeForThousandKg;
    }
}
