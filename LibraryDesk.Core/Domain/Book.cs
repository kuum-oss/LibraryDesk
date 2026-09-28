namespace LibraryDesk.Core.Domain;

/// <summary>Книга у фонді бібліотеки.</summary>
public sealed class Book
{
    /// <summary>Міжнародний стандартний номер книги.</summary>
    public string Isbn { get; init; } = string.Empty;

    /// <summary>Назва книги.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Вартість оренди книги.</summary>
    public decimal RentalFee { get; init; }

    /// <summary>Кількість доступних примірників.</summary>
    public int AvailableCopies { get; init; }
}
