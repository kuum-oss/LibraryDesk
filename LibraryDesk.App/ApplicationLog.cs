using Microsoft.Extensions.Logging;

namespace LibraryDesk.App;

/// <summary>Повідомлення верхньої межі консольного застосунку.</summary>
internal static partial class ApplicationLog
{
    [LoggerMessage(5101, LogLevel.Warning, "Порушено правило {Rule}: {Message}")]
    public static partial void DomainRuleFailed(
        ILogger logger,
        Exception exception,
        string rule,
        string message);

    [LoggerMessage(5102, LogLevel.Critical, "Непередбачений збій застосунку")]
    public static partial void UnexpectedFailure(ILogger logger, Exception exception);
}
