namespace LibraryDesk.Core.Domain;

/// <summary>Формуляр видачі книг читачеві.</summary>
public sealed class Loan
{
    private readonly List<LoanItem> _items = new();

    /// <summary>Унікальний ідентифікатор видачі.</summary>
    public int Id { get; init; }

    /// <summary>Ідентифікатор читача.</summary>
    public int ReaderId { get; init; }

    /// <summary>Поточний статус видачі.</summary>
    public LoanStatus Status { get; set; } = LoanStatus.Active;

    /// <summary>Дата та час видачі.</summary>
    public DateTimeOffset IssuedAt { get; init; }

    /// <summary>Список позицій у формулярі видачі.</summary>
    public IReadOnlyList<LoanItem> Items => _items;

    /// <summary>Додає позицію до видачі.</summary>
    /// <param name="item">Позиція формуляра видачі.</param>
    public void AddItem(LoanItem item) => _items.Add(item);

    /// <summary>Обчислює загальну вартість прокату книг у формулярі.</summary>
    /// <returns>Загальна вартість прокату у грошових одиницях.</returns>
    public decimal Total()
    {
        decimal sum = 0m;
        foreach (LoanItem item in _items)
        {
            sum += item.Amount;
        }

        return sum;
    }
}
