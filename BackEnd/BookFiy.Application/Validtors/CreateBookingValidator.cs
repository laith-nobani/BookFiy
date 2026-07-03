using BookFiy.Application.Dtos.Booking;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Application.Validtors
{
    public class CreateBookingValidator : FluentValidation.AbstractValidator<CreateBookingDto>
    {
        public CreateBookingValidator() {


            RuleFor(x => x.StartTime)
               .NotEmpty().WithMessage("Start time is required.");

            RuleFor(x=>x.UserId)
                .NotEmpty().WithMessage("UserId is required.");

            RuleFor(x => x.ServiceId)
                .NotEmpty().WithMessage("ServiceId is required.");

            RuleFor(x => x.TenantId)
                .NotEmpty().WithMessage("TenantId is required.");

        }
    }
}
