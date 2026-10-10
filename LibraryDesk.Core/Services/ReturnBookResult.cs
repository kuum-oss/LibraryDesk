namespace LibraryDesk.Core.Services;

/// <summary>Результат успішного повернення книги.</summary>
/// <param name="LoanId">Ідентифікатор формуляра.</param>
/// <param name="OverdueDays">Кількість прострочених днів.</param>
/// <param name="LateFee">Нарахована пеня.</param>
public sealed record ReturnBookResult(int LoanId, int OverdueDays, decimal LateFee);
