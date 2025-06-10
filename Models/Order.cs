namespace QuitQ1_Hx.Models;
public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public int? ShippingAddressId { get; set; }

    public string PaymentIntentId { get; set; }
    public User? User { get; set; }
    public Address? ShippingAddress { get; set; }
    public List<OrderItem>? Items { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    
    public Order? Order { get; set; }
    public Product? Product { get; set; }
}