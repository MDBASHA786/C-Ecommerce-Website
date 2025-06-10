using QuitQ1_Hx.Models;
using System.Threading.Tasks;

namespace QuitQ1_Hx.Repositories
{
    public interface IProductRepository
    {
        Task<Product> GetByIdAsync(int productId);
    }
}