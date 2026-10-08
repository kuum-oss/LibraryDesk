using LibraryDesk.Core.Abstractions;
using LibraryDesk.Core.Domain;

namespace LibraryDesk.Core.Services;

/// <summary>Сценарії реєстрації формуляра й обчислення його вартості.</summary>
public sealed class LoanService
{
    private readonly ILoanRepository _repository;
    private readonly IPricingPolicy _pricing;

    /// <summary>Створює сервіс із підставленими залежностями.</summary>
    /// <param name="repository">Сховище формулярів.</param>
    /// <param name="pricing">Політика ціни.</param>
    public LoanService(ILoanRepository repository, IPricingPolicy pricing)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
    }

    /// <summary>Реєструє виданий формуляр.</summary>
    /// <param name="loan">Формуляр у стані Active.</param>
    public void Register(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);
        if (loan.Status != LoanStatus.Active)
        {
            throw new InvalidOperationException("Реєструють лише виданий формуляр.");
        }

        _repository.Add(loan);
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
