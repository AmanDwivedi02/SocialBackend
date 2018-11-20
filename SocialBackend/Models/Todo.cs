using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialBackend.Models
{
    public class Todo
    {
        public int id { get; set; }
        public string task { get; set; }
        public DateTime dueDate { get; set; }
        public bool complete { get; set; }
        public virtual User user { get; set; }
    }
}
