using BookFiy.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookFiy.Domain.IRepositories
{
    public interface IServiceRepository
    {
        Task<Service> GetByIdAsync(Guid id, Guid tenantId);
        Task<List<Service>> GetAllAsync(Guid tenantId, Guid Employeeid);

        Task<List<Service>> GetServicesAsync(Guid tenantId);
        Task CreateAsync(Service service);
        Task UpdateAsync(Service service);
        Task DeleteAsync(Guid id, Guid tenantId);
    }
}
