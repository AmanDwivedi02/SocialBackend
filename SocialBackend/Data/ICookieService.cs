using Microsoft.AspNetCore.Http;

namespace SocialBackend.Data
{
    public interface ICookieService
    {
        string getCookieValue(HttpContext context);
    }
}
