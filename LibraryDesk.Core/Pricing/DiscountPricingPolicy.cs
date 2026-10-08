using LibraryDesk.Core.Abstractions;
using LibraryDesk.Core.Domain;

namespace LibraryDesk.Core.Pricing;

/// <summary>Прокат зі сталою відсотковою знижкою.</summary>
public sealed class DiscountPricingPolicy : IPricingPolicy
{
    private readonly decimal _rate;

    /// <summary>Створює політику знижки.</summary>
    /// <param name="rate">Частка знижки від нуля до половини ціни.</param>
    public DiscountPricingPolicy(decimal rate)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(rate);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(rate, 0.5m);
        _rate = rate;
    }

    /// <inheritdoc />
    public decimal PriceOf(LoanItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return Math.Round(item.Amount * (1m - _rate), 2);
    }
}
