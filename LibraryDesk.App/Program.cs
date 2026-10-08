using System.Text;
using LibraryDesk.Core.Abstractions;
using LibraryDesk.Core.Domain;
using LibraryDesk.Core.Pricing;
using LibraryDesk.Core.Services;
using LibraryDesk.Core.Storage;

Console.OutputEncoding = Encoding.UTF8;
ILoanRepository repository = new InMemoryLoanRepository();
IPricingPolicy pricing = new DiscountPricingPolicy(0.05m);
LoanService service = new(repository, pricing);

Reader reader = new(100, "Іваненко", "reader@example.com", true);
Book book = new("978-966-00-0001-0", "Основи програмування", 250m, 3);
DateTimeOffset issuedAt = new(2026, 10, 8, 10, 0, 0, TimeSpan.FromHours(3));
Loan loan = new(1001, reader.Id, issuedAt, new DateOnly(2026, 10, 22));
loan.AddItem(new LoanItem(book.Isbn, 2, book.RentalFee));
loan.Issue();
service.Register(loan);

Console.WriteLine($"Формуляр № {loan.Id}");
Console.WriteLine($"Читач: {reader.FullName}");
Console.WriteLine($"Книга: {book.Title}");
Console.WriteLine($"Сума: {service.TotalOf(loan):0.00}");
Console.WriteLine($"Стан: {loan.Status}");
