namespace LibraryDesk.Legacy;

public sealed class ShippingPolicy
{
    public decimal For(decimal payable)
    {
        return payable < PricingRules.FreeShippingFrom ? PricingRules.ShippingCost : 0m;
    }
}
