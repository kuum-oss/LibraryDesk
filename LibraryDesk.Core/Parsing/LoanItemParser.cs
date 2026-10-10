using System.Globalization;
using LibraryDesk.Core.Domain;

namespace LibraryDesk.Core.Parsing;

/// <summary>Перетворює неперевірений зовнішній рядок на позицію формуляра.</summary>
public static class LoanItemParser
{
    private const int MinIsbnLength = 10;
    private const int MaxIsbnLength = 20;
    private const int MaxLoanDays = 365;
    private const decimal MaxDailyRate = 100_000m;

    /// <summary>Перевіряє формат <c>ISBN;дні;денний тариф</c> і межі значень.</summary>
    /// <param name="raw">Неперевірений рядок зовнішнього вводу.</param>
    /// <param name="item">Створена позиція або <see langword="null"/>.</param>
    /// <returns><see langword="true"/>, якщо рядок пройшов усі перевірки.</returns>
    public static bool TryParse(string? raw, out LoanItem? item)
    {
        item = null;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        string[] parts = raw.Split(';');
        if (parts.Length != 3)
        {
            return false;
        }

        string isbn = parts[0].Trim();
        if (isbn.Length is < MinIsbnLength or > MaxIsbnLength)
        {
            return false;
        }

        if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int days)
            || days is < 1 or > MaxLoanDays)
        {
            return false;
        }

        const NumberStyles RateStyles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
        if (!decimal.TryParse(
                parts[2],
                RateStyles,
                CultureInfo.InvariantCulture,
                out decimal dailyRate)
            || dailyRate is < 0m or > MaxDailyRate)
        {
            return false;
        }

        item = new LoanItem(isbn, days, dailyRate);
        return true;
    }
}
