using LibraryDesk.Core.Domain;
using Microsoft.Extensions.Logging;

namespace LibraryDesk.Core.Services;

/// <summary>Високопродуктивні структуровані повідомлення сервісу формулярів.</summary>
internal static partial class LoanServiceLog
{
    private static readonly Func<ILogger, int, IDisposable?> _loanScope =
        LoggerMessage.DefineScope<int>("Loan:{LoanId}");

    /// <summary>Створює область журналу для однієї операції з формуляром.</summary>
    /// <param name="logger">Журнал операції.</param>
    /// <param name="loanId">Ідентифікатор формуляра.</param>
    /// <returns>Область, яку треба звільнити після операції.</returns>
    public static IDisposable? BeginLoanScope(ILogger logger, int loanId) => _loanScope(logger, loanId);

    [LoggerMessage(5001, LogLevel.Debug, "Початок реєстрації формуляра {LoanId}, сума {Total}")]
    public static partial void RegisterStarting(ILogger logger, int loanId, decimal total);

    [LoggerMessage(5002, LogLevel.Information, "Формуляр {LoanId} зареєстровано на суму {Total}")]
    public static partial void Registered(ILogger logger, int loanId, decimal total);

    [LoggerMessage(5003, LogLevel.Warning, "Формуляр {LoanId} має нульову вартість")]
    public static partial void ZeroTotal(ILogger logger, int loanId);

    [LoggerMessage(5004, LogLevel.Warning, "Формуляр {LoanId} не знайдено")]
    public static partial void LoanNotFound(ILogger logger, int loanId);

    [LoggerMessage(5005, LogLevel.Warning, "Правило {Rule} порушено для формуляра {LoanId} у стані {Status}")]
    public static partial void RuleViolation(ILogger logger, int loanId, string rule, LoanStatus status);

    [LoggerMessage(5006, LogLevel.Error, "Збій сховища під час реєстрації формуляра {LoanId}")]
    public static partial void StorageFailure(ILogger logger, Exception exception, int loanId);

    [LoggerMessage(5007, LogLevel.Error, "Формуляр {LoanId} має від'ємну вартість {Total}")]
    public static partial void NegativeTotal(ILogger logger, int loanId, decimal total);
}
