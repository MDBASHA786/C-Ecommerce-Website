using QuitQ1_Hx.Models;

namespace QuitQ1_Hx.Services
{
    public interface ICartService
    {
        Task<Cart> GetCartAsync(string userId);
        Task<CartItem> AddToCartAsync(string userId, int productId, int quantity);
        Task<bool> RemoveFromCartAsync(string userId, int cartItemId);
        Task<bool> UpdateCartItemAsync(string userId, int cartItemId, int quantity);
        Task<bool> ClearCartAsync(string userId);
    }
}
