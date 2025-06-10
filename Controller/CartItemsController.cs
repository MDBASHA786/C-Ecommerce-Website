using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuitQ1_Hx.Data;
using QuitQ1_Hx.DTO;
using QuitQ1_Hx.Models;
using QuitQ1_Hx.Services;
using System.Security.Claims;


namespace QuitQ1_Hx.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get the current user's ID from claims
        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                   User.FindFirstValue("sub") ??
                   throw new InvalidOperationException("User ID not found in claims");
        }

        // Get All Cart Items (With DTO)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartItemDto>>> GetCartItems()
        {
            var userId = GetUserId();
            // Get only cart items belonging to the current user's cart
            var cartItems = await _context.CartItems
                .Include(ci => ci.Cart)
                .Where(ci => ci.Cart.UserId.ToString() == userId)
                .ToListAsync();

            var cartItemDtos = cartItems.Select(ci => new CartItemDto
            {
                Id = ci.Id,
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                Price = ci.Price
            }).ToList();

            return Ok(cartItemDtos);
        }

        // Get a Single Cart Item (With DTO)
        [HttpGet("{id}")]
        public async Task<ActionResult<CartItemDto>> GetCartItem(int id)
        {
            var userId = GetUserId();

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == id && ci.Cart.UserId.ToString() == userId);

            if (cartItem == null)
                return NotFound();

            var cartItemDto = new CartItemDto
            {
                Id = cartItem.Id,
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                Price = cartItem.Price
            };

            return Ok(cartItemDto);
        }

        // Create a Cart Item
        [HttpPost]
        public async Task<ActionResult<CartItemDto>> CreateCartItem([FromBody] CreateCartItemDto createCartItemDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();

            // Find the user's cart
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId.ToString() == userId);

            if (cart == null)
                return NotFound("Cart not found");

            // Create a new cart item
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = createCartItemDto.ProductId,
                Quantity = createCartItemDto.Quantity,
                Price = createCartItemDto.Price
            };

            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            var cartItemDto = new CartItemDto
            {
                Id = cartItem.Id,
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                Price = cartItem.Price
            };

            return CreatedAtAction(nameof(GetCartItem), new { id = cartItem.Id }, cartItemDto);
        }

        // Update a Cart Item
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCartItem(int id, [FromBody] UpdateCartItemDto updateCartItemDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == id && ci.Cart.UserId.ToString() == userId);

            if (cartItem == null)
                return NotFound();

            cartItem.Quantity = updateCartItemDto.Quantity;

            if (updateCartItemDto.Price.HasValue)
                cartItem.Price = updateCartItemDto.Price.Value;

            _context.Entry(cartItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CartItemExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // Delete a Cart Item
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCartItem(int id)
        {
            var userId = GetUserId();

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == id && ci.Cart.UserId.ToString() == userId);

            if (cartItem == null)
                return NotFound();

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CartItemExists(int id)
        {
            return _context.CartItems.Any(e => e.Id == id);
        }
    }

}