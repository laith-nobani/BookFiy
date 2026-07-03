using BookFiy.Domain.Entites;
using BookFiy.Domain.IRepositories;
using BookFiy.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _context;
        public EmployeeRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Employee?> GetEmployeeByIdAsync(Guid employeeId,Guid tenantId)
        {
            return await _context.Set<Employee>()
                .FirstOrDefaultAsync(e => e.Id == employeeId && e.TenantId== tenantId);
        }
        public async Task<List<Employee>> GetAllEmployeesAsync(Guid tenantId)
        {
            return await _context.Set<Employee>()
                .Where(e=> e.TenantId==tenantId)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task CreateEmployeeAsync(Employee employee)
        {
            _context.Add(employee);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateEmployeeAsync(Employee employee)
        {
            _context.Update(employee);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteEmployeeAsync(Guid employeeId,Guid tenantId)
        {
            var employee = _context.Set<Employee>()
                .FirstOrDefault(e => e.Id == employeeId && e.TenantId==tenantId);

            if (employee != null)
            {
                _context.Remove(employee);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Employee> GetEmployeeByUserIdAsync(Guid userId, Guid tenantId)
        {
            return await _context.Set<Employee>()
                .FirstOrDefaultAsync(e => e.UserId == userId && e.TenantId == tenantId);

        }
    }
}
