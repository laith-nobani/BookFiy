using BookFiy.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Domain.IRepositories
{
    public interface ITenantRepository
    {
        public Task<Tenant> GetTenantBySlugAsync(string slug);
        public Task<Tenant> GetTenantByIdAsync(Guid tenantId);
        public Task<List<Tenant>> GetAllTenantsAsync();
        public Task CreateTenantAsync(Tenant tenant);
        public Task UpdateTenantAsync(Tenant tenant);
        public Task DeleteTenantAsync(Guid tenantId);
    }
}
