namespace LibraryDesk.Core.Domain;

/// <summary>Книга у фонді бібліотеки.</summary>
public sealed class Book
{
    public string Isbn { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public decimal RentalFee { get; init; }

    public int AvailableCopies { get; init; }
}
