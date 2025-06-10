using System.ComponentModel.DataAnnotations.Schema;

namespace QuitQ1_Hx.Models;
public class Cart
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    // Navigation property - this is what was missing
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public User? User { get; set; }
    
}

public class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    [ForeignKey("ProductId")]
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }


    public Cart? Cart { get; set; }
    public Product? Product { get; set; }

  
    public DateTime DateAdded { get; set; }

    public bool IsBuyNow { get; set; }

    

}