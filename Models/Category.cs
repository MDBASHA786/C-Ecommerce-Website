using QuitQ1_Hx.Models;
using System.Text.Json.Serialization;
namespace QuitQ1_Hx;



public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Navigation properties
    [JsonIgnore] // Add this attribute to break the cycle
    public ICollection<Product> Product { get; set; }
}