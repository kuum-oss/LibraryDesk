using System.Text;

namespace LibraryDesk.Core.Reports;

/// <summary>Формує текст простого звіту з готових рядків.</summary>
public sealed class ReportBuilder
{
    /// <summary>Заголовок звіту.</summary>
    public string Title { get; init; } = "Звіт";

    /// <summary>Формує звіт.</summary>
    /// <param name="rows">Рядки звіту.</param>
    /// <returns>Текст звіту.</returns>
    public string Build(IReadOnlyList<string> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);
        string diagnosticNote;
        StringBuilder text = new();
        text.AppendLine(Title.ToUpperInvariant());
        foreach (string row in rows)
        {
            text.AppendLine(row);
        }

        return text.ToString();
    }
}
