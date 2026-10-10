using LibraryDesk.Core.Pricing;
using Xunit;

namespace LibraryDesk.Tests;

public sealed class LateFeePolicyTests
{
    [Theory]
    [InlineData(0, 10, 1000, 0)]
    [InlineData(1, 10, 1000, 10)]
    [InlineData(7, 10, 1000, 70)]
    [InlineData(8, 10, 1000, 120)]
    [InlineData(10, 10, 1000, 150)]
    [InlineData(8, 12.5, 1000, 150)]
    public void Calculate_Boundaries_MatchesTable(
        int overdueDays,
        double dailyFee,
        double bookPrice,
        double expected)
    {
        LateFeePolicy policy = new();

        decimal actual = policy.Calculate(
            overdueDays,
            (decimal)dailyFee,
            (decimal)bookPrice);

        Assert.Equal((decimal)expected, actual);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-30)]
    public void Calculate_NegativeOverdueDays_ThrowsOutOfRange(int overdueDays)
    {
        LateFeePolicy policy = new();

        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => policy.Calculate(overdueDays, 10m, 1000m));

        Assert.Equal("overdueDays", exception.ParamName);
    }

    [Fact]
    public void Calculate_FeeAboveBookPrice_IsCappedAtBookPrice()
    {
        LateFeePolicy policy = new();

        decimal actual = policy.Calculate(30, 10m, 200m);

        Assert.Equal(200m, actual);
    }
}
