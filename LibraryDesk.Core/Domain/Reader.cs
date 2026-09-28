namespace LibraryDesk.Core.Domain;

/// <summary>Читач бібліотеки.</summary>
public sealed class Reader
{
    /// <summary>Унікальний ідентифікатор читача.</summary>
    public int Id { get; init; }

    /// <summary>Повне ім'я читача.</summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>Ознака наявності активного членства у бібліотеці.</summary>
    public bool HasActiveMembership { get; init; }
}
