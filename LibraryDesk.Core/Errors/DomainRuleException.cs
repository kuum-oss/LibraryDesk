namespace LibraryDesk.Core.Errors;

/// <summary>Описує порушення правила предметної області LibraryDesk.</summary>
public class DomainRuleException : Exception
{
    /// <summary>Ініціалізує виняток для порушеного правила.</summary>
    /// <param name="rule">Стабільний ідентифікатор правила.</param>
    /// <param name="message">Повідомлення з контекстом порушення.</param>
    public DomainRuleException(string rule, string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rule);
        Rule = rule;
    }

    /// <summary>Ініціалізує виняток і зберігає початкову причину.</summary>
    /// <param name="rule">Стабільний ідентифікатор правила.</param>
    /// <param name="message">Повідомлення з контекстом порушення.</param>
    /// <param name="innerException">Початковий виняток.</param>
    public DomainRuleException(string rule, string message, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rule);
        Rule = rule;
    }

    /// <summary>Отримує ідентифікатор порушеного правила.</summary>
    public string Rule { get; }
}
