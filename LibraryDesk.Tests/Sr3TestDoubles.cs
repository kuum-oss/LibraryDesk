using LibraryDesk.Core.Abstractions;
using LibraryDesk.Core.Domain;
using LibraryDesk.Core.Storage;

namespace LibraryDesk.Tests;

internal sealed class FixedClock : IClock
{
    public FixedClock(DateTimeOffset now) => Now = now;

    public DateTimeOffset Now { get; }
}

internal sealed class SpyScenarioLogger : IScenarioLogger
{
    public List<(ScenarioLogLevel Level, string Scenario, string Message)> Entries { get; } = new();

    public void Write(ScenarioLogLevel level, string scenario, string message)
        => Entries.Add((level, scenario, message));
}

internal sealed class ThrowingLibraryRepository : ILibraryRepository
{
    private readonly InMemoryLibraryRepository _inner = new();

    public Book? FindBook(string isbn) => _inner.FindBook(isbn);

    public Reader? FindReader(int id) => _inner.FindReader(id);

    public Loan? FindLoan(int id) => _inner.FindLoan(id);

    public IReadOnlyList<Book> GetBooks() => _inner.GetBooks();

    public IReadOnlyList<Reader> GetReaders() => _inner.GetReaders();

    public IReadOnlyList<Loan> GetLoans() => _inner.GetLoans();

    public void AddBook(Book book) => _inner.AddBook(book);

    public void AddReader(Reader reader) => _inner.AddReader(reader);

    public void AddLoan(Loan loan) => _inner.AddLoan(loan);

    public void Save() => throw new IOException("Тестовий збій сховища.");
}
