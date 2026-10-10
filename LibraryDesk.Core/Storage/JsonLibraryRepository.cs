using System.Text.Json;
using LibraryDesk.Core.Abstractions;
using LibraryDesk.Core.Domain;

namespace LibraryDesk.Core.Storage;

/// <summary>Зберігає стан LibraryDesk в одному читабельному JSON-файлі.</summary>
public sealed class JsonLibraryRepository : ILibraryRepository
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    private readonly string _path;
    private readonly Dictionary<string, Book> _books = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<int, Reader> _readers = new();
    private readonly Dictionary<int, Loan> _loans = new();

    /// <summary>Відкриває сховище та завантажує наявний файл.</summary>
    /// <param name="path">Шлях до JSON-файла; відсутній файл означає порожнє сховище.</param>
    public JsonLibraryRepository(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = path;
        Load();
    }

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
        string? directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        LibraryFile file = CreateFile();
        string json = JsonSerializer.Serialize(file, _jsonOptions);
        string temporaryPath = _path + ".tmp";
        File.WriteAllText(temporaryPath, json);
        File.Move(temporaryPath, _path, overwrite: true);
    }

    private void Load()
    {
        if (!File.Exists(_path))
        {
            return;
        }

        try
        {
            LibraryFile file = JsonSerializer.Deserialize<LibraryFile>(File.ReadAllText(_path), _jsonOptions)
                ?? throw new InvalidDataException("JSON-файл не містить стану бібліотеки.");
            Restore(file);
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("JSON-файл бібліотеки пошкоджено.", exception);
        }
    }

    private LibraryFile CreateFile()
        => new()
        {
            Books = _books.Values.Select(BookData.FromDomain).OrderBy(book => book.Isbn).ToArray(),
            Readers = _readers.Values.Select(ReaderData.FromDomain).OrderBy(reader => reader.Id).ToArray(),
            Loans = _loans.Values.Select(LoanData.FromDomain).OrderBy(loan => loan.Id).ToArray(),
        };

    private void Restore(LibraryFile file)
    {
        foreach (BookData book in file.Books)
        {
            AddBook(book.ToDomain());
        }

        foreach (ReaderData reader in file.Readers)
        {
            AddReader(reader.ToDomain());
        }

        foreach (LoanData loan in file.Loans)
        {
            AddLoan(loan.ToDomain());
        }
    }

    private static void AddUnique<TKey, TValue>(
        IDictionary<TKey, TValue> target,
        TKey key,
        TValue value,
        string duplicateMessage)
    {
        if (!target.TryAdd(key, value))
        {
            throw new InvalidDataException(duplicateMessage);
        }
    }

    private sealed record LibraryFile
    {
        public IReadOnlyList<BookData> Books { get; init; } = Array.Empty<BookData>();

        public IReadOnlyList<ReaderData> Readers { get; init; } = Array.Empty<ReaderData>();

        public IReadOnlyList<LoanData> Loans { get; init; } = Array.Empty<LoanData>();
    }

    private sealed record BookData
    {
        public required string Isbn { get; init; }

        public required string Title { get; init; }

        public decimal RentalFee { get; init; }

        public int AvailableCopies { get; init; }

        public decimal ReplacementPrice { get; init; }

        public static BookData FromDomain(Book book)
            => new()
            {
                Isbn = book.Isbn,
                Title = book.Title,
                RentalFee = book.RentalFee,
                AvailableCopies = book.AvailableCopies,
                ReplacementPrice = book.ReplacementPrice,
            };

        public Book ToDomain() => new(Isbn, Title, RentalFee, AvailableCopies, ReplacementPrice);
    }

    private sealed record ReaderData
    {
        public int Id { get; init; }

        public required string FullName { get; init; }

        public required string Email { get; init; }

        public bool HasActiveMembership { get; init; }

        public static ReaderData FromDomain(Reader reader)
            => new()
            {
                Id = reader.Id,
                FullName = reader.FullName,
                Email = reader.Email,
                HasActiveMembership = reader.HasActiveMembership,
            };

        public Reader ToDomain() => new(Id, FullName, Email, HasActiveMembership);
    }

    private sealed record LoanData
    {
        public int Id { get; init; }

        public int ReaderId { get; init; }

        public DateTimeOffset IssuedAt { get; init; }

        public DateOnly DueOn { get; init; }

        public LoanStatus Status { get; init; }

        public IReadOnlyList<LoanItemData> Items { get; init; } = Array.Empty<LoanItemData>();

        public static LoanData FromDomain(Loan loan)
            => new()
            {
                Id = loan.Id,
                ReaderId = loan.ReaderId,
                IssuedAt = loan.IssuedAt,
                DueOn = loan.DueOn,
                Status = loan.Status,
                Items = loan.Items.Select(LoanItemData.FromDomain).ToArray(),
            };

        public Loan ToDomain()
            => Loan.Restore(
                new LoanRestoreData
                {
                    Id = Id,
                    ReaderId = ReaderId,
                    IssuedAt = IssuedAt,
                    DueOn = DueOn,
                    Status = Status,
                    Items = Items.Select(item => item.ToDomain()),
                });
    }

    private sealed record LoanItemData
    {
        public required string Isbn { get; init; }

        public int Days { get; init; }

        public decimal DailyRate { get; init; }

        public decimal ReplacementPrice { get; init; }

        public static LoanItemData FromDomain(LoanItem item)
            => new()
            {
                Isbn = item.Isbn,
                Days = item.Days,
                DailyRate = item.DailyRate,
                ReplacementPrice = item.ReplacementPrice,
            };

        public LoanItem ToDomain() => new(Isbn, Days, DailyRate, ReplacementPrice);
    }
}
