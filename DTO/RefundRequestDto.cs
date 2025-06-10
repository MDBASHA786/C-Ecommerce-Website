using System.ComponentModel.DataAnnotations;

namespace QuitQ1_Hx.DTO
{
    public class RefundRequestDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; }
    }
}
