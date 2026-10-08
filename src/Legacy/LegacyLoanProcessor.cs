namespace LibraryDesk.Legacy;

public class Reader
{
    public int Id;
    public string Name = "";
    public string Email = "";
    public ReaderKind Kind = ReaderKind.Regular;
    public int DoneCount;
    public DateTime SinceUtc;
}

public class LoanLine
{
    public string Code = "";
    public int Qty;
    public decimal Price;
    public string Group = "";
}

public class LegacyLoanProcessor
{
    private decimal _tmpSum;
    private decimal _tmpDiscount;
    private readonly List<string> _log = new();

    public string Handle(
        int docId,
        int readerId,
        string readerName,
        string? readerMail,
        ReaderKind readerKind,
        int readerDone,
        List<LoanLine>? items,
        LoanState state,
        string currency,
        DateTime createdAt,
        bool sendMail)
    {
        var error = Validate(items, state, readerMail);
        if (error is not null)
        {
            return error;
        }

        _log.Add("ok " + docId);
        var validItems = items!;
        _tmpSum = Subtotal(validItems);
        _tmpDiscount = DiscountOf(_tmpSum, readerKind, readerDone);
        decimal ship = ShippingOf(_tmpSum - _tmpDiscount);
        decimal total = _tmpSum - _tmpDiscount + ship;
        string txt = "Документ #" + docId + "\n";
        txt += "Клієнт: " + readerName + "\n";
        for (int i = 0; i < validItems.Count; i++)
        {
            txt += validItems[i].Code + " x" + validItems[i].Qty + " = "
                + (validItems[i].Qty * validItems[i].Price).ToString("0.00") + " " + currency + "\n";
        }

        txt += "Знижка: " + _tmpDiscount.ToString("0.00") + " " + currency + "\n";
        txt += "Доставка: " + ship.ToString("0.00") + " " + currency + "\n";
        txt += "Разом: " + total.ToString("0.00") + " " + currency + "\n";
        if (sendMail)
        {
            _log.Add("mail -> " + readerMail);
        }

        return txt;
    }

    private static string? Validate(
        List<LoanLine>? items,
        LoanState state,
        string? readerMail)
    {
        if (items is null)
        {
            return "ERR: null";
        }

        if (items.Count == 0)
        {
            return "ERR: empty";
        }

        if (state is not (LoanState.New or LoanState.Paid))
        {
            return "ERR: state";
        }

        if (readerMail is null || !readerMail.Contains("@"))
        {
            return "ERR: mail";
        }

        return null;
    }

    private static decimal Subtotal(IReadOnlyList<LoanLine> items)
    {
        decimal sum = 0m;
        foreach (var item in items)
        {
            sum += item.Qty * item.Price;
        }

        return sum;
    }

    private static decimal DiscountOf(decimal subtotal, ReaderKind kind, int doneCount)
    {
        decimal rate = kind switch
        {
            ReaderKind.Vip => PricingRules.VipRate,
            ReaderKind.Staff => PricingRules.StaffRate,
            _ when doneCount > PricingRules.LoyalLoans => PricingRules.LoyalRate,
            _ => 0m,
        };

        return Math.Min(subtotal * rate, PricingRules.MaxDiscount);
    }

    private static decimal ShippingOf(decimal payable)
    {
        return payable < PricingRules.FreeShippingFrom ? PricingRules.ShippingCost : 0m;
    }

    public decimal Preview(ReaderKind readerKind, int readerDone, List<LoanLine> items)
    {
        decimal sum = 0m;
        foreach (var item in items)
        {
            sum += item.Qty * item.Price;
        }

        decimal discount = 0m;
        if (readerKind == ReaderKind.Vip)
        {
            discount = sum * PricingRules.VipRate;
            if (discount > PricingRules.MaxDiscount)
            {
                discount = PricingRules.MaxDiscount;
            }
        }
        else if (readerKind == ReaderKind.Staff)
        {
            discount = sum * PricingRules.StaffRate;
            if (discount > PricingRules.MaxDiscount)
            {
                discount = PricingRules.MaxDiscount;
            }
        }
        else if (readerDone > PricingRules.LoyalLoans)
        {
            discount = sum * PricingRules.LoyalRate;
            if (discount > PricingRules.MaxDiscount)
            {
                discount = PricingRules.MaxDiscount;
            }
        }

        decimal shipping = 0m;
        if (sum - discount < PricingRules.FreeShippingFrom)
        {
            shipping = PricingRules.ShippingCost;
        }

        return sum - discount + shipping;
    }

    public string DescribeReader(Reader reader)
    {
        string description = reader.Name.Trim().ToUpper();
        if (reader.Kind == ReaderKind.Vip)
        {
            description += " [VIP]";
        }

        if (reader.DoneCount > PricingRules.LoyalLoans)
        {
            description += " [ЛОЯЛЬНИЙ]";
        }

        description += " <" + reader.Email.ToLower() + ">";
        int years = DateTime.Now.Year - reader.SinceUtc.Year;
        description += " стаж " + years;
        return description;
    }

    public string DumpLog()
    {
        string result = "";
        foreach (var entry in _log)
        {
            result += entry + "\n";
        }

        return result;
    }
}
