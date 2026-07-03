using BookFiy.Domain.Constants;
using BookFiy.Domain.Entites;
using BookFiy.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Infrastructure.Data.SeedData
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {

      
            await RoleSeeder.seedAsync(serviceProvider);
            await AdminSeeder.seedAsync(serviceProvider);
        }
    }
}
