using LibraryDesk.Core.Domain;

namespace LibraryDesk.Core.Documents;

/// <summary>Результат розрахунку суми та наступного статусу.</summary>
public sealed record LoanTotalResult(decimal Total, LoanStatus NextStatus);
