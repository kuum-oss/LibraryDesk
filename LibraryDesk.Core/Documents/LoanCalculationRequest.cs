using LibraryDesk.Core.Domain;

namespace LibraryDesk.Core.Documents;

/// <summary>Незмінний набір параметрів розрахунку формуляра.</summary>
public sealed record LoanCalculationRequest(
    int LoanId,
    string ReaderName,
    string ReaderEmail,
    bool IsRegularReader,
    IReadOnlyList<LoanItem> Items,
    DateOnly IssuedOn,
    string? CouponCode,
    LoanStatus Status,
    decimal DeliveryPrice);
