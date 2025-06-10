using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuitQ1_Hx.Models
{
    public class Seller
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
        
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    }
}