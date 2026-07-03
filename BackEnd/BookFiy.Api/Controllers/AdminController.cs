using BookFiy.Application.Dtos.Admin;
using BookFiy.Application.Interfaces;
using BookFiy.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookFiy.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAdmins()
        {
            var admins = await _adminService.GetAllAdminsAsync();
            return Ok(admins);
        }
        [HttpPost]
        public async Task<IActionResult> CreateAdmin(CreateAdminDto admin)
        {
            try
            {
                var createdAdmin = await _adminService.CreateAdminAsync(admin);
                return Ok(createdAdmin);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut]
        [Route("{adminId}")]
        public async Task<IActionResult> UpdateAdmin(Guid adminId, UpdateAdminDto admin)
        {
            try
            {
                  await _adminService.UpdateAdminAsync(adminId, admin);
                  return Ok();
            }
            catch (Exception ex)
            {
                  return BadRequest(ex.Message);
            }
        }
    }
}
