using LibraryDesk.Core.Domain;

namespace LibraryDesk.Core.Reports;

/// <summary>Готові дані одного рядка звіту без способу їх виведення.</summary>
/// <param name="Id">Ідентифікатор формуляра.</param>
/// <param name="Total">Вартість прокату.</param>
/// <param name="Status">Поточний стан.</param>
public sealed record LoanReportRow(int Id, decimal Total, LoanStatus Status);
