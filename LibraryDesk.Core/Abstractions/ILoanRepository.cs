using LibraryDesk.Core.Domain;

namespace LibraryDesk.Core.Abstractions;

/// <summary>Зберігання формулярів видачі без побічних обов'язків.</summary>
public interface ILoanRepository
{
    /// <summary>Додає новий формуляр.</summary>
    /// <param name="loan">Формуляр для збереження.</param>
    void Add(Loan loan);

    /// <summary>Шукає формуляр за ідентифікатором.</summary>
    /// <param name="id">Ідентифікатор формуляра.</param>
    /// <returns>Знайдений формуляр або null.</returns>
    Loan? GetById(int id);

    /// <summary>Повертає знімок списку формулярів.</summary>
    /// <returns>Копія списку формулярів.</returns>
    IReadOnlyList<Loan> GetAll();
}
