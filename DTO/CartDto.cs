

    namespace QuitQ1_Hx.DTO
    {
       

        public class CreateCartItemDto
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; } = 1;
            public decimal Price { get; set; }
        }

        public class UpdateCartItemDto
        {
            public int Quantity { get; set; }
            public decimal? Price { get; set; }
        }

        public class AddToCartModel
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; } = 1;
        }

        
    }
