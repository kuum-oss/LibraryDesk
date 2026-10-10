using LibraryDesk.Core.Domain;
using LibraryDesk.Core.Errors;
using LibraryDesk.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LibraryDesk.Tests;

public sealed class LoanServiceDoubleTests
{
    [Fact]
    public void TotalOf_TwoItems_SumsPricesFromStub()
    {
        FakeLoanRepository repository = new();
        LoanService service = NewService(repository, new StubPricingPolicy(25m));
        Loan loan = NewLoan(7, 1);
        loan.AddItem(new LoanItem("A-1", 2, 10m));
        loan.AddItem(new LoanItem("B-2", 1, 30m));

        decimal actual = service.TotalOf(loan);

        Assert.Equal(50m, actual);
    }

    [Fact]
    public void Register_ActiveLoan_StoresInFakeAndAddsOnce()
    {
        FakeLoanRepository repository = new();
        LoanService service = NewService(repository, new StubPricingPolicy(25m));
        Loan loan = NewLoan(1, 5);
        loan.AddItem(new LoanItem("A-1", 1, 5m));
        loan.Issue();
        Reader reader = new(5, "Олена Коваль", "olena@example.com", true);

        service.Register(loan, reader);

        Assert.Same(loan, repository.GetById(1));
        Assert.Equal(1, repository.AddCalls);
    }

    [Fact]
    public void GetRequiredLoan_UnknownId_ThrowsDomainRule()
    {
        LoanService service = NewService(new FakeLoanRepository(), new StubPricingPolicy(1m));

        DomainRuleException exception = Assert.Throws<DomainRuleException>(
            () => service.GetRequiredLoan(404));

        Assert.Equal("loan.exists", exception.Rule);
    }

    private static LoanService NewService(
        FakeLoanRepository repository,
        StubPricingPolicy pricing)
        => new(repository, pricing, new StubNotifier(), NullLogger<LoanService>.Instance);

    private static Loan NewLoan(int id, int readerId)
        => new(
            id,
            readerId,
            new DateTimeOffset(2026, 10, 10, 10, 0, 0, TimeSpan.FromHours(3)),
            new DateOnly(2026, 10, 24));
}
