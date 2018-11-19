using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialBackend.Model
{
    public class User
    {
        public int id { get; set; }
        public string username { get; set; }
        public string emailAddress { get; set; }
        public string password { get; set; }
        public Boolean online { get; set; }

    }
}
