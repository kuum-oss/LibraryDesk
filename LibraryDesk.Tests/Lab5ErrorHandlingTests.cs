using LibraryDesk.Core.Abstractions;
using LibraryDesk.Core.Domain;
using LibraryDesk.Core.Errors;
using LibraryDesk.Core.Pricing;
using LibraryDesk.Core.Services;
using LibraryDesk.Core.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LibraryDesk.Tests;

public sealed class Lab5ErrorHandlingTests
{
    private static readonly string[] _mixedDayRows = { "2", "два", "0", "7" };

    [Fact]
    public void GetRequiredLoan_WhenMissing_ThrowsRuleWithIdentifier()
    {
        LoanService service = NewService(new InMemoryLoanRepository());

        DomainRuleException exception = Assert.Throws<DomainRuleException>(
            () => service.GetRequiredLoan(404));

        Assert.Equal("loan.exists", exception.Rule);
        Assert.Contains("404", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CountValidDays_WithMixedRows_ReturnsOnlyPositiveIntegers()
    {
        int actual = LoanErrorHandling.CountValidDays(_mixedDayRows);

        Assert.Equal(2, actual);
    }

    [Fact]
    public void FileAuditLog_AfterException_ReleasesOwnedFile()
    {
        string path = Path.Combine(Path.GetTempPath(), $"librarydesk-{Guid.NewGuid():N}.log");
        try
        {
            Assert.Throws<InvalidOperationException>(() => WriteAndFail(path));

            using FileStream reopened = new(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            Assert.True(reopened.Length > 0);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static LoanService NewService(ILoanRepository repository)
        => new(
            repository,
            new StandardPricingPolicy(),
            new TestNotifier(),
            NullLogger<LoanService>.Instance);

    private static void WriteAndFail(string path)
    {
        using FileAuditLog audit = new(path);
        audit.Write("test.failure", 1001);
        throw new InvalidOperationException("Перевірка звільнення ресурсу.");
    }

    private sealed class TestNotifier : INotifier
    {
        public void Notify(Reader reader, Loan loan, decimal total)
        {
        }
    }
}
