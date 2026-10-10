namespace LibraryDesk.Core.Services;

/// <summary>Вхідні дані приймання повернення.</summary>
public sealed record ReturnBookRequest
{
    /// <summary>Ідентифікатор формуляра.</summary>
    public int LoanId { get; init; }

    /// <summary>Фактична дата повернення.</summary>
    public DateOnly ReturnedOn { get; init; }
}
