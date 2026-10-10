namespace LibraryDesk.Core.Services;

/// <summary>Один рядок звіту про боржників.</summary>
public sealed record DebtorReportRow
{
    /// <summary>Ідентифікатор формуляра.</summary>
    public int LoanId { get; init; }

    /// <summary>Ідентифікатор читача.</summary>
    public int ReaderId { get; init; }

    /// <summary>Умовне ім'я читача для відображення.</summary>
    public required string ReaderName { get; init; }

    /// <summary>Останній день повернення.</summary>
    public DateOnly DueOn { get; init; }

    /// <summary>Кількість прострочених днів.</summary>
    public int OverdueDays { get; init; }

    /// <summary>Сумарна пеня за формуляром.</summary>
    public decimal LateFee { get; init; }
}
