using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuitQ1_Hx.Data;
using QuitQ1_Hx.Models;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;

namespace QuitQ1_Hx.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductsController> _logger;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(
            ApplicationDbContext context,
            ILogger<ProductsController> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                {
                    return BadRequest(new { message = "No file was uploaded" });
                }

                // Validate file type
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                string fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();

                if (!Array.Exists(allowedExtensions, ext => ext == fileExtension))
                {
                    return BadRequest(new { message = "Invalid file type. Only JPG, PNG, and GIF files are allowed." });
                }

                // Create a unique filename to prevent overwriting
                string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

                // Determine the upload path - make sure this directory exists
                string uploadsFolder = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "products");

                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                // Return the relative URL to the saved image
                string relativeUrl = $"/uploads/products/{uniqueFileName}";

                _logger.LogInformation($"Image uploaded successfully: {relativeUrl}");

                return Ok(new { imageUrl = relativeUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading product image");
                _logger.LogError($"Error in UploadImage: {ex.Message}, Stack: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { message = $"Error uploading image: {ex.Message}" });
            }
        }
        // Get all products
        [HttpGet]
        public IActionResult GetProducts()
        {
            try
            {
                _logger.LogInformation("Retrieving all products");

                var products = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Seller)
                    .ToList();

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return StatusCode(500, new { message = $"Error retrieving products: {ex.Message}" });
            }
        }

        // Get product by ID
        [HttpGet("{id}")]
        public IActionResult GetProduct(int id)
        {
            _logger.LogInformation($"Retrieving product with ID: {id}");

            var product = _context.Products
                .Include(p => p.Category)
                 .Include(p => p.Seller)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                _logger.LogWarning($"Product with ID {id} not found");
                return NotFound(new { message = "Product not found" });
            }

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            _logger.LogInformation($"Received product: {JsonSerializer.Serialize(product)}");

            if (product == null)
                return BadRequest(new { message = "The Product object is required." });

            if (string.IsNullOrWhiteSpace(product.Name))
                return BadRequest(new { message = "The Name field is required." });

            if (product.Price <= 0)
                return BadRequest(new { message = "The Price must be greater than zero." });

            // Validate Category exists
            var category = await _context.Categories.FindAsync(product.CategoryId);
            if (category == null)
                return BadRequest(new { message = "Please select a valid category." });

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate Seller exists
            var seller = await _context.Sellers.FindAsync(product.SellerId);
            if (seller == null)
                return BadRequest(new { message = "The specified seller does not exist." });
            // Set the seller name
            product.SellerName = seller.Name;

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == product.CategoryId);
            if (!categoryExists)
            {
                // Instead of returning this error, make sure the category exists in the database
                return BadRequest(new { message = "Please select a valid category." });
            }

            try
            {
                product.ImageUrls ??= new List<string>(); // Initialize if null
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Product created successfully with ID: {product.Id}");
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (DbUpdateException dbEx)
            {
                string innerMsg = dbEx.InnerException?.Message ?? dbEx.Message;
                _logger.LogError(dbEx, $"Database error while creating product: {innerMsg}");
                return StatusCode(500, new { message = $"Database error: {innerMsg}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new { message = $"Error creating product: {ex.Message}" });
            }
        }

        // Update product
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product updatedProduct)
        {
            _logger.LogInformation($"Updating product with ID: {id}");

            if (updatedProduct == null)
                return BadRequest(new { message = "Request body is required" });

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                _logger.LogWarning($"Product with ID {id} not found for update");
                return NotFound(new { message = "Product not found" });
            }

            try
            {
                // Check if seller exists if the seller ID is being updated
                if (updatedProduct.SellerId != existingProduct.SellerId)
                {
                    var seller = await _context.Sellers.FindAsync(updatedProduct.SellerId);
                    if (seller == null)
                        return BadRequest(new { message = "The specified seller does not exist." });

                    existingProduct.SellerId = updatedProduct.SellerId;
                    existingProduct.SellerName = seller.Name;
                }

                existingProduct.Name = updatedProduct.Name;
                existingProduct.Description = updatedProduct.Description;
                existingProduct.Price = updatedProduct.Price;
                existingProduct.StockQuantity = updatedProduct.StockQuantity;
                existingProduct.CategoryId = updatedProduct.CategoryId;
                existingProduct.ImageUrls = updatedProduct.ImageUrls ?? new List<string>();
                existingProduct.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Product with ID {id} updated successfully");
                return Ok(existingProduct);
            }
            catch (DbUpdateException dbEx)
            {
                string innerMsg = dbEx.InnerException?.Message ?? dbEx.Message;
                _logger.LogError(dbEx, $"Database error while updating product: {innerMsg}");
                return StatusCode(500, new { message = $"Database error: {innerMsg}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product with ID {id}: {ex.Message}");
                return StatusCode(500, new { message = $"Error updating product: {ex.Message}" });
            }
        }

        // Delete product
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            _logger.LogInformation($"Deleting product with ID: {id}");

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning($"Product with ID {id} not found for deletion");
                return NotFound(new { message = "Product not found" });
            }

            try
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Product with ID {id} deleted successfully");
                return Ok(new { message = "Product deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product with ID {id}: {ex.Message}");
                return StatusCode(500, new { message = $"Error deleting product: {ex.Message}" });
            }
        }
    }
}