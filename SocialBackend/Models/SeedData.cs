﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialBackend.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new SocialBackendContext(
                serviceProvider.GetRequiredService<DbContextOptions<SocialBackendContext>>()))
            {
                if (context.User.Count() == 0)
                {
                    context.User.AddRange(
                        new User
                        {
                            username = "test",
                            password = "testing",
                            emailAddress = "test@test.com",
                            authToken = Guid.NewGuid().ToString(),
                            online = false
                        });
                }
                context.SaveChanges();
            }
        }
    }
}