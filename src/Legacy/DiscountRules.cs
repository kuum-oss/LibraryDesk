namespace LibraryDesk.Legacy;

public interface IDiscountRule
{
    decimal? RateFor(Reader reader);
}

public sealed class VipDiscountRule : IDiscountRule
{
    public decimal? RateFor(Reader reader)
    {
        return reader.Kind == ReaderKind.Vip ? PricingRules.VipRate : null;
    }
}

public sealed class StaffDiscountRule : IDiscountRule
{
    public decimal? RateFor(Reader reader)
    {
        return reader.Kind == ReaderKind.Staff ? PricingRules.StaffRate : null;
    }
}

public sealed class LoyalReaderDiscountRule : IDiscountRule
{
    public decimal? RateFor(Reader reader)
    {
        return reader.DoneCount > PricingRules.LoyalLoans ? PricingRules.LoyalRate : null;
    }
}
