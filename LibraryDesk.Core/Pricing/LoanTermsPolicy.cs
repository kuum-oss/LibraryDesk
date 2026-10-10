namespace LibraryDesk.Core.Pricing;

/// <summary>Визначає стандартний строк видачі та кількість прострочених днів.</summary>
public static class LoanTermsPolicy
{
    /// <summary>Стандартна тривалість видачі у календарних днях.</summary>
    public const int StandardLoanDays = 14;

    /// <summary>Визначає останній день повернення для дати видачі.</summary>
    /// <param name="issuedOn">Дата видачі.</param>
    /// <returns>Дата повернення включно.</returns>
    public static DateOnly DueOn(DateOnly issuedOn) => issuedOn.AddDays(StandardLoanDays);

    /// <summary>Рахує повні дні після встановленого строку.</summary>
    /// <param name="dueOn">Останній день повернення без пені.</param>
    /// <param name="actualOn">Фактична дата повернення або звіту.</param>
    /// <returns>Нуль або додатна кількість прострочених днів.</returns>
    public static int OverdueDays(DateOnly dueOn, DateOnly actualOn)
        => Math.Max(0, actualOn.DayNumber - dueOn.DayNumber);
}
