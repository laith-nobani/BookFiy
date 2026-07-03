using BookFiy.Application.Dtos.Tenant;
using BookFiy.Application.Interfaces;
using BookFiy.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Application.Services
{
    public class TenantService : ITenantService
    {
        private readonly ITenantRepository _tenantRepository;
        public TenantService(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }
        public async Task CreateTenantAsync(CreateTenantDto tenant)
        {
            var existingTenant = await _tenantRepository.GetTenantBySlugAsync(tenant.Slug);
            if (existingTenant != null)
            {
                throw new Exception("A tenant with the same slug already exists.");
            }

            var newTenant = new Domain.Entites.Tenant
            {
                Name = tenant.Name,
                Slug = tenant.Slug
            };
            await _tenantRepository.CreateTenantAsync(newTenant);
        }

        public async Task DeleteTenantAsync(Guid tenantId)
        {
            var tenant = _tenantRepository.GetTenantByIdAsync(tenantId);
            if (tenant == null)
            {
                throw new Exception("Tenant not found.");
            }
            await _tenantRepository.DeleteTenantAsync(tenantId);            
        }

        public async Task<List<TenantDto>> GetAllTenantsAsync()
        {
            var tenants = await _tenantRepository.GetAllTenantsAsync();
            var tenantDtos = tenants.Select(t => new TenantDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                CreatedAt = t.CreatedAt
            }).ToList();
            
            return tenantDtos;
        }

        public async Task<TenantDto> GetTenantByIdAsync(Guid tenantId)
        {
            
            var tenant =await _tenantRepository.GetTenantByIdAsync(tenantId);
            if (tenant == null)
            {
                throw new Exception("Tenant not found.");
            }
            var tenantDto = new TenantDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Slug = tenant.Slug,
                CreatedAt = tenant.CreatedAt
            };
            return tenantDto;
        }

        public async Task<TenantDto> GetTenantBySlugAsync(string slug)
        {
            var tenant =await _tenantRepository.GetTenantBySlugAsync(slug);

            if (tenant == null)
            {
                throw new Exception("Tenant not found.");
            }
            var tenantDto = new TenantDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Slug = tenant.Slug,
                CreatedAt = tenant.CreatedAt
            };
            return tenantDto;
        }

        public async Task UpdateTenantAsync(Guid tenantId, UpdateTenantDto tenant)
        {
            var existingTenant =await _tenantRepository.GetTenantByIdAsync(tenantId);
            if (existingTenant == null)
            {
                throw new Exception("Tenant not found.");
            }
            if (existingTenant.Slug != tenant.Slug)
            {
                var tenantWithSameSlug = _tenantRepository.GetTenantBySlugAsync(tenant.Slug);
                if (tenantWithSameSlug.Result != null)
                {
                    throw new Exception("A tenant with the same slug already exists.");
                }
            }
            if (!string.IsNullOrEmpty(tenant.Name))
            {
                existingTenant.Name = tenant.Name;
            }
            if (!string.IsNullOrEmpty(tenant.Slug))
            {
                existingTenant.Slug = tenant.Slug;
            }
            await _tenantRepository.UpdateTenantAsync(existingTenant);

        }
    }
}
