namespace QuitQ1_Hx.DTO
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; }
        public string MainImageUrl { get; set; }
        public List<string> ImageUrls { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Category info
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        // Seller info
        public int SellerId { get; set; }
        public string SellerName { get; set; }
    }
}
