namespace LibraryDesk.Core.Legacy;

/// <summary>Рядок формуляра у початковій реалізації ЛР3.</summary>
public sealed class LoanLine
{
    /// <summary>ISBN книги.</summary>
    public string Isbn { get; set; } = string.Empty;

    /// <summary>Кількість днів прокату.</summary>
    public int Days { get; set; }

    /// <summary>Тариф за один день.</summary>
    public decimal DailyRate { get; set; }
}
