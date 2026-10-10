namespace LibraryDesk.Core.Services;

/// <summary>Вхідні дані реєстрації читача.</summary>
public sealed record RegisterReaderRequest
{
    /// <summary>Додатний ідентифікатор читача.</summary>
    public int Id { get; init; }

    /// <summary>Ім'я умовного читача.</summary>
    public required string FullName { get; init; }

    /// <summary>Навчальна адреса електронної пошти.</summary>
    public required string Email { get; init; }

    /// <summary>Ознака активного абонемента.</summary>
    public bool HasActiveMembership { get; init; } = true;
}
