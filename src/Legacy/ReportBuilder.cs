using System.Text;

namespace LibraryDesk.Legacy;

public sealed class ReportBuilder
{
    public string Build(
        LoanRequest request,
        decimal discount,
        decimal shipping,
        decimal total)
    {
        var report = new StringBuilder();
        report.Append("Документ #").Append(request.DocumentId).Append('\n');
        report.Append("Клієнт: ").Append(request.Reader.Name).Append('\n');
        foreach (var item in request.Items!)
        {
            report.Append(Line(item, request.Currency));
        }

        report.Append(Money("Знижка", discount, request.Currency));
        report.Append(Money("Доставка", shipping, request.Currency));
        report.Append(Money("Разом", total, request.Currency));
        return report.ToString();
    }

    private static string Line(LoanLine item, string currency)
    {
        return item.Code + " x" + item.Qty + " = "
            + (item.Qty * item.Price).ToString("0.00") + " " + currency + "\n";
    }

    private static string Money(string label, decimal value, string currency)
    {
        return label + ": " + value.ToString("0.00") + " " + currency + "\n";
    }
}
