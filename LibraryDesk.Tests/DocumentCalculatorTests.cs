using LibraryDesk.Core.Documents;
using LibraryDesk.Core.Domain;
using Xunit;

namespace LibraryDesk.Tests;

public sealed class DocumentCalculatorTests
{
    [Fact]
    public void Calculate_RegularReader_Returns1292_70()
    {
        LoanCalculationRequest request = new(
            1, "Іваненко", "i@ex.com", true,
            new List<LoanItem>
            {
                new("A1", 12, 100m),
                new("B2", 1, 250m),
            },
            new DateOnly(2026, 3, 10), null, LoanStatus.Active, 60m);

        LoanTotalResult result = DocumentTotalCalculator.Calculate(request);

        Assert.Equal(1292.70m, result.Total);
        Assert.Equal(LoanStatus.Returned, result.NextStatus);
    }

    [Fact]
    public void Calculate_SmallLoan_AddsDelivery()
    {
        LoanCalculationRequest request = new(
            2, "Петренко", "p@ex.com", false,
            new List<LoanItem> { new("A1", 2, 200m) },
            new DateOnly(2026, 3, 10), PricingRules.SaleCoupon, LoanStatus.Returned, 60m);

        LoanTotalResult result = DocumentTotalCalculator.Calculate(request);

        Assert.Equal(420m, result.Total);
        Assert.Equal(LoanStatus.Overdue, result.NextStatus);
    }

    [Fact]
    public void Calculate_NoBooks_ThrowsArgument()
    {
        LoanCalculationRequest request = CreateRequest(Array.Empty<LoanItem>());

        Assert.Throws<ArgumentException>(() => DocumentTotalCalculator.Calculate(request));
    }

    [Fact]
    public void Calculate_ZeroDays_ThrowsRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LoanItem("A1", 0, 10m));
    }

    [Fact]
    public void Calculate_InvalidReaderEmail_ThrowsArgument()
    {
        LoanCalculationRequest request = CreateRequest(
            new[] { new LoanItem("A1", 1, 10m) }) with
        { ReaderEmail = "invalid-email" };

        Assert.Throws<ArgumentException>(() => DocumentTotalCalculator.Calculate(request));
    }

    private static LoanCalculationRequest CreateRequest(IReadOnlyList<LoanItem> items)
        => new(1, "Читач", "reader@example.com", false, items,
            new DateOnly(2026, 3, 10), null, LoanStatus.Active, 60m);
}
