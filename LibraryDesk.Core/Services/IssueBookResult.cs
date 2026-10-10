using LibraryDesk.Core.Domain;

namespace LibraryDesk.Core.Services;

/// <summary>Результат успішної видачі книги.</summary>
/// <param name="LoanId">Ідентифікатор формуляра.</param>
/// <param name="DueOn">Останній день повернення.</param>
/// <param name="Status">Новий стан формуляра.</param>
public sealed record IssueBookResult(int LoanId, DateOnly DueOn, LoanStatus Status);
