using LibraryDesk.Core.Abstractions;
using LibraryDesk.Core.Domain;
using LibraryDesk.Core.Errors;

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
            throw new DomainRuleException(
                "loan.registerable",
                $"Формуляр {loan.Id} має стан {loan.Status}; реєструвати можна лише стан Active.");
        }

        if (loan.ReaderId != reader.Id)
        {
            throw new DomainRuleException(
                "loan.reader",
                $"Формуляр {loan.Id} належить читачеві {loan.ReaderId}, а передано читача {reader.Id}.");
        }

        _repository.Add(loan);
        _notifier.Notify(reader, loan, TotalOf(loan));
    }

    /// <summary>Повертає формуляр або повідомляє про порушення правила існування.</summary>
    /// <param name="loanId">Ідентифікатор формуляра.</param>
    /// <returns>Знайдений формуляр.</returns>
    /// <exception cref="DomainRuleException">Формуляр не знайдено.</exception>
    public Loan GetRequiredLoan(int loanId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(loanId);
        return _repository.GetById(loanId)
            ?? throw new DomainRuleException(
                "loan.exists",
                $"Формуляр {loanId} не знайдено.");
    }

    /// <summary>Додає типізовану позицію до формуляра.</summary>
    /// <param name="loanId">Ідентифікатор формуляра.</param>
    /// <param name="item">Перевірена позиція формуляра.</param>
    public void AddItem(int loanId, LoanItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        Loan loan = GetRequiredLoan(loanId);
        loan.AddItem(item);
    }

    /// <summary>Скасовує чернетку формуляра із зазначеною причиною.</summary>
    /// <param name="loanId">Ідентифікатор формуляра.</param>
    /// <param name="reason">Причина скасування для контексту операції.</param>
    public void Cancel(int loanId, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        Loan loan = GetRequiredLoan(loanId);
        loan.Cancel();
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

        if (total < 0m)
        {
            throw new DomainRuleException(
                "loan.total.nonnegative",
                $"Вартість формуляра {loan.Id} не може бути від'ємною: {total}.");
        }

        return total;
    }
}
