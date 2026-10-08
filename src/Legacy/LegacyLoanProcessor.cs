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
        _tmpSum = 0m;
        for (int i = 0; i < validItems.Count; i++)
        {
            _tmpSum += validItems[i].Qty * validItems[i].Price;
        }

        _tmpDiscount = 0m;
        if (readerKind == ReaderKind.Vip)
        {
            _tmpDiscount = _tmpSum * PricingRules.VipRate;
            if (_tmpDiscount > PricingRules.MaxDiscount)
            {
                _tmpDiscount = PricingRules.MaxDiscount;
            }
        }
        else if (readerKind == ReaderKind.Staff)
        {
            _tmpDiscount = _tmpSum * PricingRules.StaffRate;
            if (_tmpDiscount > PricingRules.MaxDiscount)
            {
                _tmpDiscount = PricingRules.MaxDiscount;
            }
        }
        else if (readerDone > PricingRules.LoyalLoans)
        {
            _tmpDiscount = _tmpSum * PricingRules.LoyalRate;
            if (_tmpDiscount > PricingRules.MaxDiscount)
            {
                _tmpDiscount = PricingRules.MaxDiscount;
            }
        }

        decimal ship = 0m;
        if (_tmpSum - _tmpDiscount < PricingRules.FreeShippingFrom)
        {
            ship = PricingRules.ShippingCost;
        }

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
