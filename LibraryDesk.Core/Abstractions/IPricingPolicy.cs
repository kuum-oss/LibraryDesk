using LibraryDesk.Core.Domain;

namespace LibraryDesk.Core.Abstractions;

/// <summary>Правило ціни однієї позиції формуляра.</summary>
public interface IPricingPolicy
{
    /// <summary>Обчислює вартість позиції.</summary>
    /// <param name="item">Позиція формуляра.</param>
    /// <returns>Вартість після застосування правила.</returns>
    decimal PriceOf(LoanItem item);
}
