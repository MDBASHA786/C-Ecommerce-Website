using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace QuitQ1_Hx.Models
{
    public enum RefundStatus
    {
        Pending,
        Approved,
        Rejected,
        Processed
    }
    public class RefundRequest
    {
        public int Id { get; set; }
        [Required]
        [ForeignKey("OrderId")]
        public int OrderId { get; set; }
        [Required]
        [ForeignKey("ProductId")]
        public int ProductId { get; set; }
        [Required]
        [ForeignKey("UserId")]
        public int UserId { get; set; }  // Change to int since User.Id is an int
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundAmount { get; set; }
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; }
        public string? AdminNotes { get; set; }
        [Required]
        public RefundStatus Status { get; set; } = RefundStatus.Pending;
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        // Navigation properties
        public Product Product { get; set; }
        // Assuming you have Order and User models
        public Order Order { get; set; }
        public User User { get; set; }  // Changed from ApplicationUser to User
    }
}