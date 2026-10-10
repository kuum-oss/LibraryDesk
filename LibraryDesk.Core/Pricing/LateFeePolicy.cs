using LibraryDesk.Core.Abstractions;

namespace LibraryDesk.Core.Pricing;

/// <summary>Обчислює пеню за прострочення повернення книги.</summary>
public sealed class LateFeePolicy : ILateFeePolicy
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
        EnsureArgumentsValid(overdueDays, dailyFee, bookPrice);
        decimal rawFee = overdueDays * dailyFee * MultiplierFor(overdueDays);
        return Math.Min(decimal.Round(rawFee, 2), bookPrice);
    }

    private static void EnsureArgumentsValid(
        int overdueDays,
        decimal dailyFee,
        decimal bookPrice)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(overdueDays);
        ArgumentOutOfRangeException.ThrowIfNegative(dailyFee);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bookPrice);
    }

    private decimal MultiplierFor(int overdueDays)
        => overdueDays >= _increasedRateFromDay
            ? _increasedRateMultiplier
            : 1m;
}
