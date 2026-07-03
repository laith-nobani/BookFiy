using FluentValidation;
using BookFiy.Application.Dtos.Employee;

namespace BookFiy.Application.Validtors
{
    public class CreateEmployeeValidator : FluentValidation.AbstractValidator<CraateEmployeeDto>
    {
        public CreateEmployeeValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required");
            RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required");
            RuleFor(x => x.JobTitle).NotEmpty().WithMessage("Job title is required");
            RuleFor(x => x.TenantId).NotEmpty().WithMessage("TenantId is required");
        }
    }
}
