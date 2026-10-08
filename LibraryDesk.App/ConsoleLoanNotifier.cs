using LibraryDesk.Core.Abstractions;
using LibraryDesk.Core.Domain;

namespace LibraryDesk.App;

/// <summary>Показує сповіщення про видачу в консолі.</summary>
public sealed class ConsoleLoanNotifier : INotifier
{
    /// <inheritdoc />
    public void Notify(Reader reader, Loan loan, decimal total)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(loan);
        Console.WriteLine($"Сповіщення для {reader.Email}: формуляр № {loan.Id}, сума {total:0.00}");
    }
}
