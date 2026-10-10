using LibraryDesk.Core.Domain;
using LibraryDesk.Core.Errors;
using LibraryDesk.Core.Pricing;
using LibraryDesk.Core.Services;
using LibraryDesk.Core.Storage;
using Xunit;

namespace LibraryDesk.Tests;

public sealed class LibraryServiceSr3Tests
{
    private static readonly DateTimeOffset _fixedNow =
        new(2026, 10, 10, 9, 0, 0, TimeSpan.FromHours(3));
    private static readonly int[] _expectedSearchIds = [2, 3];
    private static readonly int[] _expectedReportIds = [1, 2];
    private static readonly int[] _expectedOverdueDays = [9, 2];

    [Fact]
    public void RegisterBook_NewIsbn_AddsBook()
    {
        (LibraryService service, InMemoryLibraryRepository repository, _) = CreateService();

        Result<Book> result = service.RegisterBook(NewBookRequest("ISBN-1", 2));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, repository.FindBook("ISBN-1")!.AvailableCopies);
    }

    [Fact]
    public void RegisterBook_ExistingIsbn_RestocksCopies()
    {
        (LibraryService service, InMemoryLibraryRepository repository, _) = CreateService();
        service.RegisterBook(NewBookRequest("ISBN-1", 2));

        Result<Book> result = service.RegisterBook(NewBookRequest("ISBN-1", 3));

        Assert.True(result.IsSuccess);
        Assert.Equal(5, repository.FindBook("ISBN-1")!.AvailableCopies);
    }

    [Theory]
    [InlineData("", "Назва", 10, 1000, 1)]
    [InlineData("ISBN", "", 10, 1000, 1)]
    [InlineData("ISBN", "Назва", -1, 1000, 1)]
    [InlineData("ISBN", "Назва", 10, 0, 1)]
    [InlineData("ISBN", "Назва", 10, 1000, 0)]
    public void RegisterBook_InvalidBoundary_ReturnsFailure(
        string isbn,
        string title,
        double dailyFee,
        double replacementPrice,
        int copies)
    {
        (LibraryService service, _, SpyScenarioLogger logger) = CreateService();
        RegisterBookRequest request = new()
        {
            Isbn = isbn,
            Title = title,
            DailyFee = (decimal)dailyFee,
            ReplacementPrice = (decimal)replacementPrice,
            Copies = copies,
        };

        Result<Book> result = service.RegisterBook(request);

        Assert.False(result.IsSuccess);
        Assert.Contains(logger.Entries, entry => entry.Level == ScenarioLogLevel.Warning);
    }

    [Fact]
    public void RegisterReader_ValidData_AddsReader()
    {
        (LibraryService service, InMemoryLibraryRepository repository, _) = CreateService();

        Result<Reader> result = service.RegisterReader(NewReaderRequest(1));

        Assert.True(result.IsSuccess);
        Assert.NotNull(repository.FindReader(1));
    }

    [Fact]
    public void RegisterReader_DuplicateId_ReturnsFailure()
    {
        (LibraryService service, _, _) = CreateService();
        service.RegisterReader(NewReaderRequest(1));

        Result<Reader> result = service.RegisterReader(NewReaderRequest(1));

        Assert.False(result.IsSuccess);
        Assert.Contains("існує", result.Error, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0, "Читач", "reader@example.com")]
    [InlineData(1, "", "reader@example.com")]
    [InlineData(1, "Читач", "invalid-email")]
    public void RegisterReader_InvalidBoundary_ReturnsFailure(int id, string name, string email)
    {
        (LibraryService service, _, _) = CreateService();
        RegisterReaderRequest request = new() { Id = id, FullName = name, Email = email };

        Result<Reader> result = service.RegisterReader(request);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void IssueBook_ValidRequest_CreatesActiveLoan()
    {
        (LibraryService service, InMemoryLibraryRepository repository, _) = CreateService();
        SeedBookAndReader(service);

        Result<IssueBookResult> result = service.IssueBook(NewIssueRequest());

        Assert.True(result.IsSuccess);
        Assert.Equal(new DateOnly(2026, 10, 24), result.Value!.DueOn);
        Assert.Equal(LoanStatus.Active, repository.FindLoan(100)!.Status);
        Assert.Equal(1, repository.FindBook("ISBN-1")!.AvailableCopies);
    }

    [Fact]
    public void IssueBook_InactiveReader_ReturnsFailure()
    {
        (LibraryService service, _, _) = CreateService();
        service.RegisterBook(NewBookRequest("ISBN-1", 2));
        RegisterReaderRequest reader = NewReaderRequest(1) with { HasActiveMembership = false };
        service.RegisterReader(reader);

        Result<IssueBookResult> result = service.IssueBook(NewIssueRequest());

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void IssueBook_MissingBookOrReader_ReturnsFailure()
    {
        (LibraryService service, _, _) = CreateService();

        Result<IssueBookResult> result = service.IssueBook(NewIssueRequest());

        Assert.False(result.IsSuccess);
        Assert.Contains("не знайдено", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void IssueBook_DuplicateLoanId_ReturnsFailure()
    {
        (LibraryService service, _, _) = CreateService();
        SeedBookAndReader(service);
        service.IssueBook(NewIssueRequest());

        Result<IssueBookResult> result = service.IssueBook(NewIssueRequest());

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void ReturnBook_OnTime_ReturnsCopyWithoutFee()
    {
        (LibraryService service, InMemoryLibraryRepository repository, _) = CreateService();
        SeedBookAndReader(service);
        service.IssueBook(NewIssueRequest());

        Result<ReturnBookResult> result = service.ReturnBook(
            new ReturnBookRequest { LoanId = 100, ReturnedOn = new DateOnly(2026, 10, 24) });

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.Value!.LateFee);
        Assert.Equal(2, repository.FindBook("ISBN-1")!.AvailableCopies);
        Assert.Equal(LoanStatus.Returned, repository.FindLoan(100)!.Status);
    }

    [Fact]
    public void ReturnBook_OverdueLoan_CalculatesFee()
    {
        (LibraryService service, _, _) = CreateService();
        SeedBookAndReader(service);
        service.IssueBook(NewIssueRequest());

        Result<ReturnBookResult> result = service.ReturnBook(
            new ReturnBookRequest { LoanId = 100, ReturnedOn = new DateOnly(2026, 11, 1) });

        Assert.True(result.IsSuccess);
        Assert.Equal(8, result.Value!.OverdueDays);
        Assert.Equal(120m, result.Value.LateFee);
    }

    [Fact]
    public void ReturnBook_DateBeforeIssue_ReturnsFailure()
    {
        (LibraryService service, _, _) = CreateService();
        SeedBookAndReader(service);
        service.IssueBook(NewIssueRequest());

        Result<ReturnBookResult> result = service.ReturnBook(
            new ReturnBookRequest { LoanId = 100, ReturnedOn = new DateOnly(2026, 10, 9) });

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void ReturnBook_AlreadyReturned_ThrowsDomainRuleException()
    {
        (LibraryService service, _, _) = CreateService();
        SeedBookAndReader(service);
        service.IssueBook(NewIssueRequest());
        ReturnBookRequest request = new() { LoanId = 100, ReturnedOn = new DateOnly(2026, 10, 24) };
        service.ReturnBook(request);

        DomainRuleException exception = Assert.Throws<DomainRuleException>(() => service.ReturnBook(request));

        Assert.Equal("loan.returnable", exception.Rule);
    }

    [Fact]
    public void SearchLoans_NoMatches_ReturnsEmpty()
    {
        (LibraryService service, _, _) = CreateService();

        IReadOnlyList<Loan> result = service.SearchLoans(404, LoanStatus.Active);

        Assert.Empty(result);
    }

    [Fact]
    public void SearchLoans_TwoCriteria_ReturnsOrderedMatches()
    {
        (LibraryService service, InMemoryLibraryRepository repository, _) = CreateService();
        repository.AddLoan(NewActiveLoan(3, 1, new DateOnly(2026, 11, 1)));
        repository.AddLoan(NewActiveLoan(2, 1, new DateOnly(2026, 10, 20)));
        repository.AddLoan(NewActiveLoan(1, 2, new DateOnly(2026, 10, 15)));

        IReadOnlyList<Loan> result = service.SearchLoans(1, LoanStatus.Active);

        Assert.Equal(_expectedSearchIds, result.Select(loan => loan.Id));
    }

    [Fact]
    public void BuildDebtorReport_NoDebtors_ReturnsEmpty()
    {
        (LibraryService service, _, _) = CreateService();

        IReadOnlyList<DebtorReportRow> result = service.BuildDebtorReport(new DateOnly(2026, 10, 10));

        Assert.Empty(result);
    }

    [Fact]
    public void BuildDebtorReport_MultipleLoans_OrdersByOverdueDays()
    {
        (LibraryService service, InMemoryLibraryRepository repository, _) = CreateService();
        repository.AddReader(new Reader(1, "Читач 1", "reader1@example.com", true));
        repository.AddReader(new Reader(2, "Читач 2", "reader2@example.com", true));
        repository.AddLoan(NewActiveLoan(2, 2, new DateOnly(2026, 10, 8)));
        repository.AddLoan(NewActiveLoan(1, 1, new DateOnly(2026, 10, 1)));

        IReadOnlyList<DebtorReportRow> result = service.BuildDebtorReport(new DateOnly(2026, 10, 10));

        Assert.Equal(_expectedReportIds, result.Select(row => row.LoanId));
        Assert.Equal(_expectedOverdueDays, result.Select(row => row.OverdueDays));
    }

    [Fact]
    public void BuildDebtorReport_MissingReader_SkipsRowAndLogsError()
    {
        (LibraryService service, InMemoryLibraryRepository repository, SpyScenarioLogger logger) = CreateService();
        repository.AddLoan(NewActiveLoan(1, 99, new DateOnly(2026, 10, 1)));

        IReadOnlyList<DebtorReportRow> result = service.BuildDebtorReport(new DateOnly(2026, 10, 10));

        Assert.Empty(result);
        Assert.Contains(logger.Entries, entry => entry.Level == ScenarioLogLevel.Error);
    }

    [Fact]
    public void RegisterBook_SaveFailure_LogsErrorAndRethrows()
    {
        ThrowingLibraryRepository repository = new();
        SpyScenarioLogger logger = new();
        LibraryService service = new(repository, new FixedClock(_fixedNow), new LateFeePolicy(), logger);

        IOException exception = Assert.Throws<IOException>(() => service.RegisterBook(NewBookRequest("ISBN-1", 1)));

        Assert.Equal("Тестовий збій сховища.", exception.Message);
        Assert.Contains(logger.Entries, entry => entry.Level == ScenarioLogLevel.Error);
    }

    private static (LibraryService Service, InMemoryLibraryRepository Repository, SpyScenarioLogger Logger) CreateService()
    {
        InMemoryLibraryRepository repository = new();
        SpyScenarioLogger logger = new();
        LibraryService service = new(repository, new FixedClock(_fixedNow), new LateFeePolicy(), logger);
        return (service, repository, logger);
    }

    private static void SeedBookAndReader(LibraryService service)
    {
        service.RegisterBook(NewBookRequest("ISBN-1", 2));
        service.RegisterReader(NewReaderRequest(1));
    }

    private static RegisterBookRequest NewBookRequest(string isbn, int copies)
        => new()
        {
            Isbn = isbn,
            Title = "Чистий код",
            DailyFee = 10m,
            ReplacementPrice = 1000m,
            Copies = copies,
        };

    private static RegisterReaderRequest NewReaderRequest(int id)
        => new()
        {
            Id = id,
            FullName = $"Читач {id}",
            Email = $"reader{id}@example.com",
        };

    private static IssueBookRequest NewIssueRequest()
        => new() { LoanId = 100, ReaderId = 1, Isbn = "ISBN-1" };

    private static Loan NewActiveLoan(int id, int readerId, DateOnly dueOn)
    {
        DateOnly issuedOn = dueOn.AddDays(-LoanTermsPolicy.StandardLoanDays);
        DateTimeOffset issuedAt = new(issuedOn, new TimeOnly(9, 0), TimeSpan.FromHours(3));
        Loan loan = new(id, readerId, issuedAt, dueOn);
        loan.AddItem(new LoanItem($"ISBN-{id}", LoanTermsPolicy.StandardLoanDays, 10m, 1000m));
        loan.Issue();
        return loan;
    }
}
