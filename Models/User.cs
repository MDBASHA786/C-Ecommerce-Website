using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuitQ1_Hx.Models
{
    public enum UserRole
    {
        customer,
        seller,
        Administrator
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; 
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.customer;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public List<Address>? Addresses { get; set; }
        public List<Product>? Products { get; set; }
        public List<Order>? Orders { get; set; }
       
        public Cart? Cart { get; set; }
    }
}
