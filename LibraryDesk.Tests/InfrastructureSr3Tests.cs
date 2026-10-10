using LibraryDesk.Core.Domain;
using LibraryDesk.Core.Storage;
using Xunit;

namespace LibraryDesk.Tests;

public sealed class InfrastructureSr3Tests
{
    private static readonly DateTimeOffset _fixedNow =
        new(2026, 10, 10, 12, 30, 0, TimeSpan.FromHours(3));

    [Fact]
    public void JsonRepository_SaveAndReload_PreservesState()
    {
        string path = NewTemporaryPath("library.json");
        try
        {
            JsonLibraryRepository repository = new(path);
            Book book = new("ISBN-JSON", "Архітектура", 12.5m, 2, 900m);
            Reader reader = new(7, "Тестовий читач", "reader7@example.com", true);
            Loan loan = NewActiveLoan(reader.Id, book);
            repository.AddBook(book);
            repository.AddReader(reader);
            repository.AddLoan(loan);
            repository.Save();

            JsonLibraryRepository reloaded = new(path);

            Assert.Equal("Архітектура", reloaded.FindBook("ISBN-JSON")!.Title);
            Assert.Equal("reader7@example.com", reloaded.FindReader(7)!.Email);
            Assert.Equal(LoanStatus.Active, reloaded.FindLoan(77)!.Status);
            Assert.Equal(900m, reloaded.FindLoan(77)!.Items[0].ReplacementPrice);
        }
        finally
        {
            DeleteTemporaryDirectory(path);
        }
    }

    [Fact]
    public void JsonRepository_InvalidJson_ThrowsInvalidData()
    {
        string path = NewTemporaryPath("broken.json");
        try
        {
            File.WriteAllText(path, "{ broken json }");

            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => new JsonLibraryRepository(path));

            Assert.NotNull(exception.InnerException);
        }
        finally
        {
            DeleteTemporaryDirectory(path);
        }
    }

    [Fact]
    public void FileScenarioLogger_Write_UsesClockAndSingleLine()
    {
        string path = NewTemporaryPath("librarydesk.log");
        try
        {
            FileScenarioLogger logger = new(path, new FixedClock(_fixedNow));

            logger.Write(ScenarioLogLevel.Warning, "UC-03", "Відхилено\nзапит.");

            string line = File.ReadAllText(path);
            Assert.Contains("2026-10-10T12:30:00.0000000+03:00;Warning;UC-03;", line, StringComparison.Ordinal);
            Assert.DoesNotContain("Відхилено\nзапит", line, StringComparison.Ordinal);
        }
        finally
        {
            DeleteTemporaryDirectory(path);
        }
    }

    private static Loan NewActiveLoan(int readerId, Book book)
    {
        Loan loan = new(77, readerId, _fixedNow, new DateOnly(2026, 10, 24));
        loan.AddItem(new LoanItem(book.Isbn, 14, book.RentalFee, book.ReplacementPrice));
        loan.Issue();
        return loan;
    }

    private static string NewTemporaryPath(string fileName)
    {
        string directory = Path.Combine(Path.GetTempPath(), $"librarydesk-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, fileName);
    }

    private static void DeleteTemporaryDirectory(string filePath)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (directory is not null && Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
