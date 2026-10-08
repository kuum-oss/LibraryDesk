"""Generate the Ukrainian Lab 4 PDF report from verified local command output."""

from pathlib import Path
import textwrap
from xml.sax.saxutils import escape

from PIL import Image as PillowImage
from PIL import ImageDraw, ImageFont
from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER, TA_LEFT
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import (
    Image,
    KeepTogether,
    PageBreak,
    Paragraph,
    Preformatted,
    SimpleDocTemplate,
    Spacer,
    Table,
    TableStyle,
)


ROOT = Path(__file__).resolve().parents[2]
TMP = ROOT / "tmp" / "pdfs"
EVIDENCE = Path(__file__).resolve().parent / "evidence"
OUT = ROOT / "output" / "pdf" / "Звіт_ЛР4_LibraryDesk.pdf"
FONT_DIR = Path("/System/Library/Fonts/Supplemental")
pdfmetrics.registerFont(TTFont("Verdana", str(FONT_DIR / "Verdana.ttf")))
pdfmetrics.registerFont(TTFont("Verdana-Bold", str(FONT_DIR / "Verdana Bold.ttf")))
pdfmetrics.registerFont(TTFont("Andale", str(FONT_DIR / "Andale Mono.ttf")))
pdfmetrics.registerFontFamily("Verdana", normal="Verdana", bold="Verdana-Bold")

INK = colors.HexColor("#1B2735")
BLUE = colors.HexColor("#184E77")
PALE = colors.HexColor("#EFF5F9")
LINE = colors.HexColor("#C6D7E3")
SUBTLE = colors.HexColor("#5D6D7A")

styles = {
    "cover": ParagraphStyle("cover", fontName="Verdana-Bold", fontSize=21, leading=29,
                            alignment=TA_CENTER, textColor=BLUE, spaceAfter=24),
    "subtitle": ParagraphStyle("subtitle", fontName="Verdana", fontSize=13, leading=19,
                               alignment=TA_CENTER, textColor=INK, spaceAfter=16),
    "h1": ParagraphStyle("h1", fontName="Verdana-Bold", fontSize=16, leading=23,
                         textColor=BLUE, spaceAfter=14),
    "h2": ParagraphStyle("h2", fontName="Verdana-Bold", fontSize=11, leading=16,
                         textColor=BLUE, spaceBefore=9, spaceAfter=6),
    "body": ParagraphStyle("body", fontName="Verdana", fontSize=10.8, leading=16.2,
                           textColor=INK, spaceAfter=7),
    "small": ParagraphStyle("small", fontName="Verdana", fontSize=9.7, leading=14,
                            textColor=INK, spaceAfter=5),
    "cell": ParagraphStyle("cell", fontName="Verdana", fontSize=8.9, leading=13,
                           textColor=INK),
    "cellhead": ParagraphStyle("cellhead", fontName="Verdana-Bold", fontSize=8.9,
                               leading=13, textColor=colors.white),
    "caption": ParagraphStyle("caption", fontName="Verdana", fontSize=8.2,
                              leading=12, textColor=SUBTLE, spaceBefore=5, spaceAfter=10),
    "code": ParagraphStyle("code", fontName="Andale", fontSize=8.5, leading=12,
                           textColor=INK),
    "tinycode": ParagraphStyle("tinycode", fontName="Andale", fontSize=7.6,
                               leading=11, textColor=INK),
}


def para(text, style="body"):
    return Paragraph(text, styles[style])


def heading(number, title):
    return para(f"{number}. {title}", "h1")


def table(headers, rows, widths):
    content = [[para(escape(h), "cellhead") for h in headers]]
    content += [[para(str(cell), "cell") for cell in row] for row in rows]
    result = Table(content, colWidths=widths, repeatRows=1, hAlign="LEFT")
    result.setStyle(TableStyle([
        ("BACKGROUND", (0, 0), (-1, 0), BLUE),
        ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.white, PALE]),
        ("GRID", (0, 0), (-1, -1), 0.35, LINE),
        ("VALIGN", (0, 0), (-1, -1), "TOP"),
        ("LEFTPADDING", (0, 0), (-1, -1), 7),
        ("RIGHTPADDING", (0, 0), (-1, -1), 7),
        ("TOPPADDING", (0, 0), (-1, -1), 6),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 6),
    ]))
    return result


def code(text, tiny=False):
    block = Preformatted(text.strip("\n"), styles["tinycode" if tiny else "code"])
    box = Table([[block]], colWidths=[505], hAlign="LEFT")
    box.setStyle(TableStyle([
        ("BACKGROUND", (0, 0), (-1, -1), PALE),
        ("BOX", (0, 0), (-1, -1), 0.5, LINE),
        ("LEFTPADDING", (0, 0), (-1, -1), 9),
        ("RIGHTPADDING", (0, 0), (-1, -1), 9),
        ("TOPPADDING", (0, 0), (-1, -1), 8),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 8),
    ]))
    return box


def terminal_image(name, command, file_name):
    raw_lines = (EVIDENCE / file_name).read_text(encoding="utf-8").strip().splitlines()
    raw = [part for line in raw_lines
           for part in (textwrap.wrap(line, width=76, subsequent_indent="  ",
                                      break_long_words=True, break_on_hyphens=False)
                        if line else [""])]
    width, row_h = 1450, 35
    height = 82 + row_h * (len(raw) + 1) + 24
    image = PillowImage.new("RGB", (width, height), "#14202B")
    draw = ImageDraw.Draw(image)
    draw.rounded_rectangle((0, 0, width - 1, height - 1), radius=18,
                           fill="#14202B", outline="#4D6577", width=2)
    draw.rounded_rectangle((0, 0, width - 1, 56), radius=18, fill="#233646")
    for index, color in enumerate(("#F27A72", "#F2CD72", "#73D19B")):
        draw.ellipse((25 + index * 29, 22, 39 + index * 29, 36), fill=color)
    font = ImageFont.truetype(str(FONT_DIR / "Andale Mono.ttf"), 23)
    draw.text((32, 69), "$ " + command, font=font, fill="#F4D88E")
    for index, line in enumerate(raw):
        draw.text((32, 69 + row_h * (index + 1)), line, font=font, fill="#E5EEF4")
    TMP.mkdir(parents=True, exist_ok=True)
    image_path = TMP / f"{name}.png"
    image.save(image_path)
    return image_path


def footer(canvas, doc):
    canvas.saveState()
    width, _ = A4
    canvas.setStrokeColor(LINE)
    canvas.line(45, 42, width - 45, 42)
    canvas.setFont("Verdana", 7)
    canvas.setFillColor(SUBTLE)
    canvas.drawString(45, 29, "LibraryDesk · Лабораторна робота № 4 · Варіант 2")
    canvas.drawRightString(width - 45, 29, f"Сторінка {doc.page}")
    canvas.restoreState()


story = []

# 1. Cover
story += [Spacer(1, 88), para("ЗВІТ", "cover"),
          para("з лабораторної роботи № 4", "subtitle"),
          para("Конструювання класів: інкапсуляція<br/>та принципи SOLID", "cover"),
          Spacer(1, 42),
          table(["Реквізит", "Значення"], [
              ("Дисципліна", "Конструювання програмного забезпечення"),
              ("Проєкт", "LibraryDesk — каталог бібліотеки та видача книжок"),
              ("Варіант", "№ 2"),
              ("Виконав", "Гордєєв Дмитро Леонідович"),
              ("Група", "ІПЗ"),
              ("Дата", "8 жовтня 2026 року"),
          ], [145, 360]),
          Spacer(1, 100),
          para("Репозиторій: https://github.com/kuum-oss/LibraryDesk", "subtitle"),
          PageBreak()]

# 2. Purpose and model
story += [heading("1", "Мета та вихідні дані"),
          para("Мета роботи — спроєктувати формуляр видачі як абстрактний тип даних із перевіреними інваріантами та контрольованим життєвим циклом. Також потрібно винести зберігання, ціноутворення й сповіщення за вузькі інтерфейси та перевірити виправлення порушень SOLID тестами."),
          para("<b>Вихідні дані.</b> Продовжено .NET 8 рішення LibraryDesk з лабораторних робіт № 1–3. Документ — Loan, позиція — LoanItem, учасник — Reader, об'єкт каталогу — Book. Припущення: одна позиція означає один примірник на Days днів; ціна дорівнює Days × DailyRate; знижка обраної політики застосовується до кожної позиції; резервування примірників не входить до цієї роботи."),
          heading("2", "Завдання 1. Класова модель"),
          table(["Сутність / роль", "Незмінність", "Інваріант"], [
              ("Loan — формуляр", "Змінний агрегат", "Id &gt; 0; ReaderId &gt; 0; DueOn ≥ дата оформлення; позиції лише у Draft"),
              ("LoanItem — позиція", "sealed record", "Isbn не порожній; Days &gt; 0; DailyRate ≥ 0"),
              ("Reader — читач", "sealed record", "Id &gt; 0; FullName не порожнє; Email містить @"),
              ("Book — книга", "Змінний об'єкт", "Isbn і Title не порожні; RentalFee ≥ 0; AvailableCopies ≥ 0"),
          ], [145, 112, 248]),
          para("Стани й дозволені переходи", "h2"),
          table(["Зі стану", "До стану", "Умова"], [
              ("Draft", "Active", "Є позиції; Issue()"),
              ("Draft", "Cancelled", "До видачі; Cancel()"),
              ("Active", "Overdue", "today &gt; DueOn; MarkOverdue(today)"),
              ("Active / Overdue", "Returned", "Книги повернуто; Return()"),
              ("Active / Overdue", "Lost", "Втрату зафіксовано; MarkLost()"),
          ], [113, 113, 279]),
          para("Returned, Cancelled і Lost — кінцеві стани. Повна модель розміщена у docs/model.md.", "small"),
          PageBreak()]

# 3. Immutable types
item_code = """public sealed record LoanItem
{
  public LoanItem(string isbn, int days,
                  decimal dailyRate)
  {
    ArgumentException
      .ThrowIfNullOrWhiteSpace(isbn);
    ArgumentOutOfRangeException
      .ThrowIfNegativeOrZero(days);
    ArgumentOutOfRangeException
      .ThrowIfNegative(dailyRate);
    Isbn = isbn;
    Days = days;
    DailyRate = dailyRate;
  }
  public string Isbn { get; }
  public int Days { get; }
  public decimal DailyRate { get; }
  public decimal Amount => Days * DailyRate;
}"""
reader_code = """public sealed record Reader
{
  public Reader(int id,
                string fullName, string email,
                bool hasActiveMembership)
  {
    ArgumentOutOfRangeException
      .ThrowIfNegativeOrZero(id);
    ArgumentException
      .ThrowIfNullOrWhiteSpace(fullName);
    ArgumentException
      .ThrowIfNullOrWhiteSpace(email);
    if (!email.Contains('@',
                        StringComparison.Ordinal))
      throw new ArgumentException(
        "Пошта має містити символ @.",
        nameof(email));
    Id = id;
    FullName = fullName;
    Email = email;
    HasActiveMembership = hasActiveMembership;
  }
  public int Id { get; }
  public string FullName { get; }
  public string Email { get; }
  public bool HasActiveMembership { get; }
}"""
code_pair = Table([
    [para("LoanItem — позиція", "h2"), para("Reader — учасник", "h2")],
    [Preformatted(item_code, styles["tinycode"]),
     Preformatted(reader_code, styles["tinycode"])],
], colWidths=[252, 253])
code_pair.setStyle(TableStyle([
    ("BACKGROUND", (0, 1), (-1, 1), PALE),
    ("BOX", (0, 1), (-1, 1), 0.5, LINE),
    ("INNERGRID", (0, 1), (-1, 1), 0.5, LINE),
    ("VALIGN", (0, 0), (-1, -1), "TOP"),
    ("LEFTPADDING", (0, 1), (-1, 1), 8),
    ("TOPPADDING", (0, 1), (-1, 1), 8),
    ("BOTTOMPADDING", (0, 1), (-1, 1), 8),
]))
story += [heading("3", "Завдання 2. Незмінні типи та інваріанти"),
          para("Обидва типи — sealed record із властивостями лише для читання. Конструктор відхиляє неприпустимі значення до появи об'єкта. LoanItem.Amount обчислюється щоразу з Days та DailyRate і не може розійтися з ними. Читача й позицію не потрібно змінювати після створення, тому незмінність спрощує передачу між сервісами."),
          code_pair,
          para("Лістинг 1. Скорочені витяги з LibraryDesk.Core/Domain/LoanItem.cs і Reader.cs; рядки перенесено для сторінки.", "caption"),
          para("Для грошей використано decimal; дата повернення в агрегаті має тип DateOnly, дата оформлення — DateTimeOffset. Після цього кроку рішення збиралося без попереджень.", "small"),
          PageBreak()]

# 4. Aggregate
story += [heading("4", "Завдання 3. Інкапсуляція агрегату"),
          para("Loan створюється в стані Draft. Конструктор перевіряє Id, ReaderId і DueOn. Приватний список _items відкривається лише як IReadOnlyList; AsReadOnly не дозволяє привести його назад до List і додати позицію в обхід AddItem."),
          code("""private readonly List<LoanItem> _items = new();
public IReadOnlyList<LoanItem> Items => _items.AsReadOnly();
public LoanStatus Status { get; private set; }

public void AddItem(LoanItem item)
{
    ArgumentNullException.ThrowIfNull(item);
    EnsureStatus(LoanStatus.Draft);
    _items.Add(item);
}

public void Issue()
{
    EnsureStatus(LoanStatus.Draft);
    if (_items.Count == 0)
        throw new InvalidOperationException("Порожній формуляр");
    Status = LoanStatus.Active;
}"""),
          para("Лістинг 2. Контроль колекції та переходу Draft → Active; текст винятку скорочено.", "caption"),
          table(["Було", "Стало", "Що це дає"], [
              ("Status { get; set; }", "private set та Issue, Return, Cancel", "Клієнт не встановить довільний або зворотний стан"),
              ("AddItem одразу додавав у List", "AddItem перевіряє Draft", "Після видачі або закриття позиції не змінюються"),
              ("Items повертало List як IReadOnlyList", "Items повертає AsReadOnly", "Не можна обійти AddItem приведенням типу"),
              ("Book.AvailableCopies { get; init; }", "private set, Restock і LendCopy", "Кількість примірників не стане від'ємною"),
          ], [137, 171, 197]),
          para("Операції з файлами, консоллю чи поштою в Loan відсутні. Метод Total залишено для сумісності з попередньою моделлю; ціна з обраною політикою обчислюється сервісом.", "small"),
          PageBreak()]

# 5. DI
story += [heading("5", "Завдання 4. Інтерфейси та впровадження залежностей"),
          para("ILoanRepository містить тільки Add, GetById і GetAll. InMemoryLoanRepository зберігає документи у Dictionary, відхиляє повторний Id та повертає копію списку. IPricingPolicy має один метод PriceOf; реалізації StandardPricingPolicy і DiscountPricingPolicy не змінюють LoanService."),
          code("""public interface ILoanRepository
{
    void Add(Loan loan);
    Loan? GetById(int id);
    IReadOnlyList<Loan> GetAll();
}
public interface IPricingPolicy
{
    decimal PriceOf(LoanItem item);
}
public LoanService(ILoanRepository repository,
                   IPricingPolicy pricing, INotifier notifier)
{
    _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    _pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
    _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
}""", tiny=True),
          para("Лістинг 3. Вузькі контракти й конструктор LoanService; залежності зберігаються у readonly полях.", "caption"),
          code("""ILoanRepository repository = new InMemoryLoanRepository();
IPricingPolicy pricing = new DiscountPricingPolicy(0.05m);
INotifier notifier = new ConsoleLoanNotifier();
LoanService service = new(repository, pricing, notifier);"""),
          para("Лістинг 4. Композиційний корінь у LibraryDesk.App/Program.cs. Щоб перейти на іншу політику, достатньо змінити один рядок створення pricing. Запуск показав суму 475,00 для двох днів по 250 із знижкою 5%.", "small"),
          PageBreak()]

# 6. Audit
story += [heading("6", "Завдання 5. Аудит п'яти порушень SOLID"),
          para("Навчальний LoanManager у LegacyLab4 адаптовано з методички до бібліотечної області. Він навмисно зберігає порушення, але не викликається з робочого застосунку. Для демонстраційного коду локально приглушено лише пов'язані попередження аналізаторів."),
          table(["Принцип", "Доказ у коді", "Наслідок зміни вимог", "Засіб"], [
              ("SRP", "Place рахує, зберігає та сповіщає; BuildCsvReport пише файл", "Зміна CSV або повідомлення зачіпає клас розрахунку", "Сервіс, сховище, сповіщувач, форматувальник"),
              ("OCP", "if / else if за readerType: regular, vip, staff", "Новий тип читача змінює Place", "IPricingPolicy та окремі реалізації"),
              ("LSP", "ArchivedLoan : LegacyLoan; AddLine кидає NotSupportedException", "Archive падає під час виклику AddLine", "ArchivedLoanSnapshot містить Loan"),
              ("ISP", "ILoanStore оголошує 7 методів; менеджер використовує SaveToFile", "Інше сховище мусить реалізувати пошту й друк", "ILoanRepository та INotifier"),
              ("DIP", "Поля _store = new FileLoanStore(); _notifier = new SmtpNotifier()", "Тест ціни залежить від файлу й консолі", "Залежності через конструктор"),
          ], [65, 174, 143, 123]),
          Spacer(1, 10),
          para("<b>Числа аудиту.</b> LoanManager має 5 причин для зміни: правила ціни, збереження, сповіщення, CSV і архівування. Із 7 методів ILoanStore йому справді потрібен 1 — SaveToFile.", "body"),
          code("""LegacyLoan archived = new ArchivedLoan(loan.Id);
foreach (LegacyLoanLine line in loan.Lines)
    archived.AddLine(line);"""),
          para("Лістинг 5. Конкретне порушення LSP у LegacyLab4/LoanManager.cs.", "caption"),
          PageBreak()]

# 7. Refactor
story += [heading("7", "Завдання 6. Виправлення SOLID"),
          table(["Принцип", "Було", "Стало"], [
              ("SRP", "LoanManager.Place і BuildCsvReport", "LoanService, InMemoryLoanRepository, LoanCsvReport"),
              ("OCP", "if / else if у Place", "StandardPricingPolicy, DiscountPricingPolicy"),
              ("LSP", "ArchivedLoan : LegacyLoan", "ArchivedLoanSnapshot із приватним Loan"),
              ("ISP", "ILoanStore: 7 методів", "ILoanRepository: 3; INotifier: 1"),
              ("DIP", "new FileLoanStore, new SmtpNotifier в полях", "Інтерфейси у конструкторі LoanService"),
          ], [65, 196, 244]),
          Spacer(1, 11),
          code("""public sealed class ArchivedLoanSnapshot
{
    private readonly Loan _loan;
    public ArchivedLoanSnapshot(Loan loan, DateOnly archivedOn)
    {
        ArgumentNullException.ThrowIfNull(loan);
        if (loan.Status is not (LoanStatus.Returned
            or LoanStatus.Cancelled or LoanStatus.Lost))
            throw new InvalidOperationException("Лише завершений формуляр");
        _loan = loan;
        ArchivedOn = archivedOn;
    }
    public int Id => _loan.Id;
    public IReadOnlyList<LoanItem> Items => _loan.Items;
}""", tiny=True),
          para("Лістинг 6. Архівний перегляд не має AddItem. Вимога кінцевого стану гарантує, що вкладений Loan більше не змінюється; текст винятку скорочено.", "caption"),
          para("LoanCsvReport.BuildCsv повертає рядок без File.WriteAllText. LoanService приймає ILoanRepository, IPricingPolicy та INotifier у конструкторі; конкретні реалізації створює лише Program.cs. У робочому коді жоден сервіс не створює власне файлове сховище або сповіщувач.", "small"),
          PageBreak()]

# 8. Tests
story += [heading("8", "Завдання 7. Три тести інваріантів та коміти"),
          para("Додано рівно три тести ЛР № 4 в окремому Lab4InvariantTests. Попередні характеризаційні й регресійні тести збережено: загалом у рішенні проходить 11 тестів."),
          code("""[Fact]
public void LoanItem_WithZeroDays_ThrowsOutOfRange()
{
    Assert.Throws<ArgumentOutOfRangeException>(
        () => new LoanItem("ISBN-1", 0, 100m));
}

[Fact]
public void Loan_BeforeIssue_RejectsReturn()
{
    Loan loan = NewLoan(1);
    Assert.Throws<InvalidOperationException>(() => loan.Return());
}

[Fact]
public void LoanService_WithInjectedTenPercentPolicy_Returns180()
{
    ILoanRepository repository = new InMemoryLoanRepository();
    IPricingPolicy pricing = new DiscountPricingPolicy(0.10m);
    LoanService service = new(repository, pricing, new TestNotifier());
    Loan loan = NewLoan(2);
    loan.AddItem(new LoanItem("ISBN-2", 2, 100m));
    Assert.Equal(180m, service.TotalOf(loan));
}""", tiny=True),
          para("Лістинг 7. Три модульні тести; NewLoan і TestNotifier — локальні допоміжні типи тесту.", "caption"),
          table(["Перевірка", "Результат"], [
              ("dotnet build LibraryDesk.sln --no-restore", "Успіх; 0 попереджень; 0 помилок"),
              ("dotnet test LibraryDesk.sln --no-build --no-restore", "11 із 11 пройшли"),
              ("Фільтр Lab4InvariantTests", "3 із 3 пройшли"),
              ("dotnet format --verify-no-changes", "Успіх; змін не потрібно"),
          ], [295, 210]),
          PageBreak()]

# 9. Evidence images
run_img = terminal_image("lab4-run", "dotnet run --project LibraryDesk.App --no-build",
                         "lab4-run.txt")
test_img = terminal_image("lab4-test", "dotnet test ... --filter Lab4InvariantTests",
                          "lab4-test.txt")
log_img = terminal_image("lab4-log", "git log --oneline -8", "lab4-log.txt")
story += [heading("9", "Знімки виведення команд"),
          Image(str(run_img), width=505, height=505 * PillowImage.open(run_img).height / 1450),
          para("Рисунок 1. Запуск застосунку підтверджує оформлення формуляра, розрахунок 475,00 і CSV без запису файлу.", "caption"),
          Image(str(test_img), width=505, height=505 * PillowImage.open(test_img).height / 1450),
          para("Рисунок 2. Ізольований запуск трьох тестів ЛР № 4: усі пройшли.", "caption"),
          Image(str(log_img), width=505, height=505 * PillowImage.open(log_img).height / 1450),
          para("Рисунок 3. Історія Git: окремі коміти для моделі, типів, агрегату, DI, аудиту, виправлень і тестів.", "caption"),
          PageBreak()]

# 10. Conclusion
story += [heading("10", "Висновки"),
          para("1. У LoanItem і Reader значення тепер перевіряються в конструкторі; після створення клієнт не може зіпсувати ці об'єкти."),
          para("2. Loan приховує список позицій і дозволяє лише переходи з таблиці станів; повернення до Draft після видачі неможливе."),
          para("3. Нова політика знижки додається окремою реалізацією IPricingPolicy. Для вибору іншого правила у поточному застосунку змінюється один рядок композиційного кореня."),
          para("4. Сховище, сповіщення й форматування CSV відокремлено від розрахунку. Зміна формату звіту або способу зберігання більше не вимагає правити LoanService."),
          para("5. Архівний перегляд використовує композицію та не обіцяє операції AddItem, які він не підтримує. Три спеціальні тести підтвердили захист інваріантів і підстановку політики; усі 11 тестів рішення пройшли."),
          Spacer(1, 22),
          para("<b>Репозиторій:</b> https://github.com/kuum-oss/LibraryDesk", "body"),
          para("<b>Робоча гілка:</b> lab04-encapsulation-solid", "body"),
          para("<b>Файли реалізації:</b> LibraryDesk.Core/Domain, Abstractions, Services, Pricing, Storage, Reports; LibraryDesk.App; LibraryDesk.Tests/Lab4InvariantTests.cs.", "small")]

OUT.parent.mkdir(parents=True, exist_ok=True)
document = SimpleDocTemplate(str(OUT), pagesize=A4, rightMargin=45, leftMargin=45,
                             topMargin=52, bottomMargin=55,
                             title="Лабораторна робота № 4 — LibraryDesk",
                             author="Гордєєв Дмитро Леонідович")
document.build(story, onFirstPage=footer, onLaterPages=footer)
print(OUT)
