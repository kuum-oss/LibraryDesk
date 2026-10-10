namespace LibraryDesk.Core.Domain;

/// <summary>Книга у фонді бібліотеки.</summary>
public sealed class Book
{
    /// <summary>Типова вартість заміни примірника.</summary>
    public const decimal DefaultReplacementPrice = 1000m;

    /// <summary>Створює запис книги в каталозі.</summary>
    /// <param name="isbn">Ідентифікатор книги.</param>
    /// <param name="title">Назва книги.</param>
    /// <param name="rentalFee">Денний тариф.</param>
    /// <param name="availableCopies">Кількість доступних примірників.</param>
    /// <param name="replacementPrice">Вартість заміни втраченого примірника.</param>
    public Book(
        string isbn,
        string title,
        decimal rentalFee,
        int availableCopies,
        decimal replacementPrice = DefaultReplacementPrice)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(isbn);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentOutOfRangeException.ThrowIfNegative(rentalFee);
        ArgumentOutOfRangeException.ThrowIfNegative(availableCopies);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(replacementPrice);

        Isbn = isbn;
        Title = title;
        RentalFee = rentalFee;
        AvailableCopies = availableCopies;
        ReplacementPrice = replacementPrice;
    }

    /// <summary>Міжнародний стандартний номер книги.</summary>
    public string Isbn { get; }

    /// <summary>Назва книги.</summary>
    public string Title { get; }

    /// <summary>Вартість оренди книги.</summary>
    public decimal RentalFee { get; }

    /// <summary>Кількість доступних примірників.</summary>
    public int AvailableCopies { get; private set; }

    /// <summary>Вартість заміни примірника, що обмежує суму пені.</summary>
    public decimal ReplacementPrice { get; }

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

    /// <summary>Повертає один примірник до доступного фонду.</summary>
    public void ReturnCopy() => AvailableCopies = checked(AvailableCopies + 1);
}
