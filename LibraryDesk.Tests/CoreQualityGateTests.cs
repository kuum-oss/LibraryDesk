using LibraryDesk.Core;
using LibraryDesk.Core.Domain;
using LibraryDesk.Core.Errors;
using LibraryDesk.Core.Pricing;
using LibraryDesk.Core.Reports;
using LibraryDesk.Core.Storage;
using Xunit;

namespace LibraryDesk.Tests;

public sealed class CoreQualityGateTests
{
    [Fact]
    public void Book_RestockAndLend_UpdatesAvailableCopies()
    {
        Book book = new("978-966-00-0001-0", "Тестування", 25m, 1);

        book.Restock(2);
        book.LendCopy();

        Assert.Equal(2, book.AvailableCopies);
        Assert.Throws<ArgumentOutOfRangeException>(() => book.Restock(0));
    }

    [Fact]
    public void Book_LendWithoutCopies_ThrowsInvalidOperation()
    {
        Book book = new("978-966-00-0002-7", "CI", 10m, 0);

        Assert.Throws<InvalidOperationException>(book.LendCopy);
    }

    [Fact]
    public void Result_Factories_CreateSuccessAndFailure()
    {
        Result<string> success = Result.Ok("готово");
        Result<string> failure = Result.Fail<string>("помилка");

        Assert.True(success.IsSuccess);
        Assert.Equal("готово", success.Value);
        Assert.False(failure.IsSuccess);
        Assert.Equal("помилка", failure.Error);
        Assert.Throws<ArgumentNullException>(() => Result.Ok<string>(null!));
        Assert.Throws<ArgumentException>(() => Result.Fail<string>(string.Empty));
    }

    [Fact]
    public void ReportBuilders_ProduceDeterministicText()
    {
        ReportBuilder report = new() { Title = "Підсумок" };
        LoanReportRow row = new(7, 12.5m, LoanStatus.Active);
        List<string> reportRows = new() { "A", "B" };
        List<LoanReportRow> csvRows = new() { row };

        string text = report.Build(reportRows);
        string csv = LoanCsvReport.BuildCsv(csvRows);

        Assert.Equal($"ПІДСУМОК{Environment.NewLine}A{Environment.NewLine}B{Environment.NewLine}", text);
        Assert.Equal($"id;total;status{Environment.NewLine}7;12.5;Active", csv);
        Assert.Throws<ArgumentNullException>(() => report.Build(null!));
        Assert.Throws<ArgumentNullException>(() => LoanCsvReport.BuildCsv(null!));
    }

    [Fact]
    public void StandardPricingPolicy_ReturnsLoanItemAmount()
    {
        StandardPricingPolicy policy = new();
        LoanItem item = new("978-966-00-0003-4", 3, 20m);

        Assert.Equal(60m, policy.PriceOf(item));
        Assert.Throws<ArgumentNullException>(() => policy.PriceOf(null!));
    }

    [Fact]
    public void VersionInfo_ReturnsConfiguredSemanticVersion()
    {
        Assert.Equal("1.0.0", VersionInfo.Current());
    }

    [Fact]
    public void InMemoryRepository_StoresAndRejectsDuplicateLoan()
    {
        InMemoryLoanRepository repository = new();
        Loan loan = CreateLoan(101);

        repository.Add(loan);

        Assert.Same(loan, repository.GetById(101));
        Assert.Single(repository.GetAll());
        Assert.Throws<InvalidOperationException>(() => repository.Add(loan));
        Assert.Throws<ArgumentOutOfRangeException>(() => repository.GetById(0));
        Assert.Throws<ArgumentNullException>(() => repository.Add(null!));
    }

    [Fact]
    public void Order_CalculatesReportAndStateTransitions()
    {
        Order order = new("ORD-1", "Читач");
        order.AddLine("BOOK-1", 2, 100m);

        decimal total = order.CalculateTotal(false);
        string report = order.BuildReport();

        Assert.True(order.IsValid());
        Assert.Equal(240m, total);
        Assert.Contains("BOOK-1", report, StringComparison.Ordinal);
        Assert.True(order.TryChangeStatus(1));
        Assert.True(order.TryChangeStatus(2));
        Assert.False(order.TryChangeStatus(3));
    }

    [Fact]
    public void Order_FindById_ReturnsMatchOrNull()
    {
        Order first = new("ORD-1", "Читач");
        Order second = new("ORD-2", "Бібліотекар");
        List<Order> orders = new() { first, second };

        Assert.Same(second, Order.FindById(orders, "ORD-2"));
        Assert.Null(Order.FindById(orders, "ORD-3"));
    }

    private static Loan CreateLoan(int id)
    {
        Reader reader = new(id, "Читач", $"reader{id}@example.com", true);
        Loan loan = new(
            id,
            reader.Id,
            new DateTimeOffset(2026, 10, 10, 10, 0, 0, TimeSpan.Zero),
            new DateOnly(2026, 10, 24));
        loan.AddItem(new LoanItem("978-966-00-0004-1", 1, 10m));
        return loan;
    }
}
