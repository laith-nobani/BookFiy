using BookFiy.Domain.Entites;
using BookFiy.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using BookFiy.Domain.IRepositories;

namespace BookFiy.Infrastructure.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly AppDbContext _context;
        public ServiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Service?> GetByIdAsync(Guid id, Guid tenantId)
        {
            return await _context.Set<Service>()
                .Include(e=> e.Employee)
                .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);
        }

        public async Task<List<Service>> GetAllAsync(Guid tenantId,Guid EmployeeId)
        {
            return await _context.Set<Service>().
                AsNoTracking()
                .Where(s => s.TenantId == tenantId && s.EmployeeId== EmployeeId).
                ToListAsync();
        }

        public async Task CreateAsync(Service service)
        {
            await _context.Set<Service>().AddAsync(service);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Service service)
        {
            _context.Set<Service>().Update(service);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id, Guid tenantId)
        {
            var s = await _context.Set<Service>()
                .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
            if (s != null)
            {
                _context.Set<Service>().Remove(s);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Service>> GetServicesAsync(Guid tenantId)
        {
            var services = await _context.Set<Service>()
                .Where(s => s.TenantId == tenantId)
                .AsNoTracking()
                .ToListAsync();
            return services;

        }
    }
}
