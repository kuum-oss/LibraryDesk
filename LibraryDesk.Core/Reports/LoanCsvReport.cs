using System.Globalization;
using System.Text;

namespace LibraryDesk.Core.Reports;

/// <summary>Форматує готові рядки звіту як CSV без запису на диск.</summary>
public static class LoanCsvReport
{
    /// <summary>Створює CSV текст із рядків звіту.</summary>
    /// <param name="rows">Підготовлені рядки звіту.</param>
    /// <returns>Текст у форматі CSV з роздільником крапка з комою.</returns>
    public static string BuildCsv(IEnumerable<LoanReportRow> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);
        StringBuilder csv = new("id;total;status");
        foreach (LoanReportRow row in rows)
        {
            csv.AppendLine();
            csv.Append(row.Id.ToString(CultureInfo.InvariantCulture));
            csv.Append(';');
            csv.Append(row.Total.ToString(CultureInfo.InvariantCulture));
            csv.Append(';');
            csv.Append(row.Status);
        }

        return csv.ToString();
    }
}
