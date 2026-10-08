namespace LibraryDesk.Legacy;

public class Reader
{
    public int Id;
    public string Name = "";
    public string Email = "";
    public string Kind = "regular";
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
        string readerKind,
        int readerDone,
        List<LoanLine>? items,
        string state,
        string currency,
        DateTime createdAt,
        bool sendMail)
    {
        if (items != null)
        {
            if (items.Count > 0)
            {
                if (state == "new" || state == "paid")
                {
                    if (readerMail != null && readerMail.Contains("@"))
                    {
                        _log.Add("ok " + docId);
                    }
                    else
                    {
                        return "ERR: mail";
                    }
                }
                else
                {
                    return "ERR: state";
                }
            }
            else
            {
                return "ERR: empty";
            }
        }
        else
        {
            return "ERR: null";
        }

        _tmpSum = 0m;
        for (int i = 0; i < items.Count; i++)
        {
            _tmpSum += items[i].Qty * items[i].Price;
        }

        _tmpDiscount = 0m;
        if (readerKind == "vip")
        {
            _tmpDiscount = _tmpSum * PricingRules.VipRate;
            if (_tmpDiscount > PricingRules.MaxDiscount)
            {
                _tmpDiscount = PricingRules.MaxDiscount;
            }
        }
        else if (readerKind == "staff")
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
        for (int i = 0; i < items.Count; i++)
        {
            txt += items[i].Code + " x" + items[i].Qty + " = "
                + (items[i].Qty * items[i].Price).ToString("0.00") + " " + currency + "\n";
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

    public decimal Preview(string readerKind, int readerDone, List<LoanLine> items)
    {
        decimal sum = 0m;
        foreach (var item in items)
        {
            sum += item.Qty * item.Price;
        }

        decimal discount = 0m;
        if (readerKind == "vip")
        {
            discount = sum * PricingRules.VipRate;
            if (discount > PricingRules.MaxDiscount)
            {
                discount = PricingRules.MaxDiscount;
            }
        }
        else if (readerKind == "staff")
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
        if (reader.Kind == "vip")
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
