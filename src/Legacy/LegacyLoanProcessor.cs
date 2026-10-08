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
            _tmpDiscount = _tmpSum * 0.15m;
            if (_tmpDiscount > 500m)
            {
                _tmpDiscount = 500m;
            }
        }
        else if (readerKind == "staff")
        {
            _tmpDiscount = _tmpSum * 0.30m;
            if (_tmpDiscount > 500m)
            {
                _tmpDiscount = 500m;
            }
        }
        else if (readerDone > 10)
        {
            _tmpDiscount = _tmpSum * 0.05m;
            if (_tmpDiscount > 500m)
            {
                _tmpDiscount = 500m;
            }
        }

        decimal ship = 0m;
        if (_tmpSum - _tmpDiscount < 1000m)
        {
            ship = 60m;
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
            discount = sum * 0.15m;
            if (discount > 500m)
            {
                discount = 500m;
            }
        }
        else if (readerKind == "staff")
        {
            discount = sum * 0.30m;
            if (discount > 500m)
            {
                discount = 500m;
            }
        }
        else if (readerDone > 10)
        {
            discount = sum * 0.05m;
            if (discount > 500m)
            {
                discount = 500m;
            }
        }

        decimal shipping = 0m;
        if (sum - discount < 1000m)
        {
            shipping = 60m;
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

        if (reader.DoneCount > 10)
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
