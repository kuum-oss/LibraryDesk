#pragma warning disable CS1591, CA1822, SA1402, SA1600, SA1611, SA1615

using System.Globalization;
using System.Text.Json;

namespace LibraryDesk.Core.LegacyLab4;

// Навмисно змішує розрахунок, збереження, сповіщення та звіт.
public sealed class LoanManager
{
    private readonly FileLoanStore _store = new();
    private readonly SmtpNotifier _notifier = new();

    public decimal Place(LegacyLoan loan, string readerType)
    {
        decimal total = 0m;
        foreach (LegacyLoanLine line in loan.Lines)
        {
            total += line.DailyRate * line.Days;
        }

        if (readerType == "regular")
        {
            total *= 0.95m;
        }
        else if (readerType == "vip")
        {
            total *= 0.90m;
        }
        else if (readerType == "staff")
        {
            total *= 0.70m;
        }

        if (total > 1000m)
        {
            total -= 50m;
        }

        loan.Total = total;
        loan.Status = 1;
        _store.SaveToFile(loan, "loans.json");
        _notifier.Send(loan.Email, "Loan " + loan.Id, total.ToString(CultureInfo.InvariantCulture));
        return total;
    }

    public string BuildCsvReport(List<LegacyLoan> loans)
    {
        string report = "id;total;status" + Environment.NewLine;
        foreach (LegacyLoan loan in loans)
        {
            report += loan.Id + ";" + loan.Total + ";" + loan.Status + Environment.NewLine;
        }

        File.WriteAllText("report.csv", report);
        Console.WriteLine("Звіт збережено.");
        return report;
    }

    public void Archive(LegacyLoan loan)
    {
        LegacyLoan archived = new ArchivedLoan(loan.Id);
        foreach (LegacyLoanLine line in loan.Lines)
        {
            archived.AddLine(line);
        }

        _store.SaveToFile(archived, "archive.json");
    }
}

internal sealed class FileLoanStore
{
    public void SaveToFile(LegacyLoan loan, string path)
        => File.WriteAllText(path, JsonSerializer.Serialize(loan));
}

internal sealed class SmtpNotifier
{
    public void Send(string to, string subject, string body)
        => Console.WriteLine($"{to}: {subject} — {body}");
}
