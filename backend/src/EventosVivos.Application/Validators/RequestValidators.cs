using EventosVivos.Application.DTOs;
using EventosVivos.Domain.Enums;
using FluentValidation;

namespace EventosVivos.Application.Validators;

public sealed class CreateEventRequestValidator : AbstractValidator<CreateEventRequest>
{
    public CreateEventRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .Length(5, 100);

        RuleFor(x => x.Description)
            .NotEmpty()
            .Length(10, 500);

        RuleFor(x => x.VenueId)
            .GreaterThan(0);

        RuleFor(x => x.MaxCapacity)
            .GreaterThan(0);

        RuleFor(x => x.StartDateTime)
            .NotEmpty();

        RuleFor(x => x.EndDateTime)
            .GreaterThan(x => x.StartDateTime)
            .WithMessage("End date must be after start date.");

        RuleFor(x => x.TicketPrice)
            .GreaterThan(0);

        RuleFor(x => x.Type)
            .IsInEnum();
    }
}

public sealed class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.BuyerName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.BuyerEmail)
            .NotEmpty()
            .EmailAddress();
    }
}
