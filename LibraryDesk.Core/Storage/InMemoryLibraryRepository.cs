using LibraryDesk.Core.Abstractions;
using LibraryDesk.Core.Domain;

namespace LibraryDesk.Core.Storage;

/// <summary>Зберігає повний стан бібліотеки у пам'яті процесу.</summary>
public sealed class InMemoryLibraryRepository : ILibraryRepository
{
    private readonly Dictionary<string, Book> _books = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<int, Reader> _readers = new();
    private readonly Dictionary<int, Loan> _loans = new();

    /// <inheritdoc />
    public Book? FindBook(string isbn)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(isbn);
        return _books.GetValueOrDefault(isbn);
    }

    /// <inheritdoc />
    public Reader? FindReader(int id)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        return _readers.GetValueOrDefault(id);
    }

    /// <inheritdoc />
    public Loan? FindLoan(int id)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        return _loans.GetValueOrDefault(id);
    }

    /// <inheritdoc />
    public IReadOnlyList<Book> GetBooks() => Array.AsReadOnly(_books.Values.ToArray());

    /// <inheritdoc />
    public IReadOnlyList<Reader> GetReaders() => Array.AsReadOnly(_readers.Values.ToArray());

    /// <inheritdoc />
    public IReadOnlyList<Loan> GetLoans() => Array.AsReadOnly(_loans.Values.ToArray());

    /// <inheritdoc />
    public void AddBook(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);
        AddUnique(_books, book.Isbn, book, "Книга з таким ISBN уже існує.");
    }

    /// <inheritdoc />
    public void AddReader(Reader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        AddUnique(_readers, reader.Id, reader, "Читач із таким ідентифікатором уже існує.");
    }

    /// <inheritdoc />
    public void AddLoan(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);
        AddUnique(_loans, loan.Id, loan, "Формуляр із таким ідентифікатором уже існує.");
    }

    /// <inheritdoc />
    public void Save()
    {
    }

    private static void AddUnique<TKey, TValue>(
        IDictionary<TKey, TValue> target,
        TKey key,
        TValue value,
        string duplicateMessage)
    {
        if (!target.TryAdd(key, value))
        {
            throw new InvalidOperationException(duplicateMessage);
        }
    }
}
