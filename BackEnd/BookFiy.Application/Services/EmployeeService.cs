using BookFiy.Domain.IRepositories;
using BookFiy.Domain.Entites;
using Microsoft.AspNetCore.Identity;
using BookFiy.Domain.Entities;
using BookFiy.Application.Interfaces;
using BookFiy.Application.Dtos.Employee;
using BookFiy.Application.Comman;

namespace BookFiy.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public EmployeeService(IEmployeeRepository employeeRepository, UserManager<ApplicationUser> userManager,IEmailService emailService)
        {
            _employeeRepository = employeeRepository;
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<Result<EmployeeDto>> GetEmployeeByIdAsync(Guid employeeId, Guid tenantId)
        {
            var emp = await _employeeRepository.GetEmployeeByIdAsync(employeeId, tenantId);
            if (emp == null)
                return Result<EmployeeDto>.Failure("Employee not found.", ErrorType.NotFound);

            var user = await _userManager.FindByIdAsync(emp.UserId.ToString());

            var res= new EmployeeDto
            {
                Id = emp.Id,
                FirstName = user?.FullName?.Split(' ').FirstOrDefault() ?? string.Empty,
                LastName = user?.FullName?.Split(' ').Skip(1).FirstOrDefault() ?? string.Empty,
                JobTitle = emp.JobTitle,
                Bio = emp.Bio,
                Email = user?.Email ?? string.Empty,
                PhoneNumber = user?.PhoneNumber ?? string.Empty,
                TenantName = emp.Tenant?.Name ?? string.Empty
            };

            return Result<EmployeeDto>.Success(res);
        }

        public async Task<Result<List<EmployeeDto>>> GetAllEmployeesAsync(Guid tenantId)
        {
            var list = await _employeeRepository.GetAllEmployeesAsync(tenantId);
            var result = new List<EmployeeDto>();
            foreach (var emp in list)
            {
                var user = await _userManager.FindByIdAsync(emp.UserId.ToString());
                result.Add(new EmployeeDto
                {
                    Id = emp.Id,
                    FirstName = user?.FullName?.Split(' ').FirstOrDefault() ?? string.Empty,
                    LastName = user?.FullName?.Split(' ').Skip(1).FirstOrDefault() ?? string.Empty,
                    JobTitle = emp.JobTitle,
                    Bio = emp.Bio,
                    Email = user?.Email ?? string.Empty,
                    PhoneNumber = user?.PhoneNumber ?? string.Empty,
                    TenantName = emp.Tenant?.Name ?? string.Empty
                });
            }

           return Result<List<EmployeeDto>>.Success(result);
        }

        public async Task<Result<EmployeeDto>> RegisterEmployeeAsync(CraateEmployeeDto request, Guid tenantId, Guid createdBy)
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = request.Email,
                Email = request.Email,
                FullName = $"{request.FirstName} {request.LastName}",
                PhoneNumber = request.PhoneNumber,
                TenantId = tenantId,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var password= GenerateTemporaryPassword();

            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
                throw new InvalidOperationException(string.Join(", ", createResult.Errors.Select(e => e.Description)));

            await _emailService.CreateAndSendTemporaryPasswordAsync(user.Email, password);

            if (string.IsNullOrEmpty(password))
                throw new InvalidOperationException("Failed to generate temporary password.");


            await _userManager.AddToRoleAsync(user, "Employee");

            var employee = new Employee().Create(user.Id, request.JobTitle, request.Bio, tenantId,createdBy);
            await _employeeRepository.CreateEmployeeAsync(employee);

            var res= new EmployeeDto
            {
                Id = employee.Id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                JobTitle = employee.JobTitle,
                Bio = employee.Bio,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                TenantName = employee.Tenant?.Name ?? string.Empty
            };

            return Result<EmployeeDto>.Success(res);
        }

        public string GenerateTemporaryPassword(int length = 12)
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            var random = new Random();
            var passwordChars = new char[length];
            for (int i = 0; i < length; i++)
            {
                passwordChars[i] = validChars[random.Next(validChars.Length)];
            }
            return new string(passwordChars);
        }
        public async Task<Result<bool>> UpdateEmployeeAsync(Guid employeeId, UpdateEmployeeDto request, Guid tenantId)
        {
             var empTask =await _employeeRepository.GetEmployeeByIdAsync(employeeId, tenantId);

             if (empTask == null)
               return Result<bool>.Failure("Employee not found.", ErrorType.NotFound);


            if (empTask.TenantId != tenantId)
                return Result<bool>.Failure("You do not have permission to update this employee.", ErrorType.Forbidden);


            var user = await _userManager.FindByIdAsync(empTask.UserId.ToString());

            if (user == null)
                return Result<bool>.Failure("Associated user not found.", ErrorType.NotFound);

            if (user.TenantId != tenantId)
                return Result<bool>.Failure("You do not have permission to update this employee's user.", ErrorType.NotFound);

            user.UpdateUser(request.Email, request.Email, $"{request.FirstName} {request.LastName}",request.PhoneNumber);
            var userUpdateResult = await _userManager.UpdateAsync(user);
            if (!userUpdateResult.Succeeded)
            {
                return Result<bool>.Failure(string.Join(", ", userUpdateResult.Errors.Select(e => e.Description)),ErrorType.Validation);
            }

            empTask.Update(request.JobTitle, request.Bio);

            await _employeeRepository.UpdateEmployeeAsync(empTask);

            return Result<bool>.Success(true, "Employee updated successfully.");
        }

        public async Task<Result<bool>> DeleteEmployeeAsync(Guid employeeId, Guid tenantId)
        {
            var empTask =await _employeeRepository.GetEmployeeByIdAsync(employeeId, tenantId);
            if (empTask == null)
                return Result<bool>.Failure("Employee not found.", ErrorType.NotFound);
            
            if(empTask.TenantId != tenantId)
                return Result<bool>.Failure("You do not have permission to delete this employee.", ErrorType.Forbidden);
            var user = await _userManager.FindByIdAsync(empTask.UserId.ToString());

            if (user == null)
                return Result<bool>.Failure("Associated user not found.", ErrorType.NotFound);

            if(user.TenantId != tenantId)
                return Result<bool>.Failure("You do not have permission to delete this employee's user.", ErrorType.Forbidden);

            user.SoftDelete();
            var userUpdateResult = await _userManager.UpdateAsync(user);
            if (!userUpdateResult.Succeeded)
                {
                return Result<bool>.Failure(string.Join(", ", userUpdateResult.Errors.Select(e => e.Description)), ErrorType.Validation);
            }
           
            empTask.SoftDelete();
            await _employeeRepository.UpdateEmployeeAsync(empTask);

            return Result<bool>.Success(true, "Employee deleted successfully.");
        }
    }
}
