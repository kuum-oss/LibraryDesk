using LibraryDesk.Core.Domain;

namespace LibraryDesk.Core.Abstractions;

/// <summary>Порт зберігання каталогу, читачів і формулярів.</summary>
public interface ILibraryRepository
{
    /// <summary>Шукає книгу за ISBN.</summary>
    /// <param name="isbn">ISBN без порожніх символів.</param>
    /// <returns>Книга або <see langword="null"/>.</returns>
    Book? FindBook(string isbn);

    /// <summary>Шукає читача за ідентифікатором.</summary>
    /// <param name="id">Додатний ідентифікатор читача.</param>
    /// <returns>Читач або <see langword="null"/>.</returns>
    Reader? FindReader(int id);

    /// <summary>Шукає формуляр за ідентифікатором.</summary>
    /// <param name="id">Додатний ідентифікатор формуляра.</param>
    /// <returns>Формуляр або <see langword="null"/>.</returns>
    Loan? FindLoan(int id);

    /// <summary>Повертає знімок каталогу.</summary>
    /// <returns>Незмінний список книг.</returns>
    IReadOnlyList<Book> GetBooks();

    /// <summary>Повертає знімок читачів.</summary>
    /// <returns>Незмінний список читачів.</returns>
    IReadOnlyList<Reader> GetReaders();

    /// <summary>Повертає знімок формулярів.</summary>
    /// <returns>Незмінний список формулярів.</returns>
    IReadOnlyList<Loan> GetLoans();

    /// <summary>Додає нову книгу.</summary>
    /// <param name="book">Перевірена книга з унікальним ISBN.</param>
    void AddBook(Book book);

    /// <summary>Додає нового читача.</summary>
    /// <param name="reader">Перевірений читач з унікальним ідентифікатором.</param>
    void AddReader(Reader reader);

    /// <summary>Додає новий формуляр.</summary>
    /// <param name="loan">Перевірений формуляр з унікальним ідентифікатором.</param>
    void AddLoan(Loan loan);

    /// <summary>Зберігає поточний узгоджений стан.</summary>
    void Save();
}
