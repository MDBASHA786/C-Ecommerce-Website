using QuitQ1_Hx.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuitQ1_Hx.DTO
{
    
        public class CartItemDto
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }

        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal Total { get; set; }
        public int UserId { get; set; }

    }
    public class AddToCartDto
    {
        public int ProductId { get; set; }

        public int Quantity { get; set; } = 1;

        public bool IsBuyNow { get; set; } = false;
    }
}

