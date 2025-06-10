
namespace QuitQ1_Hx.Services
{
    public interface IAuthService
    {
        string GenerateJwtToken(string email, string role);
    }
}
