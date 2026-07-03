using BookFiy.Domain.Entites;
using BookFiy.Domain.Entities;
using BookFiy.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Infrastructure.Data.SeedData
{
    public class AdminSeeder
    {
        public static async Task seedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var db = serviceProvider.GetRequiredService<AppDbContext>();

            var tenantSlug = "default";
            var tenant = await db.Set<Tenant>().FirstOrDefaultAsync(t => t.Slug == tenantSlug);
            if (tenant == null)
            {
                tenant = new Tenant
                {
                    Id = new Guid("75C546DC-74C5-45EB-9563-75F9A0485C7B"),
                    Name = "Default Tenant",
                    Slug = tenantSlug
                };
                db.Add(tenant);
                await db.SaveChangesAsync();
            }

            var adminEmail = "laithalnobane323@gmail.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var newAdminUser = ApplicationUser.Create("admin", adminEmail, "laith", "nobani", "0782450024", tenant.Id);
                var result = await userManager.CreateAsync(newAdminUser, "Admin@Z1234");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdminUser, "Super Admin");
                }
            }

        }
        }
}
