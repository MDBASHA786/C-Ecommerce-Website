using Microsoft.EntityFrameworkCore;
using QuitQ1_Hx.Data;
using QuitQ1_Hx.Models;
using QuitQ1_Hx.DTO;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace QuitQ1_Hx.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CartService> _logger;

        public CartService(ApplicationDbContext context, ILogger<CartService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Cart> GetCartAsync(string userId)
        {
            // First, ensure the user has a cart
            var cart = await GetOrCreateCartAsync(userId);

            // Get cart items with product details
            var cartItems = await _context.CartItems
                .Include(ci => ci.Cart)
                .Where(ci => ci.Cart != null && ci.Cart.UserId == GetUserIdAsInt(userId))
                .Select(ci => new CartItemDto
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.Price
                })
                .ToListAsync();

            // Return cart with items
            return new Cart
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CartItems = cartItems.Select(ci => new CartItem
                {
                    Id = ci.Id,
                    CartId = cart.Id,
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.Price
                }).ToList(),
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt
            };
        }

        public async Task<CartItem> AddToCartAsync(string userId, int productId, int quantity)
        {
            try
            {
                _logger.LogInformation("Adding product {ProductId} to cart for user {UserId} with quantity {Quantity}",
                    productId, userId, quantity);

                // Get or create the user's cart
                var cart = await GetOrCreateCartAsync(userId);

                // Find the product
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found", productId);
                    throw new InvalidOperationException($"Product with ID {productId} not found");
                }

                // Check if item already exists in cart
                var existingItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId);

                if (existingItem != null)
                {
                    // Update quantity of existing item
                    existingItem.Quantity += quantity;
                    _context.Entry(existingItem).State = EntityState.Modified;
                    _logger.LogInformation("Updated quantity of existing cart item to {NewQuantity}", existingItem.Quantity);
                }
                else
                {
                    // Create new cart item
                    var cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = productId,
                        Quantity = quantity,
                        Price = product.Price
                    };

                    _context.CartItems.Add(cartItem);
                    existingItem = cartItem;
                    _logger.LogInformation("Created new cart item");
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully saved cart changes");

                // Return the added or updated cart item
                return existingItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddToCartAsync");
                throw; // Re-throw to be handled by controller
            }
        }

        public async Task<bool> RemoveFromCartAsync(string userId, int cartItemId)
        {
            // Find the cart item and ensure it belongs to the user
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.UserId == GetUserIdAsInt(userId));

            if (cartItem == null)
                return false;

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateCartItemAsync(string userId, int cartItemId, int quantity)
        {
            // Find the cart item and ensure it belongs to the user
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.UserId == GetUserIdAsInt(userId));

            if (cartItem == null)
                return false;

            // Update the quantity
            cartItem.Quantity = quantity;
            _context.Entry(cartItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            // Find the user's cart
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == GetUserIdAsInt(userId));

            if (cart == null)
                return false;

            // Remove all items from cart
            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<Cart> GetOrCreateCartAsync(string userId)
        {
            int userIdInt = GetUserIdAsInt(userId);

            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userIdInt);

            if (cart == null)
            {
                // Create new cart if one doesn't exist
                cart = new Cart
                {
                    UserId = userIdInt,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Created new cart for user {UserId}", userId);
            }

            return cart;
        }

        // Helper method to safely convert user ID string to int
        private int GetUserIdAsInt(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("User ID is null or empty");
                throw new ArgumentException("User ID cannot be null or empty");
            }
            try
            {
                return int.Parse(userId);
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Failed to parse user ID '{UserId}' as integer", userId);
                throw new FormatException($"User ID '{userId}' is not a valid integer", ex);
            }
        }
    }
}