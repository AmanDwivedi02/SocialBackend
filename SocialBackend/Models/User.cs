using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialBackend.Models
{
    public class User
    {
        public int id { get; set; }
        public virtual string username { get; set; }
        public virtual string emailAddress { get; set; }
        public virtual string password { get; set; }
        public virtual string authToken { get; set; }
        public virtual Boolean online { get; set; }

    }
}
