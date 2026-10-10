using System.Text;
using LibraryDesk.App;
using LibraryDesk.Core;
using LibraryDesk.Core.Abstractions;
using LibraryDesk.Core.Domain;
using LibraryDesk.Core.Errors;
using LibraryDesk.Core.Parsing;
using LibraryDesk.Core.Pricing;
using LibraryDesk.Core.Reports;
using LibraryDesk.Core.Services;
using LibraryDesk.Core.Storage;
using Microsoft.Extensions.Logging;

Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine($"LibraryDesk v{VersionInfo.Current()}");
using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddSimpleConsole(options =>
    {
        options.TimestampFormat = "HH:mm:ss ";
        options.IncludeScopes = true;
        options.SingleLine = true;
    });
    builder.SetMinimumLevel(LogLevel.Debug);
});

ILogger applicationLogger = loggerFactory.CreateLogger("LibraryDesk.App");
try
{
    RunApplication(loggerFactory);
}
catch (DomainRuleException exception)
{
    ApplicationLog.DomainRuleFailed(applicationLogger, exception, exception.Rule, exception.Message);
    Environment.ExitCode = 1;
}
catch (Exception exception)
{
    ApplicationLog.UnexpectedFailure(applicationLogger, exception);
    Environment.ExitCode = 2;
}

static void RunApplication(ILoggerFactory loggerFactory)
{
    ILoanRepository repository = new InMemoryLoanRepository();
    IPricingPolicy pricing = new DiscountPricingPolicy(0.05m);
    INotifier notifier = new ConsoleLoanNotifier();
    LoanService service = new(repository, pricing, notifier, loggerFactory.CreateLogger<LoanService>());

    Reader reader = new(100, "Іваненко", "reader@example.com", true);
    Book book = new("978-966-00-0001-0", "Основи програмування", 250m, 3);
    DateTimeOffset issuedAt = new(2026, 10, 8, 10, 0, 0, TimeSpan.FromHours(3));
    Loan loan = new(1001, reader.Id, issuedAt, new DateOnly(2026, 10, 22));
    LoanItem? parsedItem = null;
    foreach (string rawInput in new[]
    {
        $"{book.Isbn};два;250",
        $"{book.Isbn};2;250",
    })
    {
        if (!LoanItemParser.TryParse(rawInput, out parsedItem))
        {
            Console.WriteLine("Помилка вводу: очікується ISBN;дні;денний тариф.");
            Console.WriteLine("Повторіть введення.");
            continue;
        }

        break;
    }

    loan.AddItem(parsedItem ?? throw new InvalidOperationException("Не отримано коректної позиції формуляра."));
    loan.Issue();
    service.Register(loan, reader);
    string auditPath = Path.Combine(AppContext.BaseDirectory, "lab05-audit.log");
    using (FileAuditLog audit = new(auditPath))
    {
        audit.Write("loan.registered", loan.Id);
    }

    Console.WriteLine($"Формуляр № {loan.Id}");
    Console.WriteLine($"Читач: {reader.FullName}");
    Console.WriteLine($"Книга: {book.Title}");
    Console.WriteLine($"Сума: {service.TotalOf(loan):0.00}");
    Console.WriteLine($"Стан: {loan.Status}");

    LoanReportRow row = new(loan.Id, service.TotalOf(loan), loan.Status);
    Console.WriteLine(LoanCsvReport.BuildCsv(new[] { row }));
}
