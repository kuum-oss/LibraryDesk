using System.Text;
using LibraryDesk.App;
using LibraryDesk.Core.Abstractions;
using LibraryDesk.Core.Domain;
using LibraryDesk.Core.Errors;
using LibraryDesk.Core.Pricing;
using LibraryDesk.Core.Reports;
using LibraryDesk.Core.Services;
using LibraryDesk.Core.Storage;

Console.OutputEncoding = Encoding.UTF8;
ILoanRepository repository = new InMemoryLoanRepository();
IPricingPolicy pricing = new DiscountPricingPolicy(0.05m);
INotifier notifier = new ConsoleLoanNotifier();
LoanService service = new(repository, pricing, notifier);

Reader reader = new(100, "Іваненко", "reader@example.com", true);
Book book = new("978-966-00-0001-0", "Основи програмування", 250m, 3);
DateTimeOffset issuedAt = new(2026, 10, 8, 10, 0, 0, TimeSpan.FromHours(3));
Loan loan = new(1001, reader.Id, issuedAt, new DateOnly(2026, 10, 22));
LoanItem? parsedItem = null;
foreach (string daysInput in new[] { "два", "2" })
{
    Result<LoanItem> parsed = LoanItemParser.Parse(book.Isbn, daysInput, "250");
    if (!parsed.IsSuccess)
    {
        Console.WriteLine($"Помилка вводу: {parsed.Error}");
        Console.WriteLine("Повторіть введення.");
        continue;
    }

    parsedItem = parsed.Value;
    break;
}

loan.AddItem(parsedItem ?? throw new InvalidOperationException("Не отримано коректної позиції формуляра."));
loan.Issue();
service.Register(loan, reader);

Console.WriteLine($"Формуляр № {loan.Id}");
Console.WriteLine($"Читач: {reader.FullName}");
Console.WriteLine($"Книга: {book.Title}");
Console.WriteLine($"Сума: {service.TotalOf(loan):0.00}");
Console.WriteLine($"Стан: {loan.Status}");

LoanReportRow row = new(loan.Id, service.TotalOf(loan), loan.Status);
Console.WriteLine(LoanCsvReport.BuildCsv(new[] { row }));
