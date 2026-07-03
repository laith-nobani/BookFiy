using BookFiy.Application.Dtos.Employee;
using BookFiy.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookFiy.Application.Interfaces
{
    public interface IEmployeeService
    {
        Task<EmployeeDto> RegisterEmployeeAsync(CraateEmployeeDto request, Guid tenantId, Guid createdBy);
        Task<List<EmployeeDto>> GetAllEmployeesAsync(Guid tenantId);
        Task<EmployeeDto> GetEmployeeByIdAsync(Guid employeeId, Guid tenantId);
        Task UpdateEmployeeAsync(Guid employeeId, UpdateEmployeeDto request, Guid tenantId);
        Task DeleteEmployeeAsync(Guid employeeId, Guid tenantId);
    }
}
