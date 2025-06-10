// OrdersController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuitQ1_Hx.Data;
using QuitQ1_Hx.Models;
using QuitQ1_Hx.DTO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using QuitQ1_Hx.Services;

namespace QuitQ1_Hx.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IRefundService _refundService;

        public OrdersController(
            ApplicationDbContext context,
            IRefundService refundService)
        {
            _context = context;
            _refundService = refundService;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders.ToListAsync();
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.ShippingAddress)
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // PUT: api/Orders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            if (id != order.Id)
            {
                return BadRequest();
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Cart/add
        [HttpPost("cart/add")]
        [Authorize]
        public async Task<IActionResult> AddToCart(AddToCartDto model)
        {
            try
            {
                string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    return Unauthorized();
                }

                // Check if product exists
                var product = await _context.Products.FindAsync(model.ProductId);
                if (product == null)
                {
                    return NotFound("Product not found");
                }

                // For Buy Now functionality, first remove any existing Buy Now items
                if (model.IsBuyNow)
                {
                    var existingBuyNowItems = await _context.CartItems
                        .Where(c => c.Cart != null && c.Cart.UserId.ToString() == userId && c.IsBuyNow)
                        .ToListAsync();

                    if (existingBuyNowItems.Any())
                    {
                        _context.CartItems.RemoveRange(existingBuyNowItems);
                        await _context.SaveChangesAsync();
                    }
                }

                // Create new cart item
                var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId.ToString() == userId);
                if (cart == null)
                {
                    return NotFound("Cart not found");
                }

                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = model.ProductId,
                    Quantity = model.Quantity,
                    DateAdded = DateTime.UtcNow,
                    IsBuyNow = model.IsBuyNow
                };

                _context.CartItems.Add(cartItem);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Product added to cart successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // GET: api/Orders/buyNow
        [HttpGet("buyNow")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CartItemDto>>> GetBuyNowItems()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var cartItems = await _context.CartItems
                .Where(c => c.Cart != null && c.Cart.UserId.ToString() == userId && c.IsBuyNow)
                .Include(c => c.Product)
                .ToListAsync();

            if (cartItems == null || !cartItems.Any())
            {
                return NotFound();
            }

            var cartItemDtos = cartItems.Select(c => new CartItemDto
            {
                Id = c.Id,
                ProductId = c.ProductId,
                ProductName = c.Product?.Name ?? string.Empty,
                Price = c.Product?.Price ?? 0,
                Quantity = c.Quantity,
                ImageUrl = c.Product?.MainImageUrl ?? string.Empty,
                Total = (c.Product?.Price ?? 0) * c.Quantity
            }).ToList();

            return Ok(cartItemDtos);
        }

        // POST: api/Orders
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrder", new { id = order.Id }, order);
        }

        [HttpPost("{orderId}/request-refund")]
        [Authorize]
        public async Task<IActionResult> RequestRefund(int orderId, [FromBody] OrderRefundDto refundDto)
        {
            if (refundDto == null)
                return BadRequest(new { message = "Request data is required" });

            // Create the refund request
            var requestDto = new RefundRequestDto
            {
                OrderId = orderId,
                ProductId = refundDto.ProductId,
                Reason = refundDto.Reason
            };

            // Forward to the refund service
            return await _refundService.CreateRefundRequest(requestDto, User);
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}

