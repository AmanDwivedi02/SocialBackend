using Microsoft.AspNetCore.Http;
using System;

namespace SocialBackend.Data
{
    public class CookieService : ICookieService
    {
        public string getCookieValue(HttpContext context)
        {
            return string.IsNullOrEmpty(context.Request.Cookies["cookie"]) || string.IsNullOrWhiteSpace(context.Request.Cookies["cookie"]) ? "" : context.Request.Cookies["cookie"];
        }

        public void setCookie(HttpContext context, string authToken)
        {
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddHours(1);
            context.Response.Cookies.Append("cookie", authToken, options);
        }
    }
}
