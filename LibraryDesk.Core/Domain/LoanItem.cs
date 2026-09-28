namespace LibraryDesk.Core.Domain;

/// <summary>Позиція у формулярі видачі: книга, кількість днів прокату та тариф за день.</summary>
public sealed class LoanItem
{
    public string Isbn { get; init; } = string.Empty;

    public int Days { get; init; }

    public decimal DailyRate { get; init; }

    public decimal Amount => Days * DailyRate;
}
