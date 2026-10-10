using LibraryDesk.Core.Domain;
using LibraryDesk.Core.Errors;
using LibraryDesk.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LibraryDesk.Tests;

public sealed class LoanServiceCoverageTests
{
    [Fact]
    public void Register_DraftLoan_ThrowsRegisterableRule()
    {
        LoanService service = NewService(new FakeLoanRepository(), new StubPricingPolicy(1m));
        Loan loan = NewLoan(1, 5);
        Reader reader = NewReader(5);

        DomainRuleException exception = Assert.Throws<DomainRuleException>(
            () => service.Register(loan, reader));

        Assert.Equal("loan.registerable", exception.Rule);
    }

    [Fact]
    public void Register_ReaderMismatch_ThrowsReaderRule()
    {
        LoanService service = NewService(new FakeLoanRepository(), new StubPricingPolicy(1m));
        Loan loan = ActiveLoan(2, 5);
        Reader anotherReader = NewReader(6);

        DomainRuleException exception = Assert.Throws<DomainRuleException>(
            () => service.Register(loan, anotherReader));

        Assert.Equal("loan.reader", exception.Rule);
    }

    [Fact]
    public void Register_ZeroTotal_StoresLoan()
    {
        FakeLoanRepository repository = new();
        LoanService service = NewService(repository, new StubPricingPolicy(0m));
        Loan loan = ActiveLoan(3, 5);
        Reader reader = NewReader(5);

        service.Register(loan, reader);

        Assert.Same(loan, repository.GetById(3));
    }

    [Fact]
    public void Register_RepositoryFailure_RethrowsIOException()
    {
        LoanService service = NewService(new ThrowingLoanRepository(), new StubPricingPolicy(1m));
        Loan loan = ActiveLoan(4, 5);
        Reader reader = NewReader(5);

        IOException exception = Assert.Throws<IOException>(() => service.Register(loan, reader));

        Assert.Equal("Сховище недоступне.", exception.Message);
    }

    [Fact]
    public void GetRequiredLoan_KnownId_ReturnsStoredLoan()
    {
        FakeLoanRepository repository = new();
        Loan expected = NewLoan(5, 5);
        repository.Add(expected);
        LoanService service = NewService(repository, new StubPricingPolicy(1m));

        Loan actual = service.GetRequiredLoan(5);

        Assert.Same(expected, actual);
    }

    [Fact]
    public void AddItem_KnownDraft_AddsItem()
    {
        FakeLoanRepository repository = new();
        Loan loan = NewLoan(6, 5);
        repository.Add(loan);
        LoanService service = NewService(repository, new StubPricingPolicy(1m));

        service.AddItem(6, new LoanItem("ISBN-6", 2, 10m));

        Assert.Single(loan.Items);
    }

    [Fact]
    public void Cancel_KnownDraft_ChangesStatusToCancelled()
    {
        FakeLoanRepository repository = new();
        Loan loan = NewLoan(7, 5);
        repository.Add(loan);
        LoanService service = NewService(repository, new StubPricingPolicy(1m));

        service.Cancel(7, "Помилкове оформлення");

        Assert.Equal(LoanStatus.Cancelled, loan.Status);
    }

    [Fact]
    public void TotalOf_NegativePrice_ThrowsNonnegativeRule()
    {
        LoanService service = NewService(new FakeLoanRepository(), new StubPricingPolicy(-1m));
        Loan loan = NewLoan(8, 5);
        loan.AddItem(new LoanItem("ISBN-8", 1, 10m));

        DomainRuleException exception = Assert.Throws<DomainRuleException>(
            () => service.TotalOf(loan));

        Assert.Equal("loan.total.nonnegative", exception.Rule);
    }

    private static LoanService NewService(
        LibraryDesk.Core.Abstractions.ILoanRepository repository,
        StubPricingPolicy pricing)
        => new(repository, pricing, new StubNotifier(), NullLogger<LoanService>.Instance);

    private static Loan ActiveLoan(int id, int readerId)
    {
        Loan loan = NewLoan(id, readerId);
        loan.AddItem(new LoanItem($"ISBN-{id}", 1, 10m));
        loan.Issue();
        return loan;
    }

    private static Loan NewLoan(int id, int readerId)
        => new(
            id,
            readerId,
            new DateTimeOffset(2026, 10, 10, 10, 0, 0, TimeSpan.FromHours(3)),
            new DateOnly(2026, 10, 24));

    private static Reader NewReader(int id)
        => new(id, $"Читач {id}", $"reader{id}@example.com", true);
}
