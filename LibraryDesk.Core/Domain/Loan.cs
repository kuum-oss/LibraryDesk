namespace LibraryDesk.Core.Domain;

using LibraryDesk.Core.Errors;

/// <summary>Формуляр видачі книг читачеві.</summary>
public sealed class Loan
{
    private readonly List<LoanItem> _items = new();

    /// <summary>Створює чернетку формуляра.</summary>
    /// <param name="id">Ідентифікатор формуляра.</param>
    /// <param name="readerId">Ідентифікатор читача.</param>
    /// <param name="issuedAt">Дата й час оформлення.</param>
    /// <param name="dueOn">Останній день повернення.</param>
    public Loan(int id, int readerId, DateTimeOffset issuedAt, DateOnly dueOn)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(readerId);
        ArgumentOutOfRangeException.ThrowIfLessThan(dueOn, DateOnly.FromDateTime(issuedAt.Date));

        Id = id;
        ReaderId = readerId;
        IssuedAt = issuedAt;
        DueOn = dueOn;
        Status = LoanStatus.Draft;
    }

    /// <summary>Унікальний ідентифікатор видачі.</summary>
    public int Id { get; }

    /// <summary>Ідентифікатор читача.</summary>
    public int ReaderId { get; }

    /// <summary>Поточний статус видачі.</summary>
    public LoanStatus Status { get; private set; }

    /// <summary>Дата та час видачі.</summary>
    public DateTimeOffset IssuedAt { get; }

    /// <summary>Останній день повернення книг.</summary>
    public DateOnly DueOn { get; }

    /// <summary>Список позицій у формулярі видачі.</summary>
    public IReadOnlyList<LoanItem> Items => _items.AsReadOnly();

    /// <summary>Додає позицію до видачі.</summary>
    /// <param name="item">Позиція формуляра видачі.</param>
    public void AddItem(LoanItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureStatus(LoanStatus.Draft);
        _items.Add(item);
    }

    /// <summary>Видає книги читачеві після заповнення формуляра.</summary>
    public void Issue()
    {
        EnsureStatus(LoanStatus.Draft);
        if (_items.Count == 0)
        {
            throw new DomainRuleException(
                "loan.not-empty",
                $"Формуляр {Id} не можна видати без жодної позиції.");
        }

        Status = LoanStatus.Active;
    }

    /// <summary>Скасовує ще не виданий формуляр.</summary>
    public void Cancel()
    {
        EnsureStatus(LoanStatus.Draft);
        Status = LoanStatus.Cancelled;
    }

    /// <summary>Позначає активну видачу як прострочену після терміну повернення.</summary>
    /// <param name="today">Поточна дата.</param>
    public void MarkOverdue(DateOnly today)
    {
        EnsureStatus(LoanStatus.Active);
        if (today <= DueOn)
        {
            throw new DomainRuleException(
                "loan.overdue-date",
                $"Формуляр {Id} має строк {DueOn:yyyy-MM-dd}; дата {today:yyyy-MM-dd} ще не є простроченням.");
        }

        Status = LoanStatus.Overdue;
    }

    /// <summary>Фіксує повернення виданих книг.</summary>
    public void Return()
    {
        if (Status is not (LoanStatus.Active or LoanStatus.Overdue))
        {
            throw new DomainRuleException(
                "loan.returnable",
                $"Формуляр {Id} у стані {Status} не можна повернути.");
        }

        Status = LoanStatus.Returned;
    }

    /// <summary>Фіксує втрату виданих книг.</summary>
    public void MarkLost()
    {
        if (Status is not (LoanStatus.Active or LoanStatus.Overdue))
        {
            throw new DomainRuleException(
                "loan.loss-recordable",
                $"Формуляр {Id} у стані {Status} не дозволяє зафіксувати втрату.");
        }

        Status = LoanStatus.Lost;
    }

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

    /// <summary>Відновлює перевірений стан формуляра з інфраструктурного знімка.</summary>
    /// <param name="id">Ідентифікатор формуляра.</param>
    /// <param name="readerId">Ідентифікатор читача.</param>
    /// <param name="issuedAt">Дата й час видачі.</param>
    /// <param name="dueOn">Останній день повернення.</param>
    /// <param name="status">Збережений стан.</param>
    /// <param name="items">Збережені позиції.</param>
    /// <returns>Відновлений формуляр.</returns>
    internal static Loan Restore(
        int id,
        int readerId,
        DateTimeOffset issuedAt,
        DateOnly dueOn,
        LoanStatus status,
        IEnumerable<LoanItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        Loan loan = new(id, readerId, issuedAt, dueOn);
        loan._items.AddRange(items);
        loan.Status = status;
        return loan;
    }

    private void EnsureStatus(LoanStatus required)
    {
        if (Status != required)
        {
            throw new DomainRuleException(
                "loan.status",
                $"Формуляр {Id} має стан {Status}; операція дозволена лише у стані {required}.");
        }
    }
}
