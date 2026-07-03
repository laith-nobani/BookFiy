using BookFiy.Application.Dtos.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookFiy.Application.Interfaces
{
    public interface IServiceService
    {
        Task<ServiceDto> CreateServiceAsync(CreateServiceDto dto, Guid tenantId);
        Task<List<ServiceDto>> GetAllAsync(Guid tenantId,Guid Employeeid);
        Task<ServiceDto> GetByIdAsync(Guid id, Guid tenantId);
        Task<List<ServiceDto>> GetAllAsync(Guid tenantId);
        Task UpdateServiceAsync(Guid id, UpdateServiceDto dto, Guid tenantId);
        Task DeleteServiceAsync(Guid id, Guid tenantId);
    }
}
