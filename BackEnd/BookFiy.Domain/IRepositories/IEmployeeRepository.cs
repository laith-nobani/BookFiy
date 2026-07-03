using BookFiy.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Domain.IRepositories
{
    public interface IEmployeeRepository
    {
         public Task<Employee> GetEmployeeByIdAsync(Guid employeeId,Guid tenantId);
         public Task<List<Employee>> GetAllEmployeesAsync(Guid tenantId);
         public Task CreateEmployeeAsync(Employee employee);
         public Task UpdateEmployeeAsync(Employee employee);
         public Task DeleteEmployeeAsync(Guid employeeId, Guid tenantId);
         public Task<Employee> GetEmployeeByUserIdAsync(Guid userId, Guid tenantId);


    }
}
