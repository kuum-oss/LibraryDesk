using LibraryDesk.Core.Domain;

namespace LibraryDesk.Core.Abstractions;

/// <summary>Сповіщення читача про зареєстровану видачу.</summary>
public interface INotifier
{
    /// <summary>Повідомляє читача про суму видачі.</summary>
    /// <param name="reader">Одержувач повідомлення.</param>
    /// <param name="loan">Зареєстрований формуляр.</param>
    /// <param name="total">Вартість прокату.</param>
    void Notify(Reader reader, Loan loan, decimal total);
}
