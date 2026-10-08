using LibraryDesk.Core.Abstractions;
using LibraryDesk.Core.Domain;
using LibraryDesk.Core.Errors;
using LibraryDesk.Core.Pricing;
using LibraryDesk.Core.Services;
using LibraryDesk.Core.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LibraryDesk.Tests;

public sealed class Lab4InvariantTests
{
    [Fact]
    public void LoanItem_WithZeroDays_ThrowsOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LoanItem("ISBN-1", 0, 100m));
    }

    [Fact]
    public void Loan_BeforeIssue_RejectsReturn()
    {
        Loan loan = NewLoan(1);

        Assert.Throws<DomainRuleException>(() => loan.Return());
    }

    [Fact]
    public void LoanService_WithInjectedTenPercentPolicy_Returns180()
    {
        ILoanRepository repository = new InMemoryLoanRepository();
        IPricingPolicy pricing = new DiscountPricingPolicy(0.10m);
        LoanService service = new(repository, pricing, new TestNotifier(), NullLogger<LoanService>.Instance);
        Loan loan = NewLoan(2);
        loan.AddItem(new LoanItem("ISBN-2", 2, 100m));

        Assert.Equal(180m, service.TotalOf(loan));
    }

    private static Loan NewLoan(int id)
        => new(id, 100, new DateTimeOffset(2026, 10, 8, 10, 0, 0, TimeSpan.FromHours(3)),
            new DateOnly(2026, 10, 22));

    private sealed class TestNotifier : INotifier
    {
        public void Notify(Reader reader, Loan loan, decimal total)
        {
        }
    }
}
