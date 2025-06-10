using QuitQ1_Hx.DTO;
using System.Threading.Tasks;

namespace QuitQ1_Hx.Services
{
    public interface ITempItemsService
    {
        Task<CartItemDto> GetBuyNowItemAsync(string userId);
        Task SaveBuyNowItemAsync(string userId, CartItemDto item);
    }
}