using BookFiy.Application.Dtos.Tenant;
using BookFiy.Application.Interfaces;
using BookFiy.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookFiy.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public class TenantController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        public TenantController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetTenantBySlug(string slug)
        {
            var tenant = await _tenantService.GetTenantBySlugAsync(slug);
            if (tenant == null)
                return NotFound();
            return Ok(tenant);
        }
        [HttpGet("id/{tenantId}")]
        public async Task<IActionResult> GetTenantById(Guid tenantId)
        {
            var tenant = await _tenantService.GetTenantByIdAsync(tenantId);
            if (tenant == null)
                return NotFound();
            return Ok(tenant);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllTenants()
        {
            var tenants = await _tenantService.GetAllTenantsAsync();
            return Ok(tenants);
        }
        [HttpPost]
        public async Task<IActionResult> CreateTenant(CreateTenantDto tenant)
        {
            await _tenantService.CreateTenantAsync(tenant);
            return Ok();
        }
        [HttpPut("{tenantId}")]
        public async Task<IActionResult> UpdateTenant(Guid tenantId, UpdateTenantDto tenant)
        {
            await _tenantService.UpdateTenantAsync(tenantId, tenant);
            return Ok();
        }
        [HttpDelete("{tenantId}")]
        public async Task<IActionResult> DeleteTenant(Guid tenantId)
        {
            await _tenantService.DeleteTenantAsync(tenantId);
            return Ok();
        }
    }
   

    
}
