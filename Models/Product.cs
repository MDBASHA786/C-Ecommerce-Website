using NuGet.Protocol.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuitQ1_Hx.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]

        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
       
        public bool IsAvailable { get; set; } = true;
        public string MainImageUrl { get; set; } = string.Empty;
        public List<string>? ImageUrls { get; set; } = new List<string>(); 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys

        [ForeignKey("CategoryId")]
        public int CategoryId { get; set; }

        [ForeignKey("SellerId")]
        public int SellerId { get; set; }
       
        // Navigation Properties
        public Category? Category { get; set; }

        public string SellerName { get; set; }
        public Seller? Seller { get; set; }
        // Add this to your Product class
        public ICollection<RefundRequest> RefundRequests { get; set; } = new List<RefundRequest>();
    }
}
