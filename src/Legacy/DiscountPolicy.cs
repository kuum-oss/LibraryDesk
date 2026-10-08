namespace LibraryDesk.Legacy;

public sealed class DiscountPolicy
{
    public decimal For(decimal subtotal, ReaderKind kind, int doneCount)
    {
        var rate = kind switch
        {
            ReaderKind.Vip => PricingRules.VipRate,
            ReaderKind.Staff => PricingRules.StaffRate,
            _ when doneCount > PricingRules.LoyalLoans => PricingRules.LoyalRate,
            _ => 0m,
        };

        return Math.Min(subtotal * rate, PricingRules.MaxDiscount);
    }
}
