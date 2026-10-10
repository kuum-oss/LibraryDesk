namespace LibraryDesk.Core.Domain;

/// <summary>Незмінна позиція формуляра видачі книги.</summary>
public sealed record LoanItem
{
    /// <summary>Типова вартість заміни примірника.</summary>
    public const decimal DefaultReplacementPrice = Book.DefaultReplacementPrice;

    /// <summary>Створює перевірену позицію прокату.</summary>
    /// <param name="isbn">Ідентифікатор книги в каталозі.</param>
    /// <param name="days">Кількість днів прокату.</param>
    /// <param name="dailyRate">Денний тариф у грошових одиницях.</param>
    /// <param name="replacementPrice">Вартість заміни примірника.</param>
    public LoanItem(
        string isbn,
        int days,
        decimal dailyRate,
        decimal replacementPrice = DefaultReplacementPrice)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(isbn);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(days);
        ArgumentOutOfRangeException.ThrowIfNegative(dailyRate);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(replacementPrice);

        Isbn = isbn;
        Days = days;
        DailyRate = dailyRate;
        ReplacementPrice = replacementPrice;
    }

    /// <summary>Ідентифікатор книги в каталозі.</summary>
    public string Isbn { get; }

    /// <summary>Кількість днів прокату.</summary>
    public int Days { get; }

    /// <summary>Денний тариф за прокат книги.</summary>
    public decimal DailyRate { get; }

    /// <summary>Вартість заміни примірника, якою обмежено пеню.</summary>
    public decimal ReplacementPrice { get; }

    /// <summary>Вартість прокату без знижки.</summary>
    public decimal Amount => Days * DailyRate;
}
