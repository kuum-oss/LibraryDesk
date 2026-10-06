using LibraryDesk.Core.Domain;

namespace LibraryDesk.Core.Documents;

/// <summary>Чисті запити для розрахунку вартості формуляра.</summary>
public static class DocumentTotalCalculator
{
    /// <summary>Підсумовує книжкові позиції з гуртовою знижкою.</summary>
    /// <param name="items">Позиції формуляра.</param>
    /// <returns>Проміжний підсумок.</returns>
    public static decimal SubtotalOf(IReadOnlyList<LoanItem> items)
    {
        DocumentGuards.EnsureLinesValid(items);
        decimal subtotal = 0m;
        foreach (LoanItem item in items)
        {
            decimal amount = item.Days * item.DailyRate;
            if (item.Days >= PricingRules.BulkDays)
            {
                amount -= amount * PricingRules.BulkRate;
            }

            subtotal += amount;
        }

        return subtotal;
    }

    /// <summary>Повертає знижку постійному читачеві.</summary>
    /// <param name="amount">Сума до знижки.</param>
    /// <param name="isRegularReader">Ознака постійного читача.</param>
    /// <returns>Сума знижки.</returns>
    public static decimal ReaderDiscountOf(decimal amount, bool isRegularReader)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return isRegularReader ? amount * PricingRules.RegularReaderRate : 0m;
    }

    /// <summary>Повертає знижку за розміром суми.</summary>
    /// <param name="amount">Сума до знижки.</param>
    /// <returns>Сума знижки.</returns>
    public static decimal VolumeDiscountOf(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        if (amount > PricingRules.LargeLoanFrom)
        {
            return amount * PricingRules.LargeLoanRate;
        }

        return amount > PricingRules.MediumLoanFrom
            ? amount * PricingRules.MediumLoanRate
            : 0m;
    }

    /// <summary>Повертає знижку за кодом купона.</summary>
    /// <param name="amount">Сума до знижки.</param>
    /// <param name="couponCode">Код купона.</param>
    /// <returns>Сума знижки.</returns>
    public static decimal CouponDiscountOf(decimal amount, string? couponCode)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return couponCode switch
        {
            PricingRules.SaleCoupon => amount * PricingRules.SaleCouponRate,
            PricingRules.FixedCoupon when amount > PricingRules.FixedCouponFrom => PricingRules.FixedCouponAmount,
            _ => 0m,
        };
    }

    /// <summary>Повертає знижку за видачу у неділю.</summary>
    /// <param name="amount">Сума до знижки.</param>
    /// <param name="issuedOn">Дата видачі.</param>
    /// <returns>Сума знижки.</returns>
    public static decimal WeekendDiscountOf(decimal amount, DateOnly issuedOn)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return issuedOn.DayOfWeek == DayOfWeek.Sunday ? amount * PricingRules.WeekendRate : 0m;
    }

    /// <summary>Повертає плату за доставку для малої суми.</summary>
    /// <param name="amount">Сума до доставки.</param>
    /// <param name="deliveryPrice">Ціна доставки.</param>
    /// <returns>Плата за доставку.</returns>
    public static decimal DeliveryFeeOf(decimal amount, decimal deliveryPrice)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        ArgumentOutOfRangeException.ThrowIfNegative(deliveryPrice);
        return amount < PricingRules.FreeDeliveryFrom ? deliveryPrice : 0m;
    }

    /// <summary>Повертає наступний статус формуляра без зміни стану.</summary>
    /// <param name="current">Поточний статус.</param>
    /// <param name="total">Підсумкова сума.</param>
    /// <returns>Наступний статус.</returns>
    public static LoanStatus NextStatusOf(LoanStatus current, decimal total)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(total);
        return current switch
        {
            LoanStatus.Active when total > 0m => LoanStatus.Returned,
            LoanStatus.Returned => LoanStatus.Overdue,
            _ => current,
        };
    }

    /// <summary>Обчислює підсумок за об'єктом параметрів.</summary>
    /// <param name="request">Параметри розрахунку.</param>
    /// <returns>Сума та наступний статус.</returns>
    public static LoanTotalResult Calculate(LoanCalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        DocumentGuards.EnsureHeaderValid(request.LoanId, request.ReaderName, request.ReaderEmail);
        decimal total = SubtotalOf(request.Items);
        total -= ReaderDiscountOf(total, request.IsRegularReader);
        total -= VolumeDiscountOf(total);
        total -= CouponDiscountOf(total, request.CouponCode);
        total -= WeekendDiscountOf(total, request.IssuedOn);
        total += DeliveryFeeOf(total, request.DeliveryPrice);
        total = request.Status == LoanStatus.Cancelled ? 0m : Math.Round(Math.Max(total, 0m), 2);
        return new LoanTotalResult(total, NextStatusOf(request.Status, total));
    }
}
