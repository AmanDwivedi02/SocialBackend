using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialBackend.Data
{
    public interface ICookieService
    {
        string getCookieValue(HttpContext context);
        void setCookie(HttpContext context, string authToken);
    }
}
