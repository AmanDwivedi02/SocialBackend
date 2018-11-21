using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialBackend.Data
{
    public class CookieService : ICookieService
    {
        public string getCookieValue(HttpContext context)
        {
            return context.Request.Cookies["cookie"] == null ? "" : context.Request.Cookies["cookie"];
        }

        public void setCookie(HttpContext context, string authToken)
        {
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddHours(1);
            context.Response.Cookies.Append("cookie", authToken, options);
        }
    }
}
