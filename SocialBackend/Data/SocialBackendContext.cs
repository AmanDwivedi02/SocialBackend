using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SocialBackend.Model;

namespace SocialBackend.Models
{
    public class SocialBackendContext : DbContext
    {
        public SocialBackendContext (DbContextOptions<SocialBackendContext> options)
            : base(options)
        {
        }

        public DbSet<SocialBackend.Model.User> User { get; set; }
    }
}
