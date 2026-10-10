using System.Globalization;
using LibraryDesk.Core.Abstractions;
using LibraryDesk.Core.Domain;

namespace LibraryDesk.App;

/// <summary>Створює відтворюваний навчальний набір без реальних персональних даних.</summary>
internal static class SampleDataSeeder
{
    private const int BookCount = 50;
    private const int ReaderCount = 20;
    private const int InitialCopies = 3;
    private const decimal BaseDailyFee = 5m;
    private const decimal BaseReplacementPrice = 500m;

    /// <summary>Заповнює повністю порожнє сховище умовними даними.</summary>
    /// <param name="repository">Сховище стану бібліотеки.</param>
    public static void Seed(ILibraryRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        if (repository.GetBooks().Count > 0 || repository.GetReaders().Count > 0)
        {
            return;
        }

        AddBooks(repository);
        AddReaders(repository);
        repository.Save();
    }

    private static void AddBooks(ILibraryRepository repository)
    {
        for (int index = 1; index <= BookCount; index++)
        {
            string suffix = index.ToString("D4", CultureInfo.InvariantCulture);
            decimal dailyFee = BaseDailyFee + index;
            decimal replacementPrice = BaseReplacementPrice + (index * 10m);
            repository.AddBook(new Book($"ISBN-{suffix}", $"Навчальна книга {index}", dailyFee, InitialCopies, replacementPrice));
        }
    }

    private static void AddReaders(ILibraryRepository repository)
    {
        for (int id = 1; id <= ReaderCount; id++)
        {
            repository.AddReader(new Reader(id, $"Тестовий читач {id}", $"reader{id}@example.com", true));
        }
    }
}
