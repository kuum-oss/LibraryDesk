using LibraryDesk.Core.Abstractions;
using LibraryDesk.Core.Domain;
using LibraryDesk.Core.Errors;
using Microsoft.Extensions.Logging;

namespace LibraryDesk.Core.Services;

/// <summary>Сценарії реєстрації формуляра й обчислення його вартості.</summary>
public sealed class LoanService
{
    private readonly ILoanRepository _repository;
    private readonly IPricingPolicy _pricing;
    private readonly INotifier _notifier;
    private readonly ILogger<LoanService> _logger;

    /// <summary>Створює сервіс із підставленими залежностями.</summary>
    /// <param name="repository">Сховище формулярів.</param>
    /// <param name="pricing">Політика ціни.</param>
    /// <param name="notifier">Сповіщувач читача.</param>
    /// <param name="logger">Структурований журнал сервісу.</param>
    public LoanService(
        ILoanRepository repository,
        IPricingPolicy pricing,
        INotifier notifier,
        ILogger<LoanService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Реєструє виданий формуляр.</summary>
    /// <param name="loan">Формуляр у стані Active.</param>
    /// <param name="reader">Читач, якому видано книги.</param>
    public void Register(Loan loan, Reader reader)
    {
        ArgumentNullException.ThrowIfNull(loan);
        ArgumentNullException.ThrowIfNull(reader);
        using IDisposable? scope = LoanServiceLog.BeginLoanScope(_logger, loan.Id);
        decimal total = TotalOf(loan);
        LoanServiceLog.RegisterStarting(_logger, loan.Id, total);
        if (loan.Status != LoanStatus.Active)
        {
            LoanServiceLog.RuleViolation(_logger, loan.Id, "loan.registerable", loan.Status);
            throw new DomainRuleException(
                "loan.registerable",
                $"Формуляр {loan.Id} має стан {loan.Status}; реєструвати можна лише стан Active.");
        }

        if (loan.ReaderId != reader.Id)
        {
            LoanServiceLog.RuleViolation(_logger, loan.Id, "loan.reader", loan.Status);
            throw new DomainRuleException(
                "loan.reader",
                $"Формуляр {loan.Id} належить читачеві {loan.ReaderId}, а передано читача {reader.Id}.");
        }

        if (total == 0m)
        {
            LoanServiceLog.ZeroTotal(_logger, loan.Id);
        }

        try
        {
            _repository.Add(loan);
        }
        catch (IOException exception)
        {
            LoanServiceLog.StorageFailure(_logger, exception, loan.Id);
            throw;
        }

        _notifier.Notify(reader, loan, total);
        LoanServiceLog.Registered(_logger, loan.Id, total);
    }

    /// <summary>Повертає формуляр або повідомляє про порушення правила існування.</summary>
    /// <param name="loanId">Ідентифікатор формуляра.</param>
    /// <returns>Знайдений формуляр.</returns>
    /// <exception cref="DomainRuleException">Формуляр не знайдено.</exception>
    public Loan GetRequiredLoan(int loanId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(loanId);
        Loan? loan = _repository.GetById(loanId);
        if (loan is null)
        {
            LoanServiceLog.LoanNotFound(_logger, loanId);
            throw new DomainRuleException(
                "loan.exists",
                $"Формуляр {loanId} не знайдено.");
        }

        return loan;
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
            LoanServiceLog.NegativeTotal(_logger, loan.Id, total);
            throw new DomainRuleException(
                "loan.total.nonnegative",
                $"Вартість формуляра {loan.Id} не може бути від'ємною: {total}.");
        }

        return total;
    }
}
