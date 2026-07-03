using BookFiy.Domain.Entites;
using BookFiy.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Infrastructure.Repositories
{
    public class TenantRepository : Domain.IRepositories.ITenantRepository
    {
        private readonly AppDbContext _context;
        public TenantRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Tenant?> GetTenantBySlugAsync(string slug)
        {
            return await _context.Set<Tenant>().FirstOrDefaultAsync(t => t.Slug == slug);
        }
        public async Task<Tenant?> GetTenantByIdAsync(Guid tenantId)
        {
            return await _context.Set<Tenant>().FirstOrDefaultAsync(t => t.Id == tenantId);

        }

        public async Task<List<Tenant>> GetAllTenantsAsync()
        {
            return await _context.Set<Tenant>()
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task CreateTenantAsync(Tenant tenant)
        {
            _context.Add(tenant);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateTenantAsync(Tenant tenant)
        {
            _context.Update(tenant);
            await _context.SaveChangesAsync();
        }


        public async Task DeleteTenantAsync(Guid tenantId)
        {
            var tenant = _context.Set<Tenant>().FirstOrDefault(t => t.Id == tenantId);
            if (tenant != null)
            {
                _context.Remove(tenant);
                await _context.SaveChangesAsync();
            }
            
        }
    }
}
