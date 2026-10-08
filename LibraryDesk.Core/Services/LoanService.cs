using LibraryDesk.Core.Abstractions;
using LibraryDesk.Core.Domain;

namespace LibraryDesk.Core.Services;

/// <summary>Сценарії реєстрації формуляра й обчислення його вартості.</summary>
public sealed class LoanService
{
    private readonly ILoanRepository _repository;
    private readonly IPricingPolicy _pricing;
    private readonly INotifier _notifier;

    /// <summary>Створює сервіс із підставленими залежностями.</summary>
    /// <param name="repository">Сховище формулярів.</param>
    /// <param name="pricing">Політика ціни.</param>
    /// <param name="notifier">Сповіщувач читача.</param>
    public LoanService(ILoanRepository repository, IPricingPolicy pricing, INotifier notifier)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
    }

    /// <summary>Реєструє виданий формуляр.</summary>
    /// <param name="loan">Формуляр у стані Active.</param>
    /// <param name="reader">Читач, якому видано книги.</param>
    public void Register(Loan loan, Reader reader)
    {
        ArgumentNullException.ThrowIfNull(loan);
        ArgumentNullException.ThrowIfNull(reader);
        if (loan.Status != LoanStatus.Active)
        {
            throw new InvalidOperationException("Реєструють лише виданий формуляр.");
        }

        if (loan.ReaderId != reader.Id)
        {
            throw new ArgumentException("Читач не відповідає формуляру.", nameof(reader));
        }

        _repository.Add(loan);
        _notifier.Notify(reader, loan, TotalOf(loan));
    }

    /// <summary>Обчислює вартість формуляра за поточною політикою.</summary>
    /// <param name="loan">Формуляр для розрахунку.</param>
    /// <returns>Загальна вартість прокату.</returns>
    public decimal TotalOf(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);
        decimal total = 0m;
        foreach (LoanItem item in loan.Items)
        {
            total += _pricing.PriceOf(item);
        }

        return total;
    }
}
