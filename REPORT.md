# ЗВІТ З ЛАБОРАТОРНОЇ РОБОТИ № 1
**Дисципліна**: Конструювання програмного забезпечення  
**Тема**: Середовище конструювання та стандарт кодування проєкту  
**Варіант № 2**: LibraryDesk (облік видачі книг у бібліотеці)  
**Виконав**: студент Гордєєв Дмитро  

---

## 1. Мета роботи
Опанувати інженерні практики розгортання та конфігурації середовища конструювання програмного забезпечення для модульного проєкту на базі .NET 8. На практиці закріпити стандарт кодування через систему контролю версій Git, машинно орієнтовані конфігурації `.editorconfig`, централізовані правила збірки `Directory.Build.props` та статичні аналізатори коду Roslyn і StyleCop, досягнувши стану збірки без жодного попередження.

---

## 2. Вихідні дані
- **Назва проєкту**: `LibraryDesk`.
- **Предметна область**: Автоматизація обліку бібліотечного фонду та видачі літератури читачам на абонемент.
- **Базові сутності та перелічення станів**:
  1. *Довідник учасників*: `Reader` (читач бібліотеки).
  2. *Довідник об'єктів*: `Book` (книга з фонду з унікальним ISBN та добовим тарифом).
  3. *Документ*: `Loan` (формуляр видачі книг користувачеві).
  4. *Позиція документа*: `LoanItem` (рядок видачі: конкретна книга за ISBN, кількість днів прокату, тариф за добу).
  5. *Стани документа*: `LoanStatus` (`Active`, `Returned`, `Overdue`, `Cancelled`, `Lost`).
- **Ухвалені припущення щодо бізнес-логіки**:
  - Кожна книга має щоденну фіксовану вартість користування (`DailyRate`).
  - Підсумкова величина формуляра видачі (`Total`) розраховується як сума вартостей усіх позицій видачі:  
    $$\text{Total} = \sum_{i=1}^{n} (\text{Days}_i \times \text{DailyRate}_i)$$
  - Логіка розрахунку повністю ізольована в бібліотеці класів і не залежить від засобів вводу-виводу.

### Таблиця відповідності ролей типів (Завдання 3)

| Роль у системі | Варіант 1 (ShopOrders) | Варіант 2 (LibraryDesk) | Призначення у варіанті 2 |
| :--- | :--- | :--- | :--- |
| **Довідник учасників** | `Customer` | `Reader` | Зберігає дані читача, номер квитка та статус абонемента |
| **Довідник об’єктів** | `Product` | `Book` | Описує книгу фонду: ISBN, назва, тариф, доступна кількість |
| **Документ** | `Order` | `Loan` | Формуляр видачі, що містить список позицій, дату та читача |
| **Позиція документа** | `OrderLine` | `LoanItem` | Окремий рядок: ISBN книги, кількість днів та добова ставка |
| **Стани документа** | `OrderStatus` | `LoanStatus` | Життєвий цикл видачі (`Active`, `Returned`, `Overdue` тощо) |

---

## 3. Хід виконання роботи

### 3.1. Завдання 1. Створення рішення .NET 8
Створено модульне рішення, що складається з бібліотеки класів предметної області `LibraryDesk.Core` та консольного застосунку `LibraryDesk.App` (цільова платформа `net8.0`).

**Виконані команди:**
```bash
dotnet new sln -n LibraryDesk --format sln
dotnet new classlib -o LibraryDesk.Core -f net8.0
dotnet new console -o LibraryDesk.App -f net8.0
dotnet sln add LibraryDesk.Core/LibraryDesk.Core.csproj
dotnet sln add LibraryDesk.App/LibraryDesk.App.csproj
dotnet add LibraryDesk.App/LibraryDesk.App.csproj reference LibraryDesk.Core/LibraryDesk.Core.csproj
rm -f LibraryDesk.Core/Class1.cs
```

**Структура тек проєкту:**
```text
LibraryDesk/
├── LibraryDesk.sln
├── LibraryDesk.Core/
│   └── LibraryDesk.Core.csproj
└── LibraryDesk.App/
    ├── LibraryDesk.App.csproj
    └── Program.cs
```

**Результат перевірки збірки (`dotnet build`):**
```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

### 3.2. Завдання 2. Репозиторій Git і .gitignore
Створено файл `.gitignore` на основі стандартного шаблону для .NET і дописано специфічні для проєкту правила ігнорування.

**Дописані рядки в кінець `.gitignore`:**
```gitignore
# власні винятки проєкту
*.user
data/*.local.json

# macOS and local runtime directories
Library/
.local/
**/.local/
```

*Пояснення призначення рядків:*
1. `*.user` — відсікає файли персональних налаштувань робочого простору конкретного розробника Visual Studio/Rider, які не мають потрапляти в спільний репозиторій.
2. `data/*.local.json` — запобігає випадковому коміту локальних тестових або конфігураційних наборів даних, що можуть містити чутливу інформацію або тимчасовий стан розробника.
3. `Library/` та `.local/` — ігнорують локальні системні кеші середовища виконання на платформі macOS / Unix.

*Чому теки `bin` та `obj` не зберігають у репозиторії:*  
Теки `bin` і `obj` містять артефакти компіляції (бінарні файли `.dll`, `.exe`, проміжні кеші збірки, файли налагодження `.pdb`), які є платформозалежними та генеруються автоматично під час кожного складання. Збереження їх у Git призводить до роздування розміру репозиторію, постійних нерозв'язних конфліктів злиття (merge conflicts) бінарників та ризику виконання застарілого згенерованого коду.

**Вивід `git status --short` перед першим комітом:**
```text
A  .gitignore
A  LibraryDesk.App/LibraryDesk.App.csproj
A  LibraryDesk.App/Program.cs
A  LibraryDesk.Core/LibraryDesk.Core.csproj
A  LibraryDesk.sln
```

**Коміт № 1:**
```bash
git commit -m "chore: створено каркас рішення .NET 8"
```

---

### 3.3. Завдання 3. Доменний скелет варіанта
У бібліотеці `LibraryDesk.Core` створено теку `Domain` з моделлю предметної області Варіанта 2.

#### Лістинг 1 — `LibraryDesk.Core/Domain/LoanStatus.cs`
```csharp
namespace LibraryDesk.Core.Domain;

/// <summary>Стан видачі книг у її життєвому циклі.</summary>
public enum LoanStatus
{
    Active = 0,
    Returned = 1,
    Overdue = 2,
    Cancelled = 3,
    Lost = 4,
}
```

#### Лістинг 2 — `LibraryDesk.Core/Domain/Reader.cs`
```csharp
namespace LibraryDesk.Core.Domain;

/// <summary>Читач бібліотеки.</summary>
public sealed class Reader
{
    public int Id { get; init; }

    public string FullName { get; init; } = string.Empty;

    public bool HasActiveMembership { get; init; }
}
```

#### Лістинг 3 — `LibraryDesk.Core/Domain/Book.cs`
```csharp
namespace LibraryDesk.Core.Domain;

/// <summary>Книга у фонді бібліотеки.</summary>
public sealed class Book
{
    public string Isbn { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public decimal RentalFee { get; init; }

    public int AvailableCopies { get; init; }
}
```

#### Лістинг 4 — `LibraryDesk.Core/Domain/LoanItem.cs`
```csharp
namespace LibraryDesk.Core.Domain;

/// <summary>Позиція у формулярі видачі: книга, кількість днів прокату та тариф за день.</summary>
public sealed class LoanItem
{
    public string Isbn { get; init; } = string.Empty;

    public int Days { get; init; }

    public decimal DailyRate { get; init; }

    public decimal Amount => Days * DailyRate;
}
```

#### Лістинг 5 — `LibraryDesk.Core/Domain/Loan.cs`
```csharp
namespace LibraryDesk.Core.Domain;

/// <summary>Формуляр видачі книг читачеві.</summary>
public sealed class Loan
{
    private readonly List<LoanItem> _items = new();

    public int Id { get; init; }

    public int ReaderId { get; init; }

    public LoanStatus Status { get; set; } = LoanStatus.Active;

    public DateTimeOffset IssuedAt { get; init; }

    public IReadOnlyList<LoanItem> Items => _items;

    /// <summary>Додає позицію до видачі.</summary>
    /// <param name="item">Позиція формуляра видачі.</param>
    public void AddItem(LoanItem item) => _items.Add(item);

    /// <summary>Обчислює загальну вартість прокату книг у формулярі.</summary>
    /// <returns>Загальна вартість прокату у грошових одиницях.</returns>
    public decimal Total()
    {
        decimal sum = 0m;
        foreach (LoanItem item in _items)
        {
            sum += item.Amount;
        }

        return sum;
    }
}
```

#### Лістинг 6 — `LibraryDesk.App/Program.cs`
```csharp
using System.Globalization;
using LibraryDesk.Core.Domain;

Loan loan = new()
{
    Id = 1,
    ReaderId = 10,
    IssuedAt = DateTimeOffset.Now,
};

loan.AddItem(new LoanItem
{
    Isbn = "978-0132350884",
    Days = 14,
    DailyRate = 12.50m,
});

loan.AddItem(new LoanItem
{
    Isbn = "978-0201633610",
    Days = 7,
    DailyRate = 18.00m,
});

string total = loan.Total()
    .ToString("F2", CultureInfo.InvariantCulture);

Console.WriteLine($"Видача #{loan.Id}");
Console.WriteLine($"Стан: {loan.Status}");
Console.WriteLine($"Позицій: {loan.Items.Count}");
Console.WriteLine($"Сума: {total}");
```

**Результат запуску (`dotnet run --project LibraryDesk.App`):**
```text
Видача #1
Стан: Active
Позицій: 2
Сума: 301.00
```

**Коміт № 2:**
```bash
git commit -m "feat: додано доменний скелет варіанта"
```

---

### 3.4. Завдання 4. Файл .editorconfig
У корені рішення створено файл `.editorconfig`, що формалізує правила іменування та стилю коду.

#### Лістинг 7 — Базовий фрагмент `.editorconfig`
```ini
root = true

[*]
charset = utf-8
insert_final_newline = true
trim_trailing_whitespace = true
indent_style = space
indent_size = 4

[*.{csproj,props,targets,json,yml}]
indent_size = 2

[*.cs]
max_line_length = 100

# форматування у стилі Allman
csharp_new_line_before_open_brace = all
csharp_new_line_before_else = true
csharp_new_line_before_catch = true
csharp_prefer_braces = true:warning
csharp_indent_case_contents = true
dotnet_sort_system_directives_first = true

# сучасний синтаксис C#
csharp_style_namespace_declarations = file_scoped:warning
csharp_style_var_for_built_in_types = false:suggestion

# іменування: приватні поля з підкресленням
dotnet_naming_rule.priv_underscore.symbols = priv_field
dotnet_naming_rule.priv_underscore.style = underscore_camel
dotnet_naming_rule.priv_underscore.severity = warning
dotnet_naming_symbols.priv_field.applicable_kinds = field
dotnet_naming_symbols.priv_field.applicable_accessibilities = private
dotnet_naming_style.underscore_camel.capitalization = camel_case
dotnet_naming_style.underscore_camel.required_prefix = _
```

#### Практична перевірка дієвості правил:
1. У файлі `Loan.cs` навмисно замінено відступ властивості з 4 пробілів на 2.
2. Виконано команду перевірки `dotnet format --verify-no-changes`. Отримано помилку:
```text
LibraryDesk.Core/Domain/Loan.cs(8,3): error WHITESPACE: Fix whitespace formatting. Insert "\s\s"
```
3. Виконано команду автоформатування `dotnet format`. Помилку усунуто, наступна перевірка повернула код 0.

*Чому правило іменування полів варто описувати машинно:*  
Словесний опис стандарту в документах залежить від людської уважності під час code review, де дрібні невідповідності регулярно пропускаються. Машинне правило у `.editorconfig` перевіряється аналізатором Roslyn безпосередньо в IDE та зупиняє або попереджає збірку в CI/CD, унеможливлюючи появу неоднорідного стилю в кодовій базі.

**Коміт № 3:**
```bash
git commit -m "chore: додано .editorconfig зі стилем проєкту"
```

---

### 3.5. Завдання 5. Аналізатори Roslyn і StyleCop
Створено централізований файл налаштувань компіляції та підключено аналізатор `StyleCop.Analyzers` версії `1.2.0-beta.556`.

#### Лістинг 8 — `Directory.Build.props`
```xml
<Project>
  <PropertyGroup>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>latest</LangVersion>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisLevel>8.0-recommended</AnalysisLevel>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <NuGetAudit>false</NuGetAudit>
  </PropertyGroup>
</Project>
```

#### Первинне складання після підключення StyleCop:
Команда `dotnet build` показала **14 попереджень**:
```text
Build succeeded.
/LibraryDesk.Core/Domain/Book.cs(1,1): warning SA1633: The file header is missing or not located at the top of the file.
/LibraryDesk.Core/Domain/Loan.cs(1,1): warning SA1633: The file header is missing or not located at the top of the file.
/LibraryDesk.Core/Domain/LoanItem.cs(1,1): warning SA1633: The file header is missing or not located at the top of the file.
/LibraryDesk.Core/Domain/LoanStatus.cs(1,1): warning SA1633: The file header is missing or not located at the top of the file.
/LibraryDesk.Core/Domain/Reader.cs(1,1): warning SA1633: The file header is missing or not located at the top of the file.
/LibraryDesk.App/Program.cs(1,1): warning SA1633: The file header is missing or not located at the top of the file.
/LibraryDesk.Core/Domain/Loan.cs(6,37): warning SA1309: Field '_items' should not begin with an underscore
/LibraryDesk.Core/Domain/Loan.cs(16,45): warning SA1101: Prefix local calls with this
/LibraryDesk.Core/Domain/LoanItem.cs(12,30): warning SA1101: Prefix local calls with this
/LibraryDesk.Core/Domain/LoanItem.cs(12,37): warning SA1101: Prefix local calls with this
/LibraryDesk.Core/Domain/Loan.cs(19,43): warning SA1101: Prefix local calls with this
/LibraryDesk.Core/Domain/Loan.cs(25,35): warning SA1101: Prefix local calls with this
CSC : warning SA0001: XML comment analysis is disabled due to project configuration
CSC : warning SA0001: XML comment analysis is disabled due to project configuration
    14 Warning(s)
    0 Error(s)
```

#### Таблиця аналізу первинних попереджень:

| Код правила | Кількість | Опис правила | Обґрунтоване рішення |
| :---: | :---: | :--- | :--- |
| **SA1633** | 6 | Файл повинен починатися із заголовка з ліцензією | **Вимкнути (`none`)**: юридичний копірайт є надлишковим шумом у навчальному проєкті |
| **SA1101** | 5 | Вимагає префікса `this.` перед усіма звертаннями | **Вимкнути (`none`)**: суперечить офіційним рекомендаціям Microsoft Framework Design Guidelines |
| **SA0001** | 2 | Аналіз XML-документації вимкнено в опціях збірки | **Вимкнути (`none`)**: повну XML-документацію вимагатимемо в ЛР № 4 |
| **SA1309** | 1 | Поле не може починатися із символу `_` | **Вимкнути (`none`)**: у сучасних конвенціях C# приватні поля позначають саме з `_` |
| **SA1200** | 0 | Директиви `using` мають бути всередині простору імен | **Вимкнути (`none`)**: стандарт шаблонів .NET 8 передбачає using на початку файлу |

#### Дописана секція правил у `.editorconfig`:
```ini
# StyleCop: свідомо вимкнені правила
# заголовок з ліцензією в навчальному проєкті не потрібен
dotnet_diagnostic.SA1633.severity = none
# префікс this. суперечить конвенції .NET
dotnet_diagnostic.SA1101.severity = none
# using поза namespace — стандарт шаблонів .NET
dotnet_diagnostic.SA1200.severity = none
# приватні поля іменуємо з _, як прийнято в .NET
dotnet_diagnostic.SA1309.severity = none
# XML-документацію вимагатимемо з ЛР № 4, поки що ні
dotnet_diagnostic.SA0001.severity = none
dotnet_diagnostic.SA1600.severity = none
dotnet_diagnostic.SA1601.severity = none
dotnet_diagnostic.SA1602.severity = none
dotnet_diagnostic.SA1611.severity = none
dotnet_diagnostic.SA1615.severity = none
# кома в кінці багаторядкового ініціалізатора необов’язкова
dotnet_diagnostic.SA1413.severity = none

# правила, які лишаємо суворими
dotnet_diagnostic.SA1028.severity = warning
dotnet_diagnostic.SA1201.severity = warning
dotnet_diagnostic.SA1503.severity = warning
dotnet_diagnostic.SA1516.severity = warning
dotnet_diagnostic.IDE0055.severity = warning
```

*Обґрунтування*: Правило `SA1503` (вимога фігурних дужок для всіх операторів) залишено суворим (`warning`), оскільки пропуск дужок у конструкціях `if` є джерелом класичних критичних багів (наприклад, знаменитий Apple `goto fail`). Натомість `SA1101` вимкнено, бо багаторазове дублювання `this.` засмічує код і погіршує його читабельність.

**Результат після вимкнення шуму:**
```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

**Коміт № 4:**
```bash
git commit -m "chore: увімкнено аналізатори Roslyn і StyleCop"
```

---

### 3.6. Завдання 6. Стандарт кодування проєкту й README
Створено повнорозмірний інженерний документ `CODING_STANDARD.md` (7 обов'язкових розділів, 17 машинних правил у таблиці розділу 7) та `README.md`.

**Коміт № 5:**
```bash
git commit -m "docs: додано стандарт кодування і README"
```

---

### 3.7. Завдання 7. Чиста збірка й підсумковий коміт
Проведено повне контрольне складання з попереднім очищенням:

```bash
dotnet format --verify-no-changes
dotnet clean
dotnet build
```

Вивід фінального складання:
```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

Історія комітів у репозиторії (`git log --oneline`):
```text
2b8568a docs: додано стандарт кодування і README
546f547 chore: увімкнено аналізатори Roslyn і StyleCop
a07b048 chore: додано .editorconfig зі стилем проєкту
57edaea feat: додано доменний скелет варіанта
453c6f9 chore: створено каркас рішення .NET 8
```

---

## 4. Аналіз результату
Під час виконання роботи було виявлено, що стандартний набір правил StyleCop за замовчуванням містить багато застарілих вимог (наприклад, обов'язковий `this.`, директиви `using` виключно всередині блоку `namespace`, заборона знака підкреслення у приватних полях), які були актуальними для C# версії 2.0–3.0, але сьогодні прямо суперечать сучасним офіційним рекомендаціям Microsoft.

Чіткий поділ попереджень на:
1. **Інформаційний шум** (відсутність заголовка з ліцензією у студентському проєкті, стиль `this.`), який свідомо вимикається з документальним обґрунтуванням у `.editorconfig`.
2. **Справжні дефекти** (пропущені дужки в `if`, пробіли в кінці рядка, неправильний розмір відступу, некоректне іменування), які примусово викликають попередження компілятора (`warning`).

Такий підхід забезпечує культуру «збірки з нулем попереджень», що є стандартом промислової розробки програмного забезпечення.

---

## 5. Висновки
1. Опановано інструменти командного рядка .NET CLI для генерації та налаштування багатомодульних рішень (solution + classlib + console).
2. Реалізовано архітектурне розмежування бізнес-домену (`LibraryDesk.Core`) та шару відображення (`LibraryDesk.App`), що гарантує придатність коду до модульного тестування в майбутньому.
3. Налаштовано надійний `.gitignore`, що виключає потрапляння скомпільованих бінарників (`bin/`, `obj/`) та локальних файлів середовища розробки до Git.
4. Описано та машинізовано стандарт кодування проєкту у файлах `.editorconfig` та `Directory.Build.props`, що автоматизує перевірку стилю за допомогою `dotnet format` та компілятора.
5. Інтегровано статичні аналізатори Roslyn та StyleCop, налаштовано діагностичні рівні та досягнуто чистого складання без жодного попередження (`0 Warnings, 0 Errors`).
6. Засвоєно конвенцію Conventional Commits для формування структурованої та зрозумілої історії змін у Git.

---

## 6. Посилання на репозиторій
- **Локальний Git-репозиторій**: гілка `main`, мітка `lab1`.
- **Файл звіту та коду**: надано у складі навчального проєкту.

---

## 7. Відповіді на контрольні питання до захисту

### 1. Що входить до конструювання програмного забезпечення за SWEBOK, а що до нього не належить? Наведіть по два приклади.
**Відповідь**:  
Згідно зі стандартом **SWEBOK (Software Engineering Body of Knowledge)**, конструювання ПЗ (Software Construction) охоплює етапи, на яких проєктні рішення перетворюються на працездатний, перевірений програмний продукт.
- **Входить до конструювання**:
  1. Безпосереднє написання та форматування вихідного коду (Coding).
  2. Модульне тестування розробником (Unit testing) та зневадження (Debugging).
- **Не належить до конструювання**:
  1. Аналіз та збір вимог (Software Requirements Engineering).
  2. Глобальне архітектурне проєктування системи (Software Architecture/Design), експлуатація та супровід (Maintenance).

---

### 2. Навіщо в навчальному проєкті розділяти логіку та консольний застосунок на два проєкти рішення, якщо код можна тримати в одному? Які дві переваги дає цей поділ?
**Відповідь**:  
Розділення на два проєкти (`LibraryDesk.Core` як бібліотека та `LibraryDesk.App` як виконуваний файл) втілює принцип слабкої зв'язності (Loose Coupling) та проєктування для тестування (Design for Testability).
- **Перевага 1**: Фізична неможливість виклику консольних методів (`Console.WriteLine`, `Console.ReadLine`) всередині бізнес-логіки. Бібліотека залишається «чистою» і незалежною від платформи чи інтерфейсу користувача.
- **Перевага 2**: Придатність до модульного тестування (Unit Testing) та повторного використання: до `Core` у майбутньому можна підключити будь-який інший шар відображення (Web API, GUI на WPF/MAUI, Telegram-бот) без зміни жодного рядка бізнес-логіки.

---

### 3. Що станеться, якщо теки bin і obj потраплять до репозиторію? Назвіть три наслідки.
**Відповідь**:  
1. **Постійні конфлікти злиття (Merge Conflicts)**: Файли збірки (`.dll`, `.pdb`, `.cache`) є бінарними; Git не вміє текстово зливати такі файли, тому будь-яка спроба синхронізації між розробниками викликатиме блокуючі конфлікти.
2. **Роздування розміру репозиторію**: З кожним комітом репозиторій зберігатиме нові версії важких бінарних файлів, що суттєво уповільнює виконання команд `git clone`, `git fetch` та `git push`.
3. **«Фантомні» помилки збірки**: Розробник на іншій ОС (наприклад, Linux або інша версія SDK) отримає кешовані артефакти чужої системи, що призведе до неочевидних збоїв компіляції замість генерації свіжих цільових файлів.

---

### 4. Чим відрізняється git add від git commit? Що таке індекс (staging area)?
**Відповідь**:  
- **Індекс (Staging Area / Cache)** — це проміжний шар між вашим робочим каталогом (Working Directory) та історією репозиторію, де збираються зміни, призначені для наступного збереження.
- `git add <файли>` — переносить вказані змінені файли з робочого каталогу до індексу (staging area), готуючи їх до фіксації.
- `git commit` — фіксує знімок (снапшот) усіх файлів, які знаходяться в індексі, створюючи новий незмінний об'єкт коміту в дереві версій репозиторію з метаданими (автор, час, повідомлення, хеш SHA-1).

---

### 5. Чому правило стандарту, записане лише в текстовому документі, виконується гірше за правило, записане в .editorconfig?
**Відповідь**:  
Текстовий документ розрахований лише на людську пам'ять і дисципліну. Під час інтенсивної розробки або рев'ю розробники неминуче пропускають дрібні порушення форматування чи іменування.  
Правило в `.editorconfig`:
- Інтерпретується редактором коду (IDE) на льоту — код форматується автоматично під час збереження або натискання комбінації клавіш.
- Перевіряється автоматизовано командою `dotnet format` або під час складання через MSBuild/CI, блокуючи створення некоректного коду ще до його потрапляння в репозиторій.

---

### 6. Поясніть різницю між рівнями діагностики none, suggestion, warning і error. Коли доречно ставити error?
**Відповідь**:  
- `none` — правило повністю вимкнено; компілятор та IDE його ігнорують.
- `suggestion` — підказка/порада; відображається в IDE (наприклад, три крапки під кодом), але ніколи не потрапляє у вивід терміналу та не впливає на успішність збірки.
- `warning` — попередження; виводиться компілятором у консоль під час збірки, вказує на потенційний дефект або порушення стилю, але не перериває створення бінарних файлів (якщо не ввімкнено `TreatWarningsAsErrors`).
- `error` — фатальна помилка; збірка негайно зупиняється, вихідні артефакти не створюються.
- **Коли ставити `error`**: Для критичних правил безпеки, критичних архітектурних порушень або правил, порушення яких гарантовано призводить до збоїв у рантаймі (наприклад, заборона синтаксичних помилок, неприпустимі перетворення типів, витік пам'яті чи обов'язкові правила безпеки компанії в CI/CD).

---

### 7. Для чого потрібен файл Directory.Build.props і що робить властивість EnforceCodeStyleInBuild?
**Відповідь**:  
- `Directory.Build.props` — це спеціальний файл MSBuild, параметри з якого автоматично імпортуються всіма проєктами (`.csproj`), що лежать у цій самій або вкладених теках. Він дозволяє централізовано керувати версіями мови, налаштуваннями компілятора та спільними властивостями рішень без дублювання в кожному проєкті.
- `EnforceCodeStyleInBuild = true` — примусово вмикає виконання правил стилю коду (правила з префіксом `IDE*` з `.editorconfig`) безпосередньо на етапі складання проєкту компілятором. Без цієї властивості правила стилю відображаються лише як підказки в текстовому редакторі і не контролюються командою `dotnet build`.

---

### 8. Чим правила з префіксом CA відрізняються від правил із префіксом SA? Наведіть по одному прикладу.
**Відповідь**:  
- **Префікс `CA` (Code Analysis)** — вбудовані правила аналізаторів платформи .NET (Roslyn Analyzers від Microsoft). Вони контролюють дизайн коду, швидкодію, безпеку, надійність та використання API.  
  *Приклад*: `CA1051: Do not declare visible instance fields` (вимагає інкапсуляції полів за допомогою властивостей).
- **Префікс `SA` (StyleCop Analyzers)** — правила стороннього інструменту StyleCop, що фокусуються виключно на візуальному оформленні, стилі коду, форматуванні відступів, порядку розміщення членів класу та наявності документації.  
  *Приклад*: `SA1028: Code should not contain trailing whitespace` (заборона пробілів у кінці рядків).

---

### 9. Ви вимкнули правило StyleCop, бо воно давало 40 попереджень. Чому таке вимкнення обов’язково супроводжувати коментарем і що станеться з проєктом, якщо цього не робити?
**Відповідь**:  
Кожне вимкнене правило без коментаря — це прихований технічний борг та втрата інженерного контексту.  
Якщо не писати обґрунтування:
1. Інші члени команди або новий розробник не зрозуміють, чи було це правило вимкнено тимчасово через брак часу, чи це свідоме архітектурне рішення команди.
2. Проєкт ризикує перетворитися на некеровану кодову базу, де розробники замість виправлення дефектів просто «заглушають» усі незручні перевірки, поки аналізатор не втратить свій сенс повністю.

---

### 10. Покажіть у своєму стандарті кодування два пункти, які рецензент може перевірити однозначно, і один, який довелося сформулювати нечітко. Як би ви переписали нечіткий пункт?
**Відповідь**:  
- **Два однозначні пункти**:
  1. *«Відступ становить 4 пробіли; табуляція суворо заборонена»* — перевіряється машинно або лінійкою символів: якщо є символ `\t` чи відступ 2 пробіли, правило порушено однозначно.
  2. *«Приватні поля починаються з префікса `_` у стилі camelCase»* — правило `IDE1006` автоматично видає `warning`, якщо поле оголошено як `private int items;` замість `private int _items;`.
- **Один нечіткий пункт**:
  - *Формулювання*: *«Іменування сутностей має однозначно відображати їхнє призначення в предметній області»*. Оцінка «однозначності» суб'єктивна для різних людей.
  - **Як переписати чітко**: *«Назва сутності повинна містити термін з офіційного глосарія предметної області LibraryDesk (Reader, Book, Loan, LoanItem) і не може містити загальних абстрактних суфіксів на кшталт Data, Info, Object, ItemHolder або скорочень, окрім Id та Isbn»*.
