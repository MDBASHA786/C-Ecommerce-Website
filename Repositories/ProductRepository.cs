using Microsoft.EntityFrameworkCore;
using QuitQ1_Hx.Data;
using QuitQ1_Hx.Models;
using System.Threading.Tasks;

namespace QuitQ1_Hx.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ProductRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Product> GetByIdAsync(int productId)
        {
            return await _dbContext.Products.FindAsync(productId);
        }
    }
}