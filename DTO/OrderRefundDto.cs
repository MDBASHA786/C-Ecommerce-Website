using System.ComponentModel.DataAnnotations;

namespace QuitQ1_Hx.DTO
{
    
        public class OrderRefundDto
        {
            [Required]
            public int ProductId { get; set; }

            [Required]
            [MaxLength(500)]
            public string Reason { get; set; }
        }
 

    public class ProcessRefundDto
    {
        [Required]
        public bool Approve { get; set; }

        [MaxLength(1000)]
        public string AdminNotes { get; set; }
    }
}

