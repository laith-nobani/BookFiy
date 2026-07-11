using BookFiy.Application.Comman;
using BookFiy.Application.Dtos.Employee;
using BookFiy.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookFiy.Application.Interfaces
{
    public interface IEmployeeService
    {
        Task<Result<EmployeeDto>> RegisterEmployeeAsync(CraateEmployeeDto request, Guid tenantId, Guid createdBy);
        Task<Result<List<EmployeeDto>>> GetAllEmployeesAsync(Guid tenantId);
        Task<Result<EmployeeDto>> GetEmployeeByIdAsync(Guid employeeId, Guid tenantId);
        Task<Result<bool>> UpdateEmployeeAsync(Guid employeeId, UpdateEmployeeDto request, Guid tenantId);
        Task<Result<bool>> DeleteEmployeeAsync(Guid employeeId, Guid tenantId);
    }
}
