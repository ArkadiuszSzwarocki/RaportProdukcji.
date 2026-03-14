using Microsoft.EntityFrameworkCore;
using RaportProdukcji.Data;
using RaportProdukcji.Models;

namespace RaportProdukcji.Services;

public interface IPalletService
{
    Task<List<Pallet>> GetPalletsForOrderAsync(int orderId);
    Task<Pallet?> GetPalletByIdAsync(int id);
    Task<Pallet> CreatePalletAsync(int orderId, int bagCount);
    Task<Pallet> UpdatePalletAsync(int id, decimal weight, int bagCount);
    Task<Pallet> UpdatePalletStatusAsync(int id, PalletStatus status);
    Task DeletePalletAsync(int id);
    
    // Sync wrappers for Razor components
    List<Pallet> GetPalletsForOrder(int orderId);
    Pallet? GetPalletById(int id);
    Pallet CreatePallet(int orderId, int bagCount);
    Pallet UpdatePalletStatus(int id, PalletStatus status);
    void DeletePallet(int id);
}

public class PalletService : IPalletService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PalletService> _logger;

    public PalletService(AppDbContext context, ILogger<PalletService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Pallet>> GetPalletsForOrderAsync(int orderId)
    {
        return await _context.Pallets
            .Where(p => p.ProductionOrderId == orderId)
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();
    }

    public async Task<Pallet?> GetPalletByIdAsync(int id)
    {
        return await _context.Pallets
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Pallet> CreatePalletAsync(int orderId, int bagCount)
    {
        var order = await _context.ProductionOrders
            .FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
            throw new ArgumentException($"Order {orderId} not found");

        // Auto calculate weight based on bag type and count
        var expectedWeightPerBag = CalculateWeightPerBag(order.BagType);
        var totalWeight = expectedWeightPerBag * bagCount;

        // Generate pallet number (e.g., P-2025-0001)
        var palletCount = await _context.Pallets
            .Where(p => p.ProductionOrderId == orderId)
            .CountAsync();
        var palletNumber = $"P-{DateTime.Now.Year}-{(palletCount + 1):D4}";

        var pallet = new Pallet
        {
            ProductionOrderId = orderId,
            PalletNumber = palletNumber,
            TotalWeightKg = totalWeight,
            BagCount = bagCount,
            Status = PalletStatus.Pending
        };

        _context.Pallets.Add(pallet);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created pallet {PalletNumber} for order {OrderId} with {BagCount} bags", 
            palletNumber, orderId, bagCount);

        return pallet;
    }

    public async Task<Pallet> UpdatePalletAsync(int id, decimal weight, int bagCount)
    {
        var pallet = await _context.Pallets.FirstOrDefaultAsync(p => p.Id == id);
        if (pallet == null)
            throw new ArgumentException($"Pallet {id} not found");

        pallet.TotalWeightKg = weight;
        pallet.BagCount = bagCount;

        _context.Pallets.Update(pallet);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated pallet {PalletId}: weight={Weight}kg, bags={BagCount}", 
            id, weight, bagCount);

        return pallet;
    }

    public async Task<Pallet> UpdatePalletStatusAsync(int id, PalletStatus status)
    {
        var pallet = await _context.Pallets.FirstOrDefaultAsync(p => p.Id == id);
        if (pallet == null)
            throw new ArgumentException($"Pallet {id} not found");

        pallet.Status = status;
        _context.Pallets.Update(pallet);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated pallet {PalletId} status to {Status}", id, status);

        return pallet;
    }

    public async Task DeletePalletAsync(int id)
    {
        var pallet = await _context.Pallets.FirstOrDefaultAsync(p => p.Id == id);
        if (pallet != null)
        {
            _context.Pallets.Remove(pallet);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deleted pallet {PalletId}", id);
        }
    }

    private decimal CalculateWeightPerBag(BagType bagType)
    {
        return bagType switch
        {
            BagType.WelledBag20kg => 20m,
            BagType.WelledBag25kg => 25m,
            BagType.SewnBag20kg => 20m,
            BagType.SewnBag25kg => 25m,
            BagType.BigBag => 50m,
            _ => 20m
        };
    }

    // Sync wrappers for Razor components
    public List<Pallet> GetPalletsForOrder(int orderId)
        => GetPalletsForOrderAsync(orderId).Result;

    public Pallet? GetPalletById(int id)
        => GetPalletByIdAsync(id).Result;

    public Pallet CreatePallet(int orderId, int bagCount)
        => CreatePalletAsync(orderId, bagCount).Result;

    public Pallet UpdatePalletStatus(int id, PalletStatus status)
        => UpdatePalletStatusAsync(id, status).Result;

    public void DeletePallet(int id)
        => DeletePalletAsync(id).Wait();
}
