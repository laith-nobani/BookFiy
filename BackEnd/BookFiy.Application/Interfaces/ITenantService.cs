using BookFiy.Application.Dtos.Tenant;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Application.Interfaces
{
    public interface ITenantService
    {
        public Task<TenantDto> GetTenantBySlugAsync(string slug);
        public Task<TenantDto> GetTenantByIdAsync(Guid tenantId);
        public Task<List<TenantDto>> GetAllTenantsAsync();
        public Task CreateTenantAsync(CreateTenantDto tenant);
        public Task UpdateTenantAsync(Guid tenantId, UpdateTenantDto tenant);
        public Task DeleteTenantAsync(Guid tenantId);
    }
}
