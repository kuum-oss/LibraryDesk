using LibraryDesk.Core.Abstractions;
using LibraryDesk.Core.Domain;

namespace LibraryDesk.Core.Pricing;

/// <summary>Прокат за повним тарифом.</summary>
public sealed class StandardPricingPolicy : IPricingPolicy
{
    /// <inheritdoc />
    public decimal PriceOf(LoanItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return item.Amount;
    }
}
