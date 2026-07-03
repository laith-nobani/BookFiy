using BookFiy.Application.Dtos.Service;
using BookFiy.Application.Interfaces;
using BookFiy.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookFiy.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceService _serviceService;
        public ServicesController(IServiceService serviceService)
        {
            _serviceService = serviceService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateServiceDto dto)
        {
            try
            {
                var tenantClaim = User.FindFirst("tenant_id")?.Value;
                if (tenantClaim == null) return Forbid();
                var tenantId = Guid.Parse(tenantClaim);

                var created = await _serviceService.CreateServiceAsync(dto, tenantId);
                return Ok(created);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);

            }

        }

        [HttpGet]
        
        public async Task<IActionResult> GetAll()
        {
            var tenantClaim = User.FindFirst("tenant_id")?.Value;
            if (tenantClaim == null) return Forbid();
            var tenantId = Guid.Parse(tenantClaim);
            var list = await _serviceService.GetAllAsync(tenantId);
            return Ok(list);
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetAll(Guid employeeId)
        {
            var tenantClaim = User.FindFirst("tenant_id")?.Value;
            if (tenantClaim == null) return Forbid();

            var tenantId = Guid.Parse(tenantClaim);

            var list = await _serviceService.GetAllAsync(tenantId, employeeId);
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var tenantClaim = User.FindFirst("tenant_id")?.Value;
                if (tenantClaim == null) return Forbid();
                var tenantId = Guid.Parse(tenantClaim);
                var s = await _serviceService.GetByIdAsync(id, tenantId);
                if (s == null) return NotFound();
                return Ok(s);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateServiceDto dto)
        {
            try
            {
                var tenantClaim = User.FindFirst("tenant_id")?.Value;
                if (tenantClaim == null) return Forbid();
                var tenantId = Guid.Parse(tenantClaim);
                await _serviceService.UpdateServiceAsync(id, dto, tenantId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var tenantClaim = User.FindFirst("tenant_id")?.Value;
                if (tenantClaim == null) return Forbid();
                var tenantId = Guid.Parse(tenantClaim);
                await _serviceService.DeleteServiceAsync(id, tenantId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
