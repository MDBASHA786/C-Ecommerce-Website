using QuitQ1_Hx.Models;

public class UserProfileDto
{
    public int Id { get; set; } // Changed from string to int to match User model
    public string Username { get; set; } = string.Empty; // Initialize with empty string
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; } // Made nullable
    public string? LastName { get; set; } // Made nullable
    public string? PhoneNumber { get; set; } // Added missing property
    public UserRole Role { get; set; } // Added missing property
    public DateTime CreatedAt { get; set; } // Added missing property
}