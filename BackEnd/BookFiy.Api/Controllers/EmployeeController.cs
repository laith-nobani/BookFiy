using BookFiy.Application.Dtos.Employee;
using BookFiy.Application.Interfaces;
using BookFiy.Domain.Constants;
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
            try
            {

                var tenantClaim = User.FindFirst("tenant_id")?.Value;
                if (tenantClaim == null) return Forbid();
                var tenantId = Guid.Parse(tenantClaim);

                var createdBy = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var employee = await _employeeService.RegisterEmployeeAsync(request, tenantId, createdBy);
                return Ok(employee);
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

            var list = await _employeeService.GetAllEmployeesAsync(tenantId);
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var tenantClaim = User.FindFirst("tenant_id")?.Value;
                if (tenantClaim == null) return Forbid();
                var tenantId = Guid.Parse(tenantClaim);

                var emp = await _employeeService.GetEmployeeByIdAsync(id, tenantId);
                if (emp == null) return NotFound();
                return Ok(emp);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateEmployeeDto request)
        {
            try
            {
                var tenantClaim = User.FindFirst("tenant_id")?.Value;
                if (tenantClaim == null) return Forbid();
                var tenantId = Guid.Parse(tenantClaim);
                var updatedBy = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                _employeeService.UpdateEmployeeAsync(id, request, tenantId);

                return Ok();
            }
            catch (Exception ex)
            {
                return NotFound(ex);
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
                await _employeeService.DeleteEmployeeAsync(id, tenantId);
                return Ok();
            }
            catch
            {
                return NotFound();
            }
        }
    }
    }
