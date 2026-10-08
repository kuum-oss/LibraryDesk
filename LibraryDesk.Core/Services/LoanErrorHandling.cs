using LibraryDesk.Core.Abstractions;
using LibraryDesk.Core.Domain;
using LibraryDesk.Core.Errors;

namespace LibraryDesk.Core.Services;

/// <summary>Демонструє виправлені способи завантаження, скасування та перевірки даних.</summary>
public sealed class LoanErrorHandling
{
    private readonly ILoanRepository _repository;

    /// <summary>Створює обробник із підставленим сховищем.</summary>
    /// <param name="repository">Сховище формулярів.</param>
    public LoanErrorHandling(ILoanRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>Завантажує обов'язковий для операції формуляр.</summary>
    /// <param name="loanId">Ідентифікатор формуляра.</param>
    /// <returns>Знайдений формуляр.</returns>
    public Loan LoadRequired(int loanId)
    {
        return _repository.GetById(loanId)
            ?? throw new DomainRuleException(
                "loan.exists",
                $"Формуляр {loanId} не знайдено.");
    }

    /// <summary>Скасовує формуляр, зберігаючи початкову причину збою сховища.</summary>
    /// <param name="loanId">Ідентифікатор формуляра.</param>
    /// <param name="reason">Причина скасування.</param>
    public void Cancel(int loanId, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        try
        {
            Loan loan = LoadRequired(loanId);
            loan.Cancel();
        }
        catch (IOException exception)
        {
            throw new DomainRuleException(
                "loan.storage",
                $"Не вдалося скасувати формуляр {loanId} через збій сховища.",
                exception);
        }
    }

    /// <summary>Рахує додатні цілі значення без винятків як керування потоком.</summary>
    /// <param name="rows">Неперевірені рядки.</param>
    /// <returns>Кількість коректних додатних значень.</returns>
    public static int CountValidDays(IEnumerable<string> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);
        int valid = 0;
        foreach (string row in rows)
        {
            if (int.TryParse(row, out int days) && days > 0)
            {
                valid++;
            }
        }

        return valid;
    }
}
