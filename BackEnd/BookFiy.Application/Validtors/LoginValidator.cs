using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using BookFiy.Application.Dtos.Auth;

namespace BookFiy.Application.Validtors
{
    public class LoginValidator : FluentValidation.AbstractValidator<LoginRequest>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");

       
        }
    }
}
