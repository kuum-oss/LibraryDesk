using System.Globalization;
using LibraryDesk.Core.Domain;
using LibraryDesk.Core.Errors;
using LibraryDesk.Core.Services;

namespace LibraryDesk.App;

/// <summary>Перетворює консольний ввід на запити прикладного сервісу.</summary>
internal sealed class ConsoleApplication
{
    private readonly LibraryService _service;
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly Dictionary<string, Action> _actions;

    /// <summary>Створює консольну оболонку із замінними потоками.</summary>
    /// <param name="service">Прикладні сценарії LibraryDesk.</param>
    /// <param name="input">Потік команд користувача.</param>
    /// <param name="output">Потік текстового результату.</param>
    public ConsoleApplication(LibraryService service, TextReader input, TextWriter output)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _actions = new Dictionary<string, Action>(StringComparer.Ordinal)
        {
            ["1"] = RegisterBook,
            ["2"] = RegisterReader,
            ["3"] = IssueBook,
            ["4"] = ReturnBook,
            ["5"] = SearchLoans,
            ["6"] = BuildDebtorReport,
        };
    }

    /// <summary>Показує меню до явної команди завершення або кінця вводу.</summary>
    public void Run()
    {
        while (true)
        {
            ShowMenu();
            string? choice = _input.ReadLine();
            if (choice is null or "0")
            {
                _output.WriteLine("Роботу завершено.");
                return;
            }

            if (_actions.TryGetValue(choice, out Action? action))
            {
                action();
            }
            else
            {
                _output.WriteLine("Невідомий пункт. Оберіть число від 0 до 6.");
            }
        }
    }

    private void ShowMenu()
    {
        _output.WriteLine();
        _output.WriteLine("1 — зареєструвати книгу");
        _output.WriteLine("2 — зареєструвати читача");
        _output.WriteLine("3 — видати книжку");
        _output.WriteLine("4 — прийняти повернення");
        _output.WriteLine("5 — знайти формуляри");
        _output.WriteLine("6 — звіт «Боржники на дату»");
        _output.WriteLine("0 — завершити");
        _output.Write("Ваш вибір: ");
    }

    private void RegisterBook()
    {
        _output.Write("ISBN: ");
        string isbn = ReadText();
        _output.Write("Назва: ");
        string title = ReadText();
        decimal? dailyFee = ReadDecimal("Денний тариф: ");
        decimal? replacementPrice = ReadDecimal("Вартість заміни: ");
        int? copies = ReadInt("Кількість примірників: ");
        if (dailyFee is null || replacementPrice is null || copies is null)
        {
            return;
        }

        RegisterBookRequest request = new()
        {
            Isbn = isbn,
            Title = title,
            DailyFee = dailyFee.Value,
            ReplacementPrice = replacementPrice.Value,
            Copies = copies.Value,
        };
        Result<Book> result = _service.RegisterBook(request);
        _output.WriteLine(result.IsSuccess ? $"Збережено: {result.Value!.Title}." : $"Помилка: {result.Error}");
    }

    private void RegisterReader()
    {
        int? id = ReadInt("Ідентифікатор: ");
        _output.Write("Ім'я: ");
        string name = ReadText();
        _output.Write("Email: ");
        string email = ReadText();
        if (id is null)
        {
            return;
        }

        RegisterReaderRequest request = new() { Id = id.Value, FullName = name, Email = email };
        Result<Reader> result = _service.RegisterReader(request);
        _output.WriteLine(result.IsSuccess ? $"Збережено читача № {result.Value!.Id}." : $"Помилка: {result.Error}");
    }

    private void IssueBook()
    {
        int? loanId = ReadInt("Номер формуляра: ");
        int? readerId = ReadInt("Номер читача: ");
        _output.Write("ISBN: ");
        string isbn = ReadText();
        if (loanId is null || readerId is null)
        {
            return;
        }

        IssueBookRequest request = new() { LoanId = loanId.Value, ReaderId = readerId.Value, Isbn = isbn };
        Result<IssueBookResult> result = _service.IssueBook(request);
        string message = result.IsSuccess
            ? $"Книгу видано. Повернути до {result.Value!.DueOn:dd.MM.yyyy}."
            : $"Помилка: {result.Error}";
        _output.WriteLine(message);
    }

    private void ReturnBook()
    {
        int? loanId = ReadInt("Номер формуляра: ");
        DateOnly? returnedOn = ReadDate("Дата повернення (рррр-мм-дд): ");
        if (loanId is null || returnedOn is null)
        {
            return;
        }

        ReturnBookRequest request = new() { LoanId = loanId.Value, ReturnedOn = returnedOn.Value };
        try
        {
            Result<ReturnBookResult> result = _service.ReturnBook(request);
            string message = result.IsSuccess
                ? $"Повернення прийнято. Днів прострочення: {result.Value!.OverdueDays}; пеня: {result.Value.LateFee:0.00} грн."
                : $"Помилка: {result.Error}";
            _output.WriteLine(message);
        }
        catch (DomainRuleException exception)
        {
            _output.WriteLine($"Операцію заборонено: {exception.Message}");
        }
    }

    private void SearchLoans()
    {
        int? readerId = ReadOptionalInt("Номер читача (Enter — усі): ");
        LoanStatus? status = ReadOptionalStatus("Стан англійською (Enter — усі): ");
        IReadOnlyList<Loan> loans = _service.SearchLoans(readerId, status);
        if (loans.Count == 0)
        {
            _output.WriteLine("Формуляри не знайдено.");
            return;
        }

        foreach (Loan loan in loans)
        {
            _output.WriteLine($"#{loan.Id}; читач {loan.ReaderId}; до {loan.DueOn:dd.MM.yyyy}; {loan.Status}");
        }
    }

    private void BuildDebtorReport()
    {
        DateOnly? asOf = ReadDate("Дата звіту (рррр-мм-дд): ");
        if (asOf is null)
        {
            return;
        }

        IReadOnlyList<DebtorReportRow> rows = _service.BuildDebtorReport(asOf.Value);
        _output.WriteLine($"БОРЖНИКИ НА {asOf:dd.MM.yyyy}");
        if (rows.Count == 0)
        {
            _output.WriteLine("Боржників немає.");
            return;
        }

        foreach (DebtorReportRow row in rows)
        {
            _output.WriteLine($"#{row.LoanId}; {row.ReaderName}; {row.OverdueDays} дн.; {row.LateFee:0.00} грн");
        }
    }

    private string ReadText() => _input.ReadLine() ?? string.Empty;

    private int? ReadInt(string prompt)
    {
        _output.Write(prompt);
        if (int.TryParse(_input.ReadLine(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
        {
            return value;
        }

        _output.WriteLine("Помилка вводу: очікується ціле число.");
        return null;
    }

    private int? ReadOptionalInt(string prompt)
    {
        _output.Write(prompt);
        string? raw = _input.ReadLine();
        return string.IsNullOrWhiteSpace(raw) ? null : ParseOptionalInt(raw);
    }

    private int? ParseOptionalInt(string raw)
    {
        if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) && value > 0)
        {
            return value;
        }

        _output.WriteLine("Помилка вводу: ідентифікатор має бути додатним.");
        return null;
    }

    private decimal? ReadDecimal(string prompt)
    {
        _output.Write(prompt);
        string? raw = _input.ReadLine();
        if (decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value))
        {
            return value;
        }

        _output.WriteLine("Помилка вводу: використайте число з крапкою як роздільником.");
        return null;
    }

    private DateOnly? ReadDate(string prompt)
    {
        _output.Write(prompt);
        string? raw = _input.ReadLine();
        if (DateOnly.TryParseExact(raw, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly value))
        {
            return value;
        }

        _output.WriteLine("Помилка вводу: дата має формат рррр-мм-дд.");
        return null;
    }

    private LoanStatus? ReadOptionalStatus(string prompt)
    {
        _output.Write(prompt);
        string? raw = _input.ReadLine();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        if (Enum.TryParse(raw, ignoreCase: true, out LoanStatus status))
        {
            return status;
        }

        _output.WriteLine("Невідомий стан. Використайте Draft, Active, Overdue, Returned, Cancelled або Lost.");
        return null;
    }
}
