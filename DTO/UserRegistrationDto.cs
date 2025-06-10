using System.Text.Json.Serialization;
using QuitQ1_Hx.Models;

namespace QuitQ1_Hx.DTO
{
    public class UserRegistrationDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        [JsonPropertyName("role")]  // Add this attribute
        public UserRole Role { get; set; } = UserRole.customer;
    }
}
