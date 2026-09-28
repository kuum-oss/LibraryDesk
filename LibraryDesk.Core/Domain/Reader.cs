namespace LibraryDesk.Core.Domain;

/// <summary>Читач бібліотеки.</summary>
public sealed class Reader
{
    public int Id { get; init; }

    public string FullName { get; init; } = string.Empty;

    public bool HasActiveMembership { get; init; }
}
