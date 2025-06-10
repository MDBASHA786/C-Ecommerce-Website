using System.ComponentModel.DataAnnotations;

namespace QuitQ1_Hx.DTO
{
    public class OrderUpdateDto
    {

        [Required]
        [RegularExpression("^(Processing|Shipped|Delivered)$", ErrorMessage = "Invalid status.")]
        public string Status { get; set; }
    }
}
public class AddToCartDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public bool IsBuyNow { get; set; }
}



public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public int Id { get; set; }

    public string ProductName { get; set; }
   
    public string ImageUrl { get; set; }
    public decimal Total { get; set; }

}

    
    public class PaymentDto
    {
        public string CardNumber { get; set; }
        public string CardholderName { get; set; }
        public string ExpirationDate { get; set; }
        public string CVV { get; set; }
    }
