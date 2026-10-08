namespace LibraryDesk.Core.Domain;

/// <summary>Книга у фонді бібліотеки.</summary>
public sealed class Book
{
    /// <summary>Створює запис книги в каталозі.</summary>
    /// <param name="isbn">Ідентифікатор книги.</param>
    /// <param name="title">Назва книги.</param>
    /// <param name="rentalFee">Денний тариф.</param>
    /// <param name="availableCopies">Кількість доступних примірників.</param>
    public Book(string isbn, string title, decimal rentalFee, int availableCopies)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(isbn);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentOutOfRangeException.ThrowIfNegative(rentalFee);
        ArgumentOutOfRangeException.ThrowIfNegative(availableCopies);

        Isbn = isbn;
        Title = title;
        RentalFee = rentalFee;
        AvailableCopies = availableCopies;
    }

    /// <summary>Міжнародний стандартний номер книги.</summary>
    public string Isbn { get; }

    /// <summary>Назва книги.</summary>
    public string Title { get; }

    /// <summary>Вартість оренди книги.</summary>
    public decimal RentalFee { get; }

    /// <summary>Кількість доступних примірників.</summary>
    public int AvailableCopies { get; private set; }

    /// <summary>Реєструє надходження примірників.</summary>
    /// <param name="count">Кількість нових примірників.</param>
    public void Restock(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
        AvailableCopies = checked(AvailableCopies + count);
    }

    /// <summary>Видає один доступний примірник.</summary>
    public void LendCopy()
    {
        if (AvailableCopies == 0)
        {
            throw new InvalidOperationException("Доступних примірників немає.");
        }

        AvailableCopies--;
    }
}
