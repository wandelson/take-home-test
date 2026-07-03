namespace Fundo.Domain.Events;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
