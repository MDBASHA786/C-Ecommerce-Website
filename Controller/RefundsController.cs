// RefundsController.cs
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
    [Authorize]
    public class RefundsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IRefundService _refundService;

        public RefundsController(
            ApplicationDbContext context,
            IRefundService refundService)
        {
            _context = context;
            _refundService = refundService;
        }

        // POST: api/Refunds
        [HttpPost]
        public async Task<IActionResult> CreateRefundRequest([FromBody] RefundRequestDto requestDto)
        {
            return await _refundService.CreateRefundRequest(requestDto, User);
        }

        // GET: api/Refunds
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RefundRequest>>> GetRefundRequests()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            return await _context.RefundRequests
                .Where(r => r.UserId.ToString() == userId)
                .Include(r => r.Order)
                .ToListAsync();
        }

        // GET: api/Refunds/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RefundRequest>> GetRefundRequest(int id)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var refundRequest = await _context.RefundRequests
                .Include(r => r.Order)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId.ToString() == userId);

            if (refundRequest == null)
            {
                return NotFound();
            }

            return refundRequest;
        }
    }
}