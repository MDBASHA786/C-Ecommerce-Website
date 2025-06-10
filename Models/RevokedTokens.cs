using System;
using System.ComponentModel.DataAnnotations;

namespace QuitQ1_Hx.Models
{
    public class RevokedTokens
    {
        [Key]
        public int Id { get; set; }
        public required string Token { get; set; }
        // Other properties...
        
        public DateTime RevokedAt { get; set; }
    }
}