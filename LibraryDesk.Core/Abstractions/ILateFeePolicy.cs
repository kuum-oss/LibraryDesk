namespace LibraryDesk.Core.Abstractions;

/// <summary>Правило обчислення пені за прострочене повернення.</summary>
public interface ILateFeePolicy
{
    /// <summary>Обчислює пеню за одним примірником.</summary>
    /// <param name="overdueDays">Кількість повних прострочених днів.</param>
    /// <param name="dailyFee">Базова денна пеня.</param>
    /// <param name="bookPrice">Вартість примірника, якою обмежено пеню.</param>
    /// <returns>Пеня, округлена до копійок.</returns>
    decimal Calculate(int overdueDays, decimal dailyFee, decimal bookPrice);
}
