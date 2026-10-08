using System.Text;

namespace LibraryDesk.Legacy;

public sealed class ReportBuilder
{
    public string Build(
        int documentId,
        string readerName,
        IReadOnlyList<LoanLine> items,
        string currency,
        decimal discount,
        decimal shipping,
        decimal total)
    {
        var report = new StringBuilder();
        report.Append("Документ #").Append(documentId).Append('\n');
        report.Append("Клієнт: ").Append(readerName).Append('\n');
        foreach (var item in items)
        {
            report.Append(Line(item, currency));
        }

        report.Append(Money("Знижка", discount, currency));
        report.Append(Money("Доставка", shipping, currency));
        report.Append(Money("Разом", total, currency));
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
