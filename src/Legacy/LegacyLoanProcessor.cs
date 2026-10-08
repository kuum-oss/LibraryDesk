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

    public string Handle(LoanRequest request)
    {
        var error = Validate(request);
        if (error is not null)
        {
            return error;
        }

        _log.Add("ok " + request.DocumentId);
        var validItems = request.Items!;
        _tmpSum = Subtotal(validItems);
        _tmpDiscount = _discounts.For(
            _tmpSum,
            request.Reader.Kind,
            request.Reader.DoneCount);
        decimal ship = _shipping.For(_tmpSum - _tmpDiscount);
        decimal total = _tmpSum - _tmpDiscount + ship;
        if (request.SendMail)
        {
            _log.Add("mail -> " + request.Reader.Email);
        }

        return _report.Build(request, _tmpDiscount, ship, total);
    }

    private static string? Validate(LoanRequest request)
    {
        if (request.Items is null)
        {
            return "ERR: null";
        }

        if (request.Items.Count == 0)
        {
            return "ERR: empty";
        }

        if (request.State is not (LoanState.New or LoanState.Paid))
        {
            return "ERR: state";
        }

        if (request.Reader.Email is null || !request.Reader.Email.Contains("@"))
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
