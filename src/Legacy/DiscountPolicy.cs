namespace LibraryDesk.Legacy;

public sealed class DiscountPolicy
{
    private readonly IReadOnlyList<IDiscountRule> _rules =
    [
        new VipDiscountRule(),
        new StaffDiscountRule(),
        new LoyalReaderDiscountRule(),
    ];

    public decimal For(decimal subtotal, Reader reader)
    {
        decimal rate = 0m;
        foreach (var rule in _rules)
        {
            var matchingRate = rule.RateFor(reader);
            if (matchingRate.HasValue)
            {
                rate = matchingRate.Value;
                break;
            }
        }

        return Math.Min(subtotal * rate, PricingRules.MaxDiscount);
    }
}
