namespace LibraryDesk.Core.Domain;

/// <summary>Незмінна позиція формуляра видачі книги.</summary>
public sealed record LoanItem
{
    /// <summary>Створює перевірену позицію прокату.</summary>
    /// <param name="isbn">Ідентифікатор книги в каталозі.</param>
    /// <param name="days">Кількість днів прокату.</param>
    /// <param name="dailyRate">Денний тариф у грошових одиницях.</param>
    public LoanItem(string isbn, int days, decimal dailyRate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(isbn);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(days);
        ArgumentOutOfRangeException.ThrowIfNegative(dailyRate);

        Isbn = isbn;
        Days = days;
        DailyRate = dailyRate;
    }

    /// <summary>Ідентифікатор книги в каталозі.</summary>
    public string Isbn { get; }

    /// <summary>Кількість днів прокату.</summary>
    public int Days { get; }

    /// <summary>Денний тариф за прокат книги.</summary>
    public decimal DailyRate { get; }

    /// <summary>Вартість прокату без знижки.</summary>
    public decimal Amount => Days * DailyRate;
}
