using EventosVivos.Application.DTOs;
using EventosVivos.Domain.Enums;
using FluentValidation;

namespace EventosVivos.Application.Validators;

public sealed class CreateEventRequestValidator : AbstractValidator<CreateEventRequest>
{
    public CreateEventRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es obligatorio.")
            .Length(5, 100).WithMessage("El título debe tener entre 5 y 100 caracteres.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .Length(10, 500).WithMessage("La descripción debe tener entre 10 y 500 caracteres.");

        RuleFor(x => x.VenueId)
            .GreaterThan(0).WithMessage("Debes seleccionar un lugar válido.");

        RuleFor(x => x.MaxCapacity)
            .GreaterThan(0).WithMessage("La capacidad máxima debe ser mayor a cero.");

        RuleFor(x => x.StartDateTime)
            .NotEmpty().WithMessage("La fecha de inicio es obligatoria.");

        RuleFor(x => x.EndDateTime)
            .GreaterThan(x => x.StartDateTime)
            .WithMessage("La fecha de fin debe ser posterior a la de inicio.");

        RuleFor(x => x.TicketPrice)
            .GreaterThan(0).WithMessage("El precio de entrada debe ser mayor a cero.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Debes seleccionar un tipo de evento válido.");
    }
}

public sealed class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("El evento es obligatorio.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(1).WithMessage("La cantidad debe ser al menos 1.");

        RuleFor(x => x.BuyerName)
            .NotEmpty().WithMessage("El nombre del comprador es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres.");

        RuleFor(x => x.BuyerEmail)
            .NotEmpty().WithMessage("El correo del comprador es obligatorio.")
            .EmailAddress().WithMessage("El correo del comprador no tiene un formato válido.");
    }
}
