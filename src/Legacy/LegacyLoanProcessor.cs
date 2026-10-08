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
    private readonly DiscountPolicy _discounts = new();
    private readonly ShippingPolicy _shipping = new();
    private readonly ReportBuilder _report = new();
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
        _tmpDiscount = _discounts.For(_tmpSum, readerKind, readerDone);
        decimal ship = _shipping.For(_tmpSum - _tmpDiscount);
        decimal total = _tmpSum - _tmpDiscount + ship;
        if (sendMail)
        {
            _log.Add("mail -> " + readerMail);
        }

        return _report.Build(
            docId,
            readerName,
            validItems,
            currency,
            _tmpDiscount,
            ship,
            total);
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

    public decimal Preview(ReaderKind readerKind, int readerDone, List<LoanLine> items)
    {
        var subtotal = Subtotal(items);
        var discount = _discounts.For(subtotal, readerKind, readerDone);
        var payable = subtotal - discount;
        return payable + _shipping.For(payable);
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
