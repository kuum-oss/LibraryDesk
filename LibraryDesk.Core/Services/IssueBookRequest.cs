namespace LibraryDesk.Core.Services;

/// <summary>Вхідні дані видачі однієї книги.</summary>
public sealed record IssueBookRequest
{
    /// <summary>Новий ідентифікатор формуляра.</summary>
    public int LoanId { get; init; }

    /// <summary>Ідентифікатор читача.</summary>
    public int ReaderId { get; init; }

    /// <summary>ISBN книги.</summary>
    public required string Isbn { get; init; }
}
