using System.Globalization;
using LibraryDesk.Core.Domain;
using LibraryDesk.Core.Errors;

namespace LibraryDesk.App;

/// <summary>Перетворює неперевірені рядки інтерфейсу на позицію формуляра.</summary>
public static class LoanItemParser
{
    /// <summary>Перевіряє введення та створює типізовану позицію формуляра.</summary>
    /// <param name="isbn">Введений ISBN.</param>
    /// <param name="days">Введена кількість днів.</param>
    /// <param name="dailyRate">Введений денний тариф.</param>
    /// <returns>Позиція або пояснення помилки введення.</returns>
    public static Result<LoanItem> Parse(string isbn, string days, string dailyRate)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            return Result.Fail<LoanItem>("ISBN не може бути порожнім.");
        }

        if (!int.TryParse(days, NumberStyles.Integer, CultureInfo.CurrentCulture, out int parsedDays)
            || parsedDays <= 0)
        {
            return Result.Fail<LoanItem>("Кількість днів має бути цілим числом більше нуля.");
        }

        if (!decimal.TryParse(
                dailyRate,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out decimal parsedRate)
            || parsedRate < 0m)
        {
            return Result.Fail<LoanItem>(
                "Тариф має бути невід'ємним числом із роздільником поточної культури.");
        }

        return Result.Ok(new LoanItem(isbn.Trim(), parsedDays, parsedRate));
    }
}
