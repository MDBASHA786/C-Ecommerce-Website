using QuitQ1_Hx.DTO;

using QuitQ1_Hx.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuitQ1_Hx.Services
{
    public class TempItemsService : ITempItemsService
    {
        private readonly Dictionary<string, CartItemDto> _buyNowItems = new();

        public Task<CartItemDto> GetBuyNowItemAsync(string userId)
        {
            if (_buyNowItems.TryGetValue(userId, out var item))
            {
                return Task.FromResult(item);
            }
            return Task.FromResult<CartItemDto>(null);
        }

        public Task SaveBuyNowItemAsync(string userId, CartItemDto item)
        {
            _buyNowItems[userId] = item;
            return Task.CompletedTask;
        }
    }
}