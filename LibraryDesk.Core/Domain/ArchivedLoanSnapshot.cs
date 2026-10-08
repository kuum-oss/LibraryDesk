namespace LibraryDesk.Core.Domain;

/// <summary>Архівний перегляд завершеного формуляра через композицію.</summary>
public sealed class ArchivedLoanSnapshot
{
    private readonly Loan _loan;

    /// <summary>Архівує завершений формуляр.</summary>
    /// <param name="loan">Формуляр у кінцевому стані.</param>
    /// <param name="archivedOn">Дата архівування.</param>
    public ArchivedLoanSnapshot(Loan loan, DateOnly archivedOn)
    {
        ArgumentNullException.ThrowIfNull(loan);
        if (loan.Status is not (LoanStatus.Returned or LoanStatus.Cancelled or LoanStatus.Lost))
        {
            throw new InvalidOperationException("Архівують лише завершений формуляр.");
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(
            archivedOn,
            DateOnly.FromDateTime(loan.IssuedAt.Date));
        _loan = loan;
        ArchivedOn = archivedOn;
    }

    /// <summary>Дата архівування.</summary>
    public DateOnly ArchivedOn { get; }

    /// <summary>Ідентифікатор завершеного формуляра.</summary>
    public int Id => _loan.Id;

    /// <summary>Кінцевий стан формуляра.</summary>
    public LoanStatus Status => _loan.Status;

    /// <summary>Незмінний перегляд позицій формуляра.</summary>
    public IReadOnlyList<LoanItem> Items => _loan.Items;
}
