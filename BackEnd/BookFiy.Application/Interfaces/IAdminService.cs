using BookFiy.Application.Dtos.Admin;
using Microsoft.AspNetCore.Identity.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Application.Interfaces
{
    public interface IAdminService
    {
        public Task<List<AdminDto>> GetAllAdminsAsync();
        public Task<AdminDto> GetAdminByIdAsync(Guid adminId);
        public Task<AdminDto> CreateAdminAsync(CreateAdminDto admin);
        public Task UpdateAdminAsync(Guid adminId, UpdateAdminDto admin);
    }
}
