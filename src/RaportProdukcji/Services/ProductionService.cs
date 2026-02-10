using RaportProdukcji.Models;

namespace RaportProdukcji.Services;

public interface IProductionService
{
    List<ProductionOrder> GetAllOrders();
    ProductionOrder? GetOrderById(int id);
    void AddOrder(ProductionOrder order);
    void UpdateOrder(ProductionOrder order);
    void DeleteOrder(int id);
    void AddBatchToOrder(int orderId, Batch batch);
    List<ProductionOrder> GetOrdersByStatus(OrderStatus status);
}

public class ProductionService : IProductionService
{
    private readonly List<ProductionOrder> _orders = new();
    private int _nextOrderId = 1;
    private int _nextBatchId = 1;

    public List<ProductionOrder> GetAllOrders()
    {
        return _orders.OrderByDescending(o => o.CreatedDate).ToList();
    }

    public ProductionOrder? GetOrderById(int id)
    {
        return _orders.FirstOrDefault(o => o.Id == id);
    }

    public void AddOrder(ProductionOrder order)
    {
        order.Id = _nextOrderId++;
        order.CreatedDate = DateTime.Now;
        _orders.Add(order);
    }

    public void UpdateOrder(ProductionOrder order)
    {
        var existing = _orders.FirstOrDefault(o => o.Id == order.Id);
        if (existing != null)
        {
            var index = _orders.IndexOf(existing);
            _orders[index] = order;
        }
    }

    public void DeleteOrder(int id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order != null)
        {
            _orders.Remove(order);
        }
    }

    public void AddBatchToOrder(int orderId, Batch batch)
    {
        var order = _orders.FirstOrDefault(o => o.Id == orderId);
        if (order != null)
        {
            batch.Id = _nextBatchId++;
            batch.ProductionOrderId = orderId;
            batch.MixedDateTime = DateTime.Now;
            order.Batches.Add(batch);

            // Update order status based on weight
            if (order.TotalMixedWeight >= order.PlannedWeightKg)
            {
                order.Status = OrderStatus.ReadyForBagging;
            }
            else if (order.Status == OrderStatus.Planned)
            {
                order.Status = OrderStatus.InProgress;
            }
        }
    }

    public List<ProductionOrder> GetOrdersByStatus(OrderStatus status)
    {
        return _orders.Where(o => o.Status == status).OrderByDescending(o => o.CreatedDate).ToList();
    }
}
