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
        new("SKU-1", 3, 250m),
        new("SKU-2", 12, 90m),
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
