using System.Text;
using LibraryDesk.Core.Documents;
using LibraryDesk.Core.Domain;

Console.OutputEncoding = Encoding.UTF8;
LoanCalculationRequest request = new(
    1001,
    "Іваненко",
    "reader@example.com",
    true,
    new List<LoanItem>
    {
        new() { Isbn = "SKU-1", Days = 3, DailyRate = 250m },
        new() { Isbn = "SKU-2", Days = 12, DailyRate = 90m },
    },
    new DateOnly(2026, 10, 6),
    null,
    LoanStatus.Active,
    60m);

LoanTotalResult result = DocumentTotalCalculator.Calculate(request);
Console.WriteLine($"Формуляр № {request.LoanId}");
Console.WriteLine($"Читач: {request.ReaderName}");
Console.WriteLine($"Разом: {result.Total}");
Console.WriteLine($"Наступний стан: {result.NextStatus}");
