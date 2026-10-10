using LibraryDesk.Core.Pricing;
using Xunit;

namespace LibraryDesk.Tests;

public sealed class LateFeePolicyTests
{
    [Fact]
    public void Calculate_NegativeOverdueDays_ThrowsOutOfRange()
    {
        LateFeePolicy policy = new();

        Action act = () => policy.Calculate(-1, 10m, 1000m);

        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Calculate_ZeroOverdueDays_ReturnsZero()
    {
        LateFeePolicy policy = new();

        decimal actual = policy.Calculate(0, 10m, 1000m);

        Assert.Equal(0m, actual);
    }

    [Fact]
    public void Calculate_OneOverdueDay_UsesStandardRate()
    {
        LateFeePolicy policy = new();

        decimal actual = policy.Calculate(1, 10m, 1000m);

        Assert.Equal(10m, actual);
    }

    [Fact]
    public void Calculate_SevenOverdueDays_UsesStandardRate()
    {
        LateFeePolicy policy = new();

        decimal actual = policy.Calculate(7, 10m, 1000m);

        Assert.Equal(70m, actual);
    }

    [Fact]
    public void Calculate_EightOverdueDays_UsesIncreasedRate()
    {
        LateFeePolicy policy = new();

        decimal actual = policy.Calculate(8, 10m, 1000m);

        Assert.Equal(120m, actual);
    }

    [Fact]
    public void Calculate_TenOverdueDays_UsesIncreasedRate()
    {
        LateFeePolicy policy = new();

        decimal actual = policy.Calculate(10, 10m, 1000m);

        Assert.Equal(150m, actual);
    }
}
