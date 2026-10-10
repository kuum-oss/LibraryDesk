using LibraryDesk.Core.Abstractions;
using LibraryDesk.Core.Domain;

namespace LibraryDesk.Tests;

internal sealed class FakeLoanRepository : ILoanRepository
{
    private readonly Dictionary<int, Loan> _items = new();

    public int AddCalls { get; private set; }

    public void Add(Loan loan)
    {
        _items.Add(loan.Id, loan);
        AddCalls++;
    }

    public Loan? GetById(int id) => _items.GetValueOrDefault(id);

    public IReadOnlyList<Loan> GetAll() => _items.Values.ToArray();
}

internal sealed class StubPricingPolicy : IPricingPolicy
{
    private readonly decimal _price;

    public StubPricingPolicy(decimal price) => _price = price;

    public decimal PriceOf(LoanItem item) => _price;
}

internal sealed class StubNotifier : INotifier
{
    public void Notify(Reader reader, Loan loan, decimal total)
    {
    }
}
