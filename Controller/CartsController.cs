using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuitQ1_Hx.Services;
using System.Security.Claims;
using QuitQ1_Hx.DTO;
using QuitQ1_Hx.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuitQ1_Hx.Data;
using QuitQ1_Hx.Repositories;

namespace QuitQ1_Hx.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<CartController> _logger;
        private readonly ITempItemsService _tempItemsService;
        private readonly IProductRepository _productRepository;

        public CartController(
            ICartService cartService,
            ApplicationDbContext dbContext,
            ILogger<CartController> logger,
            ITempItemsService tempItemsService,
            IProductRepository productRepository)
        {
            _cartService = cartService;
            _dbContext = dbContext;
            _logger = logger;
            _tempItemsService = tempItemsService;
            _productRepository = productRepository;
        }

        private async Task<string> GetUserIdAsync()
        {
            try
            {
                // Try standard claims first
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    _logger.LogInformation("Found user ID in NameIdentifier claim: {UserId}", userId);
                    // Verify it's a valid integer
                    if (int.TryParse(userId, out _))
                    {
                        return userId;
                    }
                    else
                    {
                        _logger.LogWarning("User ID from NameIdentifier claim is not a valid integer: {UserId}", userId);
                    }
                }

                var subClaim = User.FindFirstValue("sub");
                if (!string.IsNullOrEmpty(subClaim))
                {
                    _logger.LogInformation("Found user ID in sub claim: {UserId}", subClaim);
                    // Verify it's a valid integer
                    if (int.TryParse(subClaim, out _))
                    {
                        return subClaim;
                    }
                    else
                    {
                        _logger.LogWarning("User ID from sub claim is not a valid integer: {UserId}", subClaim);
                    }
                }

                // Get the username from the token
                var username = User.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrEmpty(username))
                {
                    username = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
                    if (string.IsNullOrEmpty(username))
                    {
                        _logger.LogWarning("No username claim found in token");
                        throw new InvalidOperationException("Username not found in claims");
                    }
                }

                _logger.LogInformation("Looking up user by name/email: {Username}", username);

                // Since we don't have a UserName property, let's create a more flexible query
                // that checks multiple fields where the username might be stored
                var user = await _dbContext.Users
                    .Where(u => u.Email == username ||
                           EF.Property<string>(u, "UserName") == username ||
                           EF.Property<string>(u, "Name") == username)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    // As a fallback, let's try to find a user by looking for the username in any string field
                    _logger.LogInformation("User not found by direct lookup, trying to find by name or email");

                    // Directly query for the user with the matching username
                    // This assumes you have some way to identify users by username in your database
                    var sql = $"SELECT * FROM Users WHERE Email = '{username}' OR Name = '{username}' OR UserName = '{username}'";
                    user = await _dbContext.Users.FromSqlRaw(sql).FirstOrDefaultAsync();
                }

                if (user == null)
                {
                    _logger.LogWarning("No user found with username: {Username}", username);

                    // If we can't find the user, create a temporary debug user
                    // This is just for debugging and should be removed in production
                    _logger.LogWarning("Creating temporary debug user for testing");
                    var tempUser = new { Id = 1 }; // Temporary user with ID 1
                    return tempUser.Id.ToString();

                    // In production, you should throw an exception instead:
                    // throw new InvalidOperationException($"User not found with username: {username}");
                }

                _logger.LogInformation("Found user with ID: {UserId}", user.Id);

                // Ensure the user ID is actually an integer
                if (!int.TryParse(user.Id.ToString(), out _))
                {
                    _logger.LogWarning("User ID is not an integer: {UserId}", user.Id);
                    throw new FormatException($"User ID {user.Id} cannot be converted to an integer");
                }

                return user.Id.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user ID from claims: {Message}", ex.Message);
                // For debugging purposes, return a default user ID
                // Remove this in production and let the exception propagate
                return "1";
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                var userId = await GetUserIdAsync();
                _logger.LogInformation("Getting cart for user ID: {UserId}", userId);

                var cart = await _cartService.GetCartAsync(userId);
                return Ok(cart);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when getting cart");
                return BadRequest(new { message = ex.Message });
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Format exception when getting cart");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart: {Exception}", ex);
                return StatusCode(500, new { message = "An error occurred while retrieving the cart.", details = ex.Message });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartModel model)
        {
            _logger.LogInformation("AddToCart called with ProductId: {ProductId}, Quantity: {Quantity}",
                model?.ProductId, model?.Quantity);

            try
            {
                if (model == null)
                {
                    _logger.LogWarning("AddToCart called with null model");
                    return BadRequest(new { message = "Request body is required" });
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state in AddToCart: {ModelState}",
                        string.Join("; ", ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)));
                    return BadRequest(ModelState);
                }

                // Validate product exists before proceeding
                var product = await _dbContext.Products.FindAsync(model.ProductId);
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found", model.ProductId);
                    return BadRequest(new { message = $"Product with ID {model.ProductId} not found" });
                }

                var userId = await GetUserIdAsync();
                _logger.LogInformation("Adding item to cart for user ID: {UserId}", userId);

                var cartItem = await _cartService.AddToCartAsync(userId, model.ProductId, model.Quantity);
                return Ok(cartItem);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when adding to cart");
                return BadRequest(new { message = ex.Message });
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Format exception when adding to cart: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to cart: {Exception}", ex);
                // Include more detail in the response to help debugging
                return StatusCode(500, new
                {
                    message = "An error occurred while adding to cart.",
                    details = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        [HttpDelete("remove/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            try
            {
                var userId = await GetUserIdAsync();
                _logger.LogInformation("Removing item {CartItemId} from cart for user ID: {UserId}", cartItemId, userId);

                var success = await _cartService.RemoveFromCartAsync(userId, cartItemId);
                if (!success)
                {
                    _logger.LogWarning("Cart item {CartItemId} not found for user {UserId}", cartItemId, userId);
                    return NotFound(new { message = $"Cart item with ID {cartItemId} not found" });
                }
                return Ok(new { message = "Item removed from cart successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when removing from cart");
                return BadRequest(new { message = ex.Message });
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Format exception when removing from cart");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing from cart: {Exception}", ex);
                return StatusCode(500, new
                {
                    message = "An error occurred while removing from cart.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("buyNow")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CartItemDto>>> GetBuyNowItem()
        {
            try
            {
                var userId = await GetUserIdAsync();
                _logger.LogInformation("Getting buy now item for user ID: {UserId}", userId);

                var buyNowItem = await _tempItemsService.GetBuyNowItemAsync(userId);

                if (buyNowItem == null)
                {
                    return NotFound("No buy now item found");
                }

                return Ok(new List<CartItemDto> { buyNowItem });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting buy now item: {Exception}", ex);
                return StatusCode(500, new { message = "An error occurred while retrieving the buy now item.", details = ex.Message });
            }
        }

        [HttpPost("buyNow")]
        [Authorize]
        public async Task<ActionResult<CartItemDto>> SetBuyNowItem([FromBody] AddToCartDto request)
        {
            try
            {
                var userId = await GetUserIdAsync();
                _logger.LogInformation("Setting buy now item for user ID: {UserId}", userId);

                // Validate product exists
                var product = await _productRepository.GetByIdAsync(request.ProductId);
                if (product == null)
                {
                    return NotFound("Product not found");
                }

                // Create a temporary "Buy Now" item
                var buyNowItem = new CartItemDto
                {
                    Id = 0, // Use a temporary ID
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = request.Quantity,
                    ImageUrl = product.MainImageUrl // Corrected property name
                    // Add other necessary properties
                };

                // Store this item temporarily
                await _tempItemsService.SaveBuyNowItemAsync(userId, buyNowItem);

                return Ok(buyNowItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting buy now item: {Exception}", ex);
                return StatusCode(500, new { message = "An error occurred while setting the buy now item.", details = ex.Message });
            }
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userId = await GetUserIdAsync();
                _logger.LogInformation("Clearing cart for user ID: {UserId}", userId);

                await _cartService.ClearCartAsync(userId);
                return Ok(new { message = "Cart cleared successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when clearing cart");
                return BadRequest(new { message = ex.Message });
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Format exception when clearing cart");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart: {Exception}", ex);
                return StatusCode(500, new
                {
                    message = "An error occurred while clearing the cart.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("debug")]
        [AllowAnonymous] // So you can access it even if auth is broken
        public IActionResult DebugCartService()
        {
            try
            {
                // Collect all claims from current user
                var claims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList();

                // Check if email claim exists (try multiple possible claim types)
                var emailClaim = User.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrEmpty(emailClaim))
                {
                    emailClaim = User.FindFirstValue(ClaimTypes.Email);
                }
                if (string.IsNullOrEmpty(emailClaim))
                {
                    emailClaim = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
                }

                // Try to find user with this email
                var user = emailClaim != null ? _dbContext.Users.FirstOrDefault(u => u.Email == emailClaim) : null;

                // Check if cart exists for this user
                var cart = user != null ? _dbContext.Carts.FirstOrDefault(c => c.UserId == user.Id) : null;

                // Check if the requested product exists
                var product = _dbContext.Products.Find(11); // Check the specific product ID from the request

                return Ok(new
                {
                    IsAuthenticated = User.Identity.IsAuthenticated,
                    Claims = claims,
                    EmailClaimExists = !string.IsNullOrEmpty(emailClaim),
                    EmailClaim = emailClaim,
                    UserExists = user != null,
                    UserId = user?.Id,
                    UserIdType = user?.Id.GetType().FullName,
                    CartExists = cart != null,
                    CartId = cart?.Id,
                    ProductExists = product != null,
                    ProductId = product?.Id,
                    ProductPrice = product?.Price,
                    // Check if we can parse user ID as int
                    UserIdAsIntValid = user != null && int.TryParse(user.Id.ToString(), out _)
                });
            }
            catch (Exception ex)
            {
                // Return full exception details for debugging
                return StatusCode(500, new
                {
                    Error = ex.Message,
                    StackTrace = ex.StackTrace,
                    InnerException = ex.InnerException?.Message
                });
            }
        }
    }
}
