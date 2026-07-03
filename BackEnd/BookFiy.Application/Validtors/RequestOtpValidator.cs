using FluentValidation;
using BookFiy.Application.Dtos.Auth;

namespace BookFiy.Application.Validtors
{
    public class RequestOtpValidator : FluentValidation.AbstractValidator<RegisterRequest>
    {
        public RequestOtpValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.TenantId)
                .NotEmpty().WithMessage("TenantId is required.");
        }
    }
}
