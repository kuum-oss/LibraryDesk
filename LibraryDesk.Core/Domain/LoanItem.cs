namespace LibraryDesk.Core.Domain;

/// <summary>Позиція у формулярі видачі: книга, кількість днів прокату та тариф за день.</summary>
public sealed class LoanItem
{
    /// <summary>Міжнародний стандартний номер книги.</summary>
    public string Isbn { get; init; } = string.Empty;

    /// <summary>Кількість днів прокату.</summary>
    public int Days { get; init; }

    /// <summary>Денний тариф за прокат книги.</summary>
    public decimal DailyRate { get; init; }

    /// <summary>Загальна сума за прокат цієї позиції.</summary>
    public decimal Amount => Days * DailyRate;
}
