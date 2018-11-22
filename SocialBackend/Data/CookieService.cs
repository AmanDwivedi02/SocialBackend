using Microsoft.AspNetCore.Http;
using System;

namespace SocialBackend.Data
{
    public class CookieService : ICookieService
    {
        public string getCookieValue(HttpContext context)
        {
            return string.IsNullOrEmpty(context.Request.Headers["token"].ToString()) || string.IsNullOrWhiteSpace(context.Request.Headers["token"].ToString()) ? "" : context.Request.Headers["token"].ToString();
        }
    }
}
