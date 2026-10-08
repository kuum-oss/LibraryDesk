"""Generate the verified Ukrainian Lab 5 PDF report."""

from pathlib import Path
import textwrap
from xml.sax.saxutils import escape

from PIL import Image as PillowImage
from PIL import ImageDraw, ImageFont
from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import (
    Image,
    PageBreak,
    Paragraph,
    Preformatted,
    SimpleDocTemplate,
    Spacer,
    Table,
    TableStyle,
)


ROOT = Path(__file__).resolve().parents[2]
EVIDENCE = Path(__file__).resolve().parent / "evidence"
TMP = ROOT / "tmp" / "pdfs" / "lab05"
OUT = ROOT / "output" / "pdf" / "Звіт_ЛР5_LibraryDesk.pdf"
FONT_DIR = Path("/System/Library/Fonts/Supplemental")

pdfmetrics.registerFont(TTFont("Verdana", str(FONT_DIR / "Verdana.ttf")))
pdfmetrics.registerFont(TTFont("Verdana-Bold", str(FONT_DIR / "Verdana Bold.ttf")))
pdfmetrics.registerFont(TTFont("Andale", str(FONT_DIR / "Andale Mono.ttf")))
pdfmetrics.registerFontFamily("Verdana", normal="Verdana", bold="Verdana-Bold")

INK = colors.HexColor("#172433")
BLUE = colors.HexColor("#173F6D")
PALE = colors.HexColor("#EEF4F8")
LINE = colors.HexColor("#C4D2DD")
SUBTLE = colors.HexColor("#5A6976")
GREEN = colors.HexColor("#E8F3EC")

styles = {
    "cover": ParagraphStyle(
        "cover", fontName="Verdana-Bold", fontSize=21, leading=29,
        alignment=TA_CENTER, textColor=BLUE, spaceAfter=22,
    ),
    "subtitle": ParagraphStyle(
        "subtitle", fontName="Verdana", fontSize=12.5, leading=19,
        alignment=TA_CENTER, textColor=INK, spaceAfter=15,
    ),
    "h1": ParagraphStyle(
        "h1", fontName="Verdana-Bold", fontSize=16, leading=22,
        textColor=BLUE, spaceAfter=13,
    ),
    "h2": ParagraphStyle(
        "h2", fontName="Verdana-Bold", fontSize=11, leading=16,
        textColor=BLUE, spaceBefore=7, spaceAfter=5,
    ),
    "body": ParagraphStyle(
        "body", fontName="Verdana", fontSize=10.4, leading=15.5,
        textColor=INK, spaceAfter=7,
    ),
    "small": ParagraphStyle(
        "small", fontName="Verdana", fontSize=9, leading=13,
        textColor=INK, spaceAfter=5,
    ),
    "cell": ParagraphStyle(
        "cell", fontName="Verdana", fontSize=7.6, leading=10.8,
        textColor=INK,
    ),
    "cellhead": ParagraphStyle(
        "cellhead", fontName="Verdana-Bold", fontSize=7.7, leading=11,
        textColor=colors.white,
    ),
    "caption": ParagraphStyle(
        "caption", fontName="Verdana", fontSize=8, leading=11.5,
        textColor=SUBTLE, spaceBefore=4, spaceAfter=9,
    ),
    "code": ParagraphStyle(
        "code", fontName="Andale", fontSize=7.8, leading=10.8,
        textColor=INK,
    ),
}


def para(text, style="body"):
    return Paragraph(text, styles[style])


def heading(number, title):
    return para(f"{number}. {title}", "h1")


def report_table(headers, rows, widths, font="cell"):
    content = [[para(escape(str(value)), "cellhead") for value in headers]]
    content.extend([[para(str(value), font) for value in row] for row in rows])
    result = Table(content, colWidths=widths, repeatRows=1, hAlign="LEFT")
    result.setStyle(TableStyle([
        ("BACKGROUND", (0, 0), (-1, 0), BLUE),
        ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.white, PALE]),
        ("GRID", (0, 0), (-1, -1), 0.35, LINE),
        ("VALIGN", (0, 0), (-1, -1), "MIDDLE"),
        ("LEFTPADDING", (0, 0), (-1, -1), 6),
        ("RIGHTPADDING", (0, 0), (-1, -1), 6),
        ("TOPPADDING", (0, 0), (-1, -1), 5),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 5),
    ]))
    return result


def code(text):
    block = Preformatted(text.strip("\n"), styles["code"])
    box = Table([[block]], colWidths=[505], hAlign="LEFT")
    box.setStyle(TableStyle([
        ("BACKGROUND", (0, 0), (-1, -1), PALE),
        ("BOX", (0, 0), (-1, -1), 0.45, LINE),
        ("LEFTPADDING", (0, 0), (-1, -1), 9),
        ("RIGHTPADDING", (0, 0), (-1, -1), 9),
        ("TOPPADDING", (0, 0), (-1, -1), 7),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 7),
    ]))
    return box


def terminal_image(name, command, file_name, width_chars=78):
    raw_lines = (EVIDENCE / file_name).read_text(encoding="utf-8").strip().splitlines()
    wrapped = []
    for line in raw_lines:
        wrapped.extend(
            textwrap.wrap(
                line,
                width=width_chars,
                subsequent_indent="  ",
                break_long_words=True,
                break_on_hyphens=False,
            ) or [""]
        )
    image_width = 1450
    row_height = 34
    image_height = 82 + row_height * (len(wrapped) + 1) + 24
    image = PillowImage.new("RGB", (image_width, image_height), "#13202B")
    draw = ImageDraw.Draw(image)
    draw.rounded_rectangle(
        (0, 0, image_width - 1, image_height - 1),
        radius=18,
        fill="#13202B",
        outline="#4D6577",
        width=2,
    )
    draw.rounded_rectangle((0, 0, image_width - 1, 56), radius=18, fill="#243746")
    for index, color in enumerate(("#F27A72", "#F2CD72", "#73D19B")):
        draw.ellipse((25 + index * 29, 21, 40 + index * 29, 36), fill=color)
    font = ImageFont.truetype(str(FONT_DIR / "Andale Mono.ttf"), 22)
    draw.text((32, 68), "$ " + command, font=font, fill="#F4D88E")
    for index, line in enumerate(wrapped):
        draw.text((32, 68 + row_height * (index + 1)), line, font=font, fill="#E5EEF4")
    TMP.mkdir(parents=True, exist_ok=True)
    image_path = TMP / f"{name}.png"
    image.save(image_path)
    return image_path


def footer(canvas, document):
    canvas.saveState()
    page_width, _ = A4
    canvas.setStrokeColor(LINE)
    canvas.line(45, 42, page_width - 45, 42)
    canvas.setFont("Verdana", 7)
    canvas.setFillColor(SUBTLE)
    canvas.drawString(45, 29, "LibraryDesk · Лабораторна робота № 5 · Варіант 2")
    canvas.drawRightString(page_width - 45, 29, f"Сторінка {document.page}")
    canvas.restoreState()


story = []

# 1. Cover
story.extend([
    Spacer(1, 86),
    para("ЗВІТ", "cover"),
    para("з лабораторної роботи № 5", "subtitle"),
    para("Обробка помилок, винятків і логування", "cover"),
    Spacer(1, 42),
    report_table(
        ["Реквізит", "Значення"],
        [
            ("Дисципліна", "Конструювання програмного забезпечення"),
            ("Проєкт", "LibraryDesk — облік видачі книг"),
            ("Варіант", "№ 2"),
            ("Виконав", "Гордєєв Дмитро"),
            ("Група", "ІПЗ"),
            ("Дата", "8 жовтня 2026 року"),
        ],
        [145, 360],
        "small",
    ),
    Spacer(1, 98),
    para("Гілка репозиторію: lab05-error-handling", "subtitle"),
    PageBreak(),
])

# 2. Purpose and source data
story.extend([
    heading("1", "Мета роботи"),
    para(
        "Мета роботи — розділити очікувані помилки введення, порушення правил "
        "предметної області та збої зовнішнього середовища. У застосунку побудовано "
        "барикаду між текстовим інтерфейсом і типізованим ядром, додано Result&lt;T&gt; "
        "та DomainRuleException. Також виправлено антипатерни обробки винятків, "
        "підключено структуроване логування і гарантоване звільнення файлового ресурсу."
    ),
    heading("2", "Вихідні дані"),
    para(
        "LibraryDesk — .NET 8 застосунок для обліку видачі книг. Документом є Loan, "
        "позицією документа — LoanItem, учасником — Reader, а об'єктом обліку — Book. "
        "Продовжено код лабораторних робіт № 1–4 без створення нового рішення."
    ),
    report_table(
        ["Тип", "Призначення", "Ключові правила"],
        [
            ("Loan", "Формуляр видачі", "Стани Draft, Active, Overdue, Returned, Cancelled, Lost"),
            ("LoanItem", "Позиція формуляра", "ISBN не порожній; Days > 0; DailyRate ≥ 0"),
            ("Reader", "Читач бібліотеки", "Id > 0; ім'я і пошта заповнені"),
            ("LoanService", "Прикладні сценарії", "DI, предметні винятки та журналювання"),
        ],
        [100, 155, 250],
    ),
    para(
        "Барикада проходить між LibraryDesk.App та LibraryDesk.Core. Зовні ядра "
        "перебувають рядки, культура введення і повторні запити; усередині — лише "
        "перевірені LoanItem та інші типізовані об'єкти.",
        "small",
    ),
    PageBreak(),
])

# 3. Error classification
story.extend([
    heading("3", "Завдання 1. Класифікація помилок"),
    report_table(
        ["№", "Ситуація", "Клас", "Місце", "Стратегія", "Повідомлення"],
        [
            (1, "«два» замість кількості", "Користувача", "LoanItemParser", "Result.Fail; повтор", "Кількість — ціле число > 0"),
            (2, "Порожній ISBN", "Користувача", "Парсер / каталог", "Result.Fail; без змін", "Вкажіть ISBN книги"),
            (3, "Некоректний тариф", "Користувача", "LoanItemParser", "Result.Fail; пояснення", "Тариф — невід'ємне число"),
            (4, "null-залежність сервісу", "Програміста", "LoanService.ctor", "ArgumentNullException", "Внутрішня помилка INIT-01"),
            (5, "Return для Draft", "Програміста", "Loan.Return", "DomainRuleException", "Внутрішня помилка STATE-01"),
            (6, "Від'ємна вартість", "Програміста", "LoanService.TotalOf", "Error; припинити", "Внутрішня помилка CALC-01"),
            (7, "Файл аудиту зайнятий", "Середовища", "FileAuditLog", "IOException на межі", "Журнал зайнятий; повторіть"),
            (8, "Немає прав на запис", "Середовища", "FileAuditLog", "Повідомити; код", "Немає прав на журнал"),
            (9, "Збій сховища", "Середовища", "Repository / Register", "Error; throw зі стеком", "Формуляр не збережено"),
        ],
        [20, 103, 68, 82, 107, 125],
    ),
    para(
        "Таблиця 1. Повна класифікація міститься у docs/errors.md разом із повідомленнями "
        "користувачеві. Стратегія навмисно різна: користувацький ввід є очікуваною "
        "гілкою, помилка програміста має виявлятися швидко, а збій середовища потребує "
        "контекстного журналу на межі застосунку.",
        "caption",
    ),
    PageBreak(),
])

# 4. Barricade
story.extend([
    heading("4", "Завдання 2. Барикада і валідація"),
    para("До рефакторингу ядро могло б саме розбирати текстові значення:"),
    code(
        """// БУЛО: сирі рядки потрапляють у ядро
public void AddItem(int loanId, string isbn,
                    string days, string rate)
{
    int parsedDays = int.Parse(days);
    decimal parsedRate = decimal.Parse(rate);
    AddItem(loanId,
        new LoanItem(isbn, parsedDays, parsedRate));
}"""
    ),
    para("Лістинг 1 — стан без барикади", "caption"),
    code(
        """// СТАЛО: розбір виконується у LibraryDesk.App
public static Result<LoanItem> Parse(
    string isbn, string days, string dailyRate)
{
    if (!int.TryParse(days, NumberStyles.Integer,
        CultureInfo.CurrentCulture, out int parsedDays)
        || parsedDays <= 0)
        return Result.Fail<LoanItem>(
            "Кількість днів має бути цілим числом більше нуля.");

    if (!decimal.TryParse(dailyRate, NumberStyles.Number,
        CultureInfo.CurrentCulture, out decimal parsedRate)
        || parsedRate < 0m)
        return Result.Fail<LoanItem>("Некоректний тариф.");

    return Result.Ok(
        new LoanItem(isbn.Trim(), parsedDays, parsedRate));
}"""
    ),
    para("Лістинг 2 — скорочений LoanItemParser після побудови барикади", "caption"),
    para(
        "Метод Loan.AddItem приймає готовий LoanItem і не знає про формат консолі. "
        "Десятковий роздільник визначається CultureInfo.CurrentCulture. Перевірка "
        "продукційних каталогів ядра не знаходить int.Parse або decimal.Parse.",
        "small",
    ),
    PageBreak(),
])

# 5. Exception and Result
story.extend([
    heading("5", "Завдання 3–4. DomainRuleException і Result"),
    code(
        """public class DomainRuleException : Exception
{
    public DomainRuleException(string rule, string message)
        : base(message) => Rule = rule;

    public DomainRuleException(string rule, string message,
                               Exception innerException)
        : base(message, innerException) => Rule = rule;

    public string Rule { get; }
}"""
    ),
    para("Лістинг 3 — предметний виняток з ідентифікатором правила", "caption"),
    code(
        """public readonly struct Result<T>
{
    internal Result(bool ok, T? value, string error)
    {
        IsSuccess = ok; Value = value; Error = error;
    }
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string Error { get; }
}

public static class Result
{
    public static Result<T> Ok<T>(T value) =>
        new(true, value, string.Empty);
    public static Result<T> Fail<T>(string error) =>
        new(false, default, error);
}"""
    ),
    para("Лістинг 4 — тип результату для очікуваних помилок", "caption"),
    code(
        """public Loan GetRequiredLoan(int loanId)
{
    Loan? loan = _repository.GetById(loanId);
    if (loan is null)
        throw new DomainRuleException(
            "loan.exists", $"Формуляр {loanId} не знайдено.");
    return loan;
}"""
    ),
    para("Лістинг 5 — null перетворюється на виняток у бізнес-сценарії", "caption"),
    report_table(
        ["Метод", "Рішення", "Причина"],
        [
            ("ILoanRepository.GetById", "Залишити Loan?", "Відсутність запису — нормальний результат пошуку"),
            ("InMemoryLoanRepository.GetById", "Залишити Loan?", "Реалізація відповідає контракту репозиторію"),
            ("LoanService.GetRequiredLoan", "DomainRuleException", "Операція вимагає наявного документа"),
        ],
        [160, 130, 215],
    ),
    para(
        "Result&lt;T&gt; використано для очікуваних помилок введення. Він містить "
        "IsSuccess, Value та Error, тому консольний шар показує пояснення й повторює "
        "спробу без аварійного завершення.",
    ),
    PageBreak(),
])

# 6. Antipatterns
story.extend([
    heading("6", "Завдання 5. Виправлення антипатернів"),
    report_table(
        ["Антипатерн", "Місце до", "Заміна", "Усунутий наслідок"],
        [
            ("Порожній catch", "Before.Load", "Не перехоплювати", "Збій не зникає без сліду"),
            ("catch (Exception)", "Before.Cancel", "catch (IOException)", "Помилки програміста не маскуються"),
            ("throw exception", "Before.Cancel", "innerException / throw", "Початковий стек збережено"),
            ("Parse у try/catch", "Before.CountValidDays", "int.TryParse", "Ввід не створює виняток"),
        ],
        [100, 115, 125, 165],
    ),
    para("Таблиця 2 — відповідність стану «до» та фінального виправлення", "caption"),
    code(
        """// ДО: широкий catch і втрата стека
catch (Exception exception)
{
    Console.WriteLine("Щось пішло не так");
    throw exception;
}

// ПІСЛЯ: конкретний збій і збережена причина
catch (IOException exception)
{
    throw new DomainRuleException(
        "loan.storage",
        $"Не вдалося скасувати формуляр {loanId}.",
        exception);
}"""
    ),
    para("Лістинг 6 — виправлення широкого перехоплення і втрати стека", "caption"),
    code(
        """// ДО: виняток керує звичайною гілкою
try { int days = int.Parse(row); }
catch (FormatException) { }

// ПІСЛЯ: очікувана перевірка без винятку
if (int.TryParse(row, out int days) && days > 0)
    valid++;"""
    ),
    para("Лістинг 7 — заміна винятку як керування потоком", "caption"),
    para(
        "Погана реалізація збережена комітом a503ce1 «antipatterns: before», після "
        "чого файл замінено виправленим LoanErrorHandling. Єдиний catch (Exception) "
        "залишився у Program.cs як остання межа процесу.",
        "small",
    ),
    PageBreak(),
])

# 7. Logging
run_image = terminal_image(
    "lab05-run",
    "dotnet run --project LibraryDesk.App --no-build",
    "run.txt",
)
story.extend([
    heading("7", "Завдання 6. Структуроване логування"),
    para(
        "LoanService отримує ILogger&lt;LoanService&gt; через конструктор. Повідомлення "
        "генеруються LoggerMessage і мають іменовані поля LoanId, Total, Rule та Status. "
        "BeginLoanScope додає контекст Loan:{LoanId} до всієї операції. У коді є "
        "рівні Debug, Information, Warning та Error; Critical використано у верхньому "
        "обробнику непередбачених збоїв."
    ),
    code(
        """using IDisposable? scope =
    LoanServiceLog.BeginLoanScope(_logger, loan.Id);
LoanServiceLog.RegisterStarting(
    _logger, loan.Id, total);             // Debug
LoanServiceLog.ZeroTotal(
    _logger, loan.Id);                    // Warning
LoanServiceLog.StorageFailure(
    _logger, exception, loan.Id);         // Error
LoanServiceLog.Registered(
    _logger, loan.Id, total);             // Information"""
    ),
    para("Лістинг 8 — рівні та контекст журналу", "caption"),
    Image(str(run_image), width=505, height=505 * PillowImage.open(run_image).height / 1450),
    para("Знімок екрана 1 — помилка введення, повтор і структурований журнал", "caption"),
    PageBreak(),
])

# 8. Resources
story.extend([
    heading("8", "Завдання 7. using та IDisposable"),
    code(
        """public sealed class FileAuditLog : IDisposable
{
    private readonly StreamWriter _writer;
    private bool _disposed;

    public void Write(string operation, int loanId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _writer.WriteLine(
            $"{DateTimeOffset.Now:O};{operation};{loanId}");
        _writer.Flush();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _writer.Flush();
        _writer.Dispose();
        _disposed = true;
    }
}"""
    ),
    para("Лістинг 9 — скорочений FileAuditLog", "caption"),
    para(
        "У Program.cs журнал створюється через using. Навіть якщо наступна операція "
        "кине виняток, компілятор перетворить using на try/finally і викличе Dispose. "
        "Тест нижче навмисно перериває блок, а потім відкриває файл ексклюзивно."
    ),
    code(
        """Assert.Throws<InvalidOperationException>(
    () => WriteAndFail(path));

using FileStream reopened = new(
    path, FileMode.Open, FileAccess.ReadWrite,
    FileShare.None);
Assert.True(reopened.Length > 0);"""
    ),
    para("Лістинг 10 — перевірка звільнення файла після винятку", "caption"),
    report_table(
        ["Властивість", "Результат"],
        [
            ("Власник ресурсу", "FileAuditLog володіє StreamWriter"),
            ("Гарантія", "Dispose викликається при нормальному виході та винятку"),
            ("Повторне Dispose", "Безпечне завдяки прапорцю _disposed"),
            ("Перевірка", "Файл повторно відкривається з FileShare.None"),
        ],
        [170, 335],
    ),
    PageBreak(),
])

# 9. Verification and history
build_image = terminal_image("lab05-build", "dotnet build LibraryDesk.sln", "build.txt")
test_image = terminal_image("lab05-test", "dotnet test LibraryDesk.sln --no-build", "test.txt")
commits_image = terminal_image("lab05-commits", "git log --oneline main..lab05-error-handling", "commits.txt")
story.extend([
    heading("9", "Перевірка та історія комітів"),
    Image(str(build_image), width=505, height=505 * PillowImage.open(build_image).height / 1450),
    para("Знімок екрана 2 — збірка без попереджень і помилок", "caption"),
    Image(str(test_image), width=505, height=505 * PillowImage.open(test_image).height / 1450),
    para("Знімок екрана 3 — усі 14 тестів пройдено", "caption"),
    Image(str(commits_image), width=505, height=505 * PillowImage.open(commits_image).height / 1450),
    para("Знімок екрана 4 — окремі тематичні коміти завдань 1–7", "caption"),
    PageBreak(),
])

# 10. Conclusions
story.extend([
    heading("10", "Висновки"),
    para(
        "Найчисленнішими в LibraryDesk виявилися помилки користувацького введення "
        "та порушення станів формуляра. Найскладніше було помітити втрату початкового "
        "стека через throw exception, оскільки код зовні виглядав як повторне передавання "
        "тієї самої помилки. Барикада зробила межу застосунку явною: рядки перевіряються "
        "у LibraryDesk.App, а ядро отримує готові значення. Result&lt;T&gt; спростив нормальну "
        "реакцію на помилкове введення, тоді як DomainRuleException надав порушенням "
        "правил стабільні ідентифікатори. Структуроване логування додало придатні для "
        "пошуку поля та контекст однієї операції. using і IDisposable гарантують "
        "звільнення файла незалежно від способу завершення блоку. Після змін код став "
        "передбачуванішим і простішим для діагностики."
    ),
    para("Підсумок виконання", "h2"),
    report_table(
        ["Критерій", "Стан"],
        [
            ("9 ситуацій, по 3 на кожен клас", "Виконано"),
            ("Барикада та типізоване ядро", "Виконано"),
            ("DomainRuleException і Result", "Виконано"),
            ("4 антипатерни та стан «до»", "Виконано"),
            ("4 рівні логування та BeginScope", "Виконано"),
            ("using, IDisposable і тест після винятку", "Виконано"),
            ("Build: 0 warnings / 0 errors", "Виконано"),
            ("Tests: 14 / 14", "Виконано"),
        ],
        [365, 140],
    ),
    Spacer(1, 20),
    para("Репозиторій", "h2"),
    para("https://github.com/kuum-oss/LibraryDesk", "body"),
    para("Гілка: lab05-error-handling", "body"),
])

OUT.parent.mkdir(parents=True, exist_ok=True)
document = SimpleDocTemplate(
    str(OUT),
    pagesize=A4,
    rightMargin=45,
    leftMargin=45,
    topMargin=48,
    bottomMargin=55,
    title="Звіт до лабораторної роботи № 5 — LibraryDesk",
    author="Гордєєв Дмитро",
)
document.build(story, onFirstPage=footer, onLaterPages=footer)
print(OUT)
