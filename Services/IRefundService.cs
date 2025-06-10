using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QuitQ1_Hx.Data;
using QuitQ1_Hx.DTO;
using QuitQ1_Hx.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace QuitQ1_Hx.Services
{
    public interface IRefundService
    {
        Task<ActionResult> CreateRefundRequest(RefundRequestDto requestDto, ClaimsPrincipal user);
    }

    public class RefundService : IRefundService
    {
        private readonly ApplicationDbContext _context;

        public RefundService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult> CreateRefundRequest(RefundRequestDto requestDto, ClaimsPrincipal user)
        {
            try
            {
                // Validate user identity
                string? userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return new UnauthorizedResult();
                }

                // Check if order exists and belongs to user
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == requestDto.OrderId && o.UserId.ToString() == userId);

                if (order == null)
                {
                    return new NotFoundObjectResult(new { message = "Order not found or does not belong to this user" });
                }

                // Check if product exists in order
                var orderItem = await _context.OrderItems
                    .FirstOrDefaultAsync(oi => oi.OrderId == requestDto.OrderId && oi.ProductId == requestDto.ProductId);

                if (orderItem == null)
                {
                    return new NotFoundObjectResult(new { message = "Product not found in the specified order" });
                }

                // Check if a refund request already exists for this order item
                var existingRefundRequest = await _context.RefundRequests
                    .FirstOrDefaultAsync(r => r.OrderId == requestDto.OrderId && r.ProductId == requestDto.ProductId);

                if (existingRefundRequest != null)
                {
                    return new BadRequestObjectResult(new { message = "A refund request already exists for this product" });
                }

                // Create the refund request
                var refundRequest = new RefundRequest
                {
                    OrderId = requestDto.OrderId,
                    ProductId = requestDto.ProductId,
                    UserId = int.Parse(userId),
                    Reason = requestDto.Reason,
                    Status = RefundStatus.Pending,
                    RequestedAt = DateTime.UtcNow  // Using RequestedAt instead of CreatedAt
                };

                _context.RefundRequests.Add(refundRequest);
                await _context.SaveChangesAsync();

                return new OkObjectResult(new { message = "Refund request submitted successfully", refundId = refundRequest.Id });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { message = $"An error occurred: {ex.Message}" })
                {
                    StatusCode = 500
                };
            }
        }
    }
}