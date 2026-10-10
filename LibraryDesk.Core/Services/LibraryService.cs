using LibraryDesk.Core.Abstractions;
using LibraryDesk.Core.Domain;
using LibraryDesk.Core.Errors;
using LibraryDesk.Core.Pricing;

namespace LibraryDesk.Core.Services;

/// <summary>Виконує сценарії каталогу, видачі, повернення, пошуку та звіту.</summary>
public sealed class LibraryService
{
    private const string RegisterBookScenario = "UC-01";
    private const string RegisterReaderScenario = "UC-02";
    private const string IssueBookScenario = "UC-03";
    private const string ReturnBookScenario = "UC-04";
    private const string SearchLoansScenario = "UC-05";
    private const string DebtorReportScenario = "UC-06";

    private readonly ILibraryRepository _repository;
    private readonly IClock _clock;
    private readonly ILateFeePolicy _lateFee;
    private readonly IScenarioLogger _logger;

    /// <summary>Створює прикладний сервіс із замінними зовнішніми залежностями.</summary>
    /// <param name="repository">Сховище стану бібліотеки.</param>
    /// <param name="clock">Джерело поточного часу.</param>
    /// <param name="lateFee">Політика пені.</param>
    /// <param name="logger">Журнал сценаріїв.</param>
    public LibraryService(
        ILibraryRepository repository,
        IClock clock,
        ILateFeePolicy lateFee,
        IScenarioLogger logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _lateFee = lateFee ?? throw new ArgumentNullException(nameof(lateFee));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Реєструє нову книгу або поповнює наявну.</summary>
    /// <param name="request">Неперевірені дані з межі застосунку.</param>
    /// <returns>Книга або зрозуміла помилка вводу.</returns>
    public Result<Book> RegisterBook(RegisterBookRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        _logger.Write(ScenarioLogLevel.Information, RegisterBookScenario, "Початок реєстрації книги.");
        string? error = ValidateBook(request);
        if (error is not null)
        {
            return Failure<Book>(RegisterBookScenario, error);
        }

        Book? existing = _repository.FindBook(request.Isbn);
        Book book = existing ?? CreateBook(request);
        if (existing is null)
        {
            _repository.AddBook(book);
        }
        else
        {
            existing.Restock(request.Copies);
        }

        Save(RegisterBookScenario);
        return Success(RegisterBookScenario, book, "Книгу збережено.");
    }

    /// <summary>Реєструє нового читача.</summary>
    /// <param name="request">Неперевірені дані з межі застосунку.</param>
    /// <returns>Читач або зрозуміла помилка вводу.</returns>
    public Result<Reader> RegisterReader(RegisterReaderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        _logger.Write(ScenarioLogLevel.Information, RegisterReaderScenario, "Початок реєстрації читача.");
        string? error = ValidateReader(request);
        if (error is not null)
        {
            return Failure<Reader>(RegisterReaderScenario, error);
        }

        if (_repository.FindReader(request.Id) is not null)
        {
            return Failure<Reader>(RegisterReaderScenario, "Читач із таким ідентифікатором уже існує.");
        }

        Reader reader = new(request.Id, request.FullName, request.Email, request.HasActiveMembership);
        _repository.AddReader(reader);
        Save(RegisterReaderScenario);
        return Success(RegisterReaderScenario, reader, "Читача збережено.");
    }

    /// <summary>Видає доступну книгу читачеві з активним абонементом.</summary>
    /// <param name="request">Ідентифікатори нового формуляра, читача і книги.</param>
    /// <returns>Строк повернення або зрозуміла помилка запиту.</returns>
    public Result<IssueBookResult> IssueBook(IssueBookRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        _logger.Write(ScenarioLogLevel.Information, IssueBookScenario, "Початок видачі книги.");
        string? error = ValidateIssueRequest(request);
        if (error is not null)
        {
            return Failure<IssueBookResult>(IssueBookScenario, error);
        }

        Reader? reader = _repository.FindReader(request.ReaderId);
        Book? book = _repository.FindBook(request.Isbn);
        error = ValidateIssueState(request, reader, book);
        if (error is not null)
        {
            return Failure<IssueBookResult>(IssueBookScenario, error);
        }

        Loan loan = CreateActiveLoan(request, reader!, book!);
        book!.LendCopy();
        _repository.AddLoan(loan);
        Save(IssueBookScenario);
        IssueBookResult result = new(loan.Id, loan.DueOn, loan.Status);
        return Success(IssueBookScenario, result, $"Формуляр {loan.Id} активовано.");
    }

    /// <summary>Приймає повернення і розраховує пеню.</summary>
    /// <param name="request">Номер формуляра і фактична дата повернення.</param>
    /// <returns>Кількість днів і сума пені або помилка запиту.</returns>
    public Result<ReturnBookResult> ReturnBook(ReturnBookRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        _logger.Write(ScenarioLogLevel.Information, ReturnBookScenario, "Початок повернення книги.");
        if (request.LoanId <= 0)
        {
            return Failure<ReturnBookResult>(ReturnBookScenario, "Ідентифікатор формуляра має бути додатним.");
        }

        Loan? loan = _repository.FindLoan(request.LoanId);
        string? error = ValidateReturnRequest(request, loan);
        if (error is not null)
        {
            return Failure<ReturnBookResult>(ReturnBookScenario, error);
        }

        int overdueDays = LoanTermsPolicy.OverdueDays(loan!.DueOn, request.ReturnedOn);
        decimal fee = CalculateFee(loan, overdueDays);
        EnsureBooksExist(loan);
        loan.Return();
        ReturnCopies(loan);
        Save(ReturnBookScenario);
        ReturnBookResult result = new(loan.Id, overdueDays, fee);
        return Success(ReturnBookScenario, result, $"Формуляр {loan.Id} повернуто.");
    }

    /// <summary>Шукає формуляри за читачем і станом та детерміновано сортує результат.</summary>
    /// <param name="readerId">Необов'язковий додатний ідентифікатор читача.</param>
    /// <param name="status">Необов'язковий стан формуляра.</param>
    /// <returns>Незмінний відсортований список.</returns>
    public IReadOnlyList<Loan> SearchLoans(int? readerId, LoanStatus? status)
    {
        if (readerId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(readerId));
        }

        _logger.Write(ScenarioLogLevel.Information, SearchLoansScenario, "Виконано пошук формулярів.");
        IEnumerable<Loan> query = _repository.GetLoans();
        if (readerId.HasValue)
        {
            query = query.Where(loan => loan.ReaderId == readerId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(loan => loan.Status == status.Value);
        }

        return Array.AsReadOnly(query.OrderBy(loan => loan.DueOn).ThenBy(loan => loan.Id).ToArray());
    }

    /// <summary>Формує рядки звіту «Боржники на дату».</summary>
    /// <param name="asOf">Дата, станом на яку визначається прострочення.</param>
    /// <returns>Рядки за спаданням прострочення та номером формуляра.</returns>
    public IReadOnlyList<DebtorReportRow> BuildDebtorReport(DateOnly asOf)
    {
        _logger.Write(ScenarioLogLevel.Information, DebtorReportScenario, "Початок формування звіту.");
        List<DebtorReportRow> rows = new();
        foreach (Loan loan in OverdueLoans(asOf))
        {
            Reader? reader = _repository.FindReader(loan.ReaderId);
            if (reader is null)
            {
                _logger.Write(ScenarioLogLevel.Error, DebtorReportScenario, $"Немає читача для формуляра {loan.Id}.");
                continue;
            }

            int days = LoanTermsPolicy.OverdueDays(loan.DueOn, asOf);
            rows.Add(CreateDebtorRow(loan, reader, days));
        }

        DebtorReportRow[] ordered = rows.OrderByDescending(row => row.OverdueDays).ThenBy(row => row.LoanId).ToArray();
        _logger.Write(ScenarioLogLevel.Information, DebtorReportScenario, $"Сформовано рядків: {ordered.Length}.");
        return Array.AsReadOnly(ordered);
    }

    private static string? ValidateBook(RegisterBookRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Isbn) || string.IsNullOrWhiteSpace(request.Title))
        {
            return "ISBN і назва книги є обов'язковими.";
        }

        if (request.DailyFee < 0m || request.ReplacementPrice <= 0m || request.Copies <= 0)
        {
            return "Тариф не може бути від'ємним, а вартість і кількість мають бути додатними.";
        }

        return null;
    }

    private static string? ValidateReader(RegisterReaderRequest request)
    {
        if (request.Id <= 0 || string.IsNullOrWhiteSpace(request.FullName))
        {
            return "Ідентифікатор має бути додатним, а ім'я — непорожнім.";
        }

        return string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@', StringComparison.Ordinal)
            ? "Електронна адреса має містити символ @."
            : null;
    }

    private static string? ValidateIssueRequest(IssueBookRequest request)
    {
        if (request.LoanId <= 0 || request.ReaderId <= 0)
        {
            return "Ідентифікатори формуляра й читача мають бути додатними.";
        }

        return string.IsNullOrWhiteSpace(request.Isbn) ? "ISBN є обов'язковим." : null;
    }

    private string? ValidateIssueState(IssueBookRequest request, Reader? reader, Book? book)
    {
        if (_repository.FindLoan(request.LoanId) is not null)
        {
            return "Формуляр із таким ідентифікатором уже існує.";
        }

        if (reader is null || book is null)
        {
            return "Читача або книгу не знайдено.";
        }

        if (!reader.HasActiveMembership || book.AvailableCopies == 0)
        {
            return "Потрібен активний абонемент і доступний примірник.";
        }

        return null;
    }

    private static string? ValidateReturnRequest(ReturnBookRequest request, Loan? loan)
    {
        if (loan is null)
        {
            return "Формуляр не знайдено.";
        }

        DateOnly issuedOn = DateOnly.FromDateTime(loan.IssuedAt.Date);
        return request.ReturnedOn < issuedOn ? "Дата повернення не може передувати даті видачі." : null;
    }

    private static Book CreateBook(RegisterBookRequest request)
        => new(request.Isbn, request.Title, request.DailyFee, request.Copies, request.ReplacementPrice);

    private Loan CreateActiveLoan(IssueBookRequest request, Reader reader, Book book)
    {
        DateTimeOffset issuedAt = _clock.Now;
        DateOnly issuedOn = DateOnly.FromDateTime(issuedAt.Date);
        Loan loan = new(request.LoanId, reader.Id, issuedAt, LoanTermsPolicy.DueOn(issuedOn));
        loan.AddItem(new LoanItem(book.Isbn, LoanTermsPolicy.StandardLoanDays, book.RentalFee, book.ReplacementPrice));
        loan.Issue();
        return loan;
    }

    private decimal CalculateFee(Loan loan, int overdueDays)
        => loan.Items.Sum(item => _lateFee.Calculate(overdueDays, item.DailyRate, item.ReplacementPrice));

    private void ReturnCopies(Loan loan)
    {
        foreach (LoanItem item in loan.Items)
        {
            Book book = _repository.FindBook(item.Isbn)!;
            book.ReturnCopy();
        }
    }

    private void EnsureBooksExist(Loan loan)
    {
        foreach (LoanItem item in loan.Items)
        {
            if (_repository.FindBook(item.Isbn) is null)
            {
                throw new InvalidDataException($"Книгу {item.Isbn} з формуляра {loan.Id} не знайдено.");
            }
        }
    }

    private IEnumerable<Loan> OverdueLoans(DateOnly asOf)
        => _repository.GetLoans().Where(
            loan => loan.Status is LoanStatus.Active or LoanStatus.Overdue && loan.DueOn < asOf);

    private DebtorReportRow CreateDebtorRow(Loan loan, Reader reader, int overdueDays)
        => new()
        {
            LoanId = loan.Id,
            ReaderId = reader.Id,
            ReaderName = reader.FullName,
            DueOn = loan.DueOn,
            OverdueDays = overdueDays,
            LateFee = CalculateFee(loan, overdueDays),
        };

    private void Save(string scenario)
    {
        try
        {
            _repository.Save();
        }
        catch (IOException exception)
        {
            _logger.Write(ScenarioLogLevel.Error, scenario, $"Помилка збереження: {exception.GetType().Name}.");
            throw;
        }
    }

    private Result<T> Failure<T>(string scenario, string error)
    {
        _logger.Write(ScenarioLogLevel.Warning, scenario, error);
        return Result.Fail<T>(error);
    }

    private Result<T> Success<T>(string scenario, T value, string message)
    {
        _logger.Write(ScenarioLogLevel.Information, scenario, message);
        return Result.Ok(value);
    }
}
