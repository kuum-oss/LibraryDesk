using LibraryDesk.Core.Abstractions;
using LibraryDesk.Core.Domain;

namespace LibraryDesk.Core.Storage;

/// <summary>Сховище формулярів у пам'яті процесу.</summary>
public sealed class InMemoryLoanRepository : ILoanRepository
{
    private readonly Dictionary<int, Loan> _loans = new();

    /// <inheritdoc />
    public void Add(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);
        if (!_loans.TryAdd(loan.Id, loan))
        {
            throw new InvalidOperationException("Формуляр із таким ідентифікатором уже існує.");
        }
    }

    /// <inheritdoc />
    public Loan? GetById(int id)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        return _loans.GetValueOrDefault(id);
    }

    /// <inheritdoc />
    public IReadOnlyList<Loan> GetAll() => Array.AsReadOnly(_loans.Values.ToArray());
}
