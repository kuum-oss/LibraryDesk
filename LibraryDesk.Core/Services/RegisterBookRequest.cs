namespace LibraryDesk.Core.Services;

/// <summary>Вхідні дані реєстрації книги на межі прикладного шару.</summary>
public sealed record RegisterBookRequest
{
    /// <summary>ISBN книги.</summary>
    public required string Isbn { get; init; }

    /// <summary>Назва книги.</summary>
    public required string Title { get; init; }

    /// <summary>Денний тариф і базова денна пеня.</summary>
    public decimal DailyFee { get; init; }

    /// <summary>Вартість заміни примірника.</summary>
    public decimal ReplacementPrice { get; init; }

    /// <summary>Кількість примірників, що надходять.</summary>
    public int Copies { get; init; }
}
