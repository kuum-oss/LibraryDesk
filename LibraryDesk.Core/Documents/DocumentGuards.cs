using LibraryDesk.Core.Domain;

namespace LibraryDesk.Core.Documents;

/// <summary>Перевірки передумов операцій із формуляром.</summary>
public static class DocumentGuards
{
    /// <summary>Перевіряє реквізити формуляра та читача.</summary>
    /// <param name="loanId">Ідентифікатор формуляра.</param>
    /// <param name="readerName">Ім'я читача.</param>
    /// <param name="readerEmail">Електронна пошта читача.</param>
    public static void EnsureHeaderValid(int loanId, string readerName, string readerEmail)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(loanId);
        ArgumentException.ThrowIfNullOrWhiteSpace(readerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(readerEmail);
        if (!readerEmail.Contains('@'))
        {
            throw new ArgumentException("Пошта має містити символ @.", nameof(readerEmail));
        }
    }

    /// <summary>Перевіряє одну книжкову позицію.</summary>
    /// <param name="item">Книжкова позиція.</param>
    public static void EnsureLineValid(LoanItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(item.Isbn);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(item.Days, nameof(item.Days));
        ArgumentOutOfRangeException.ThrowIfNegative(item.DailyRate, nameof(item.DailyRate));
    }

    /// <summary>Перевіряє непорожній список книжок.</summary>
    /// <param name="items">Позиції формуляра.</param>
    public static void EnsureLinesValid(IReadOnlyList<LoanItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (items.Count == 0)
        {
            throw new ArgumentException("Формуляр не містить жодної книжки.", nameof(items));
        }

        foreach (LoanItem item in items)
        {
            EnsureLineValid(item);
        }
    }
}
