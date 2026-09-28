using System.Globalization;
using LibraryDesk.Core.Domain;

Loan loan = new()
{
    Id = 1,
    ReaderId = 10,
    IssuedAt = DateTimeOffset.Now,
};

loan.AddItem(new LoanItem
{
    Isbn = "978-0132350884",
    Days = 14,
    DailyRate = 12.50m,
});

loan.AddItem(new LoanItem
{
    Isbn = "978-0201633610",
    Days = 7,
    DailyRate = 18.00m,
});

string total = loan.Total()
    .ToString("F2", CultureInfo.InvariantCulture);

Console.WriteLine($"Видача #{loan.Id}");
Console.WriteLine($"Стан: {loan.Status}");
Console.WriteLine($"Позицій: {loan.Items.Count}");
Console.WriteLine($"Сума: {total}");
