namespace LibraryDesk.Core.Pricing;

/// <summary>Обчислює пеню за прострочення повернення книги.</summary>
public sealed class LateFeePolicy
{
    /// <summary>Перший день, з якого діє підвищена ставка.</summary>
    public const int IncreasedRateFromDay = 8;

    /// <summary>Множник підвищеної ставки для тривалого прострочення.</summary>
    public const decimal IncreasedRateMultiplier = 1.5m;

    private readonly int _increasedRateFromDay = IncreasedRateFromDay;
    private readonly decimal _increasedRateMultiplier = IncreasedRateMultiplier;

    /// <summary>Обчислює пеню за кількістю прострочених днів.</summary>
    /// <param name="overdueDays">Кількість днів прострочення.</param>
    /// <param name="dailyFee">Базова пеня за один день.</param>
    /// <param name="bookPrice">Вартість книги для подальшого обмеження пені.</param>
    /// <returns>Сума пені, округлена до копійок.</returns>
    public decimal Calculate(int overdueDays, decimal dailyFee, decimal bookPrice)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(overdueDays);
        ArgumentOutOfRangeException.ThrowIfNegative(dailyFee);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bookPrice);

        if (overdueDays == 0)
        {
            return 0m;
        }

        decimal multiplier = overdueDays >= _increasedRateFromDay
            ? _increasedRateMultiplier
            : 1m;
        return decimal.Round(overdueDays * dailyFee * multiplier, 2);
    }
}
