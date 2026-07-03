using BookFiy.Application.Dtos.Auth;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Application.Validtors
{
    public class RegisterValidator : FluentValidation.AbstractValidator<RegisterRequest>
    {
        public RegisterValidator() { 
        
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username is required");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
             .NotEmpty().WithMessage("Password is required.")
             .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
             .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
             .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
             .Matches("[0-9]").WithMessage("Password must contain at least one number.")
             .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required");

            RuleFor(x => x.TenantId)
                .NotEmpty().WithMessage("TenantId is required");

        }
    }
}
