namespace EventosVivos.Domain.Exceptions;

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object key)
        : base(BuildMessage(entityName, key))
    {
        EntityName = entityName;
        Key = key;
    }

    public string EntityName { get; }
    public object Key { get; }

    private static string BuildMessage(string entityName, object key) =>
        entityName switch
        {
            "Event" => "No se encontró el evento solicitado.",
            "Reservation" => "No se encontró la reserva solicitada.",
            "Venue" => "No se encontró el lugar solicitado.",
            _ => $"No se encontró {entityName} con id '{key}'."
        };
}
