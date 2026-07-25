using BookFiy.Api.Extensions;
using BookFiy.Application.Dtos.Employee;
using BookFiy.Application.Interfaces;
using BookFiy.Application.Services;
using BookFiy.Domain.Constants;
using BookFiy.Domain.Entites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookFiy.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterEmployee(CraateEmployeeDto request)
        {


            var tenantClaim = User.FindFirst("tenant_id")?.Value;
            if (tenantClaim == null) 
                return Forbid();

            var tenantId = Guid.Parse(tenantClaim);
            var createdBy = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            return (await _employeeService.RegisterEmployeeAsync(request, tenantId, createdBy))
               .ToActionResult();

        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tenantClaim = User.FindFirst("tenant_id")?.Value;
            if (tenantClaim == null) 
                return Forbid();

            var tenantId = Guid.Parse(tenantClaim);
            var list = await _employeeService.GetAllEmployeesAsync(tenantId);
            return Ok(list.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var tenantClaim = User.FindFirst("tenant_id")?.Value;
            if (tenantClaim == null) return Forbid();
            var tenantId = Guid.Parse(tenantClaim);
    
            var res = await _employeeService.GetEmployeeByIdAsync(id, tenantId);   
            if (!res.IsSuccess)   
                return NotFound(res.Message);
                
            return Ok(res.Data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateEmployeeDto request)
        {

            var tenantClaim = User.FindFirst("tenant_id")?.Value;
            if (tenantClaim == null) 
                return Forbid();

            var tenantId = Guid.Parse(tenantClaim);
            var updatedBy = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            
            return (await _employeeService.UpdateEmployeeAsync(id, request, tenantId))
                      .ToActionResult();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var tenantClaim = User.FindFirst("tenant_id")?.Value;

            if (tenantClaim == null) 
                return Forbid();

            var tenantId = Guid.Parse(tenantClaim);

            return (await _employeeService.DeleteEmployeeAsync(id,tenantId))
                            .ToActionResult();


        }
    }
}
