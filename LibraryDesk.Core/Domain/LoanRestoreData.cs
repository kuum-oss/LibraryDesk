namespace LibraryDesk.Core.Domain;

/// <summary>Внутрішній перевірений знімок для відновлення формуляра зі сховища.</summary>
internal sealed record LoanRestoreData
{
    /// <summary>Ідентифікатор формуляра.</summary>
    public int Id { get; init; }

    /// <summary>Ідентифікатор читача.</summary>
    public int ReaderId { get; init; }

    /// <summary>Дата й час видачі.</summary>
    public DateTimeOffset IssuedAt { get; init; }

    /// <summary>Останній день повернення.</summary>
    public DateOnly DueOn { get; init; }

    /// <summary>Збережений стан.</summary>
    public LoanStatus Status { get; init; }

    /// <summary>Збережені позиції.</summary>
    public required IEnumerable<LoanItem> Items { get; init; }
}
