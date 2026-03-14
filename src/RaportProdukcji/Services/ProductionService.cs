using Microsoft.EntityFrameworkCore;
using RaportProdukcji.Data;
using RaportProdukcji.Models;

namespace RaportProdukcji.Services;

public interface IProductionService
{
    Task<List<ProductionOrder>> GetAllOrdersAsync();
    Task<ProductionOrder?> GetOrderByIdAsync(int id);
    Task<ProductionOrder> AddOrderAsync(ProductionOrder order);
    Task<ProductionOrder> UpdateOrderAsync(ProductionOrder order);
    Task DeleteOrderAsync(int id);
    Task<Batch> AddBatchToOrderAsync(int orderId, Batch batch);
    Task<List<ProductionOrder>> GetOrdersByStatusAsync(OrderStatus status);

    // Sync wrappers for Razor components
    List<ProductionOrder> GetAllOrders() => GetAllOrdersAsync().Result;
    ProductionOrder? GetOrderById(int id) => GetOrderByIdAsync(id).Result;
    void AddOrder(ProductionOrder order) => AddOrderAsync(order).Wait();
    void UpdateOrder(ProductionOrder order) => UpdateOrderAsync(order).Wait();
    void DeleteOrder(int id) => DeleteOrderAsync(id).Wait();
    void AddBatchToOrder(int orderId, Batch batch) => AddBatchToOrderAsync(orderId, batch).Wait();
    List<ProductionOrder> GetOrdersByStatus(OrderStatus status) => GetOrdersByStatusAsync(status).Result;
}

public class ProductionService : IProductionService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductionService> _logger;

    public ProductionService(AppDbContext context, ILogger<ProductionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ProductionOrder>> GetAllOrdersAsync()
    {
        return await _context.ProductionOrders
            .Include(o => o.Batches)
            .Include(o => o.Pallets)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync();
    }

    public async Task<ProductionOrder?> GetOrderByIdAsync(int id)
    {
        return await _context.ProductionOrders
            .Include(o => o.Batches)
            .Include(o => o.Pallets)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<ProductionOrder> AddOrderAsync(ProductionOrder order)
    {
        _context.ProductionOrders.Add(order);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created production order {OrderId}: {ProductName}", order.Id, order.ProductName);
        return order;
    }

    public async Task<ProductionOrder> UpdateOrderAsync(ProductionOrder order)
    {
        _context.ProductionOrders.Update(order);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated production order {OrderId}", order.Id);
        return order;
    }

    public async Task DeleteOrderAsync(int id)
    {
        var order = await _context.ProductionOrders.FindAsync(id);
        if (order != null)
        {
            _context.ProductionOrders.Remove(order);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deleted production order {OrderId}", id);
        }
    }

    public async Task<Batch> AddBatchToOrderAsync(int orderId, Batch batch)
    {
        var order = await _context.ProductionOrders.FindAsync(orderId);
        if (order == null)
            throw new ArgumentException($"Order {orderId} not found");

        batch.ProductionOrderId = orderId;
        batch.MixedDateTime = DateTime.Now;

        _context.Batches.Add(batch);

        // Update order status based on total weight
        var totalWeight = await _context.Batches
            .Where(b => b.ProductionOrderId == orderId)
            .SumAsync(b => b.ActualWeightKg) + batch.ActualWeightKg;

        if (totalWeight >= order.PlannedWeightKg)
            order.Status = OrderStatus.ReadyForBagging;
        else if (order.Status == OrderStatus.Planned)
            order.Status = OrderStatus.InProgress;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Added batch {BatchId} to order {OrderId}: {Weight}kg", batch.Id, orderId, batch.ActualWeightKg);
        return batch;
    }

    public async Task<List<ProductionOrder>> GetOrdersByStatusAsync(OrderStatus status)
    {
        return await _context.ProductionOrders
            .Include(o => o.Batches)
            .Include(o => o.Pallets)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync();
    }
}
