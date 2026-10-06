namespace LibraryDesk.Core.Documents;

/// <summary>Іменовані правила розрахунку вартості прокату.</summary>
public static class PricingRules
{
    /// <summary>Поріг гуртової знижки у днях.</summary>
    public const int BulkDays = 10;

    /// <summary>Ставка гуртової знижки.</summary>
    public const decimal BulkRate = 0.05m;

    /// <summary>Ставка знижки постійному читачеві.</summary>
    public const decimal RegularReaderRate = 0.07m;

    /// <summary>Поріг великої суми.</summary>
    public const decimal LargeLoanFrom = 5000m;

    /// <summary>Ставка знижки великої суми.</summary>
    public const decimal LargeLoanRate = 0.10m;

    /// <summary>Поріг середньої суми.</summary>
    public const decimal MediumLoanFrom = 2000m;

    /// <summary>Ставка знижки середньої суми.</summary>
    public const decimal MediumLoanRate = 0.05m;

    /// <summary>Ставка знижки у неділю.</summary>
    public const decimal WeekendRate = 0.02m;

    /// <summary>Код відсоткового купона.</summary>
    public const string SaleCoupon = "SALE10";

    /// <summary>Ставка відсоткового купона.</summary>
    public const decimal SaleCouponRate = 0.10m;

    /// <summary>Код фіксованого купона.</summary>
    public const string FixedCoupon = "MINUS200";

    /// <summary>Сума фіксованої знижки.</summary>
    public const decimal FixedCouponAmount = 200m;

    /// <summary>Мінімальна сума фіксованого купона.</summary>
    public const decimal FixedCouponFrom = 1000m;

    /// <summary>Поріг безкоштовної доставки.</summary>
    public const decimal FreeDeliveryFrom = 1000m;
}
