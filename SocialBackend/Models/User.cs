using System;

namespace SocialBackend.Models
{
    public class User
    {
        public int id { get; set; }
        public string username { get; set; }
        public string emailAddress { get; set; }
        public string password { get; set; }
        public string authToken { get; set; }
        public Boolean online { get; set; }

    }
}
