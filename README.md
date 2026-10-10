# LibraryDesk

[![CI](https://github.com/kuum-oss/LibraryDesk/actions/workflows/ci.yml/badge.svg)](https://github.com/kuum-oss/LibraryDesk/actions/workflows/ci.yml)

Навчальний проєкт з дисципліни «Конструювання програмного забезпечення».  
**Варіант № 2**: облік видачі книг у бібліотеці.

## Призначення системи
Проєкт призначений для автоматизації процесів обліку абонементів читачів, реєстрації книжкового фонду, формування формулярів видачі книг (`Loan`) із контролем термінів, розрахунку вартості прокату та відстеження життєвого циклу видачі.

## Склад рішення
- **`LibraryDesk.Core`** — бібліотека класів, що містить моделі предметної області, перелічення та логіку розрахунків без прив'язки до консолі.
- **`LibraryDesk.App`** — консольний застосунок (шар взаємодії з користувачем), який посилається на `LibraryDesk.Core`.

## Системні вимоги
- **.NET SDK 8.0** або новіший (підтримується .NET 8 / 10).
- **Git** версії 2.40+.

## Складання та запуск

1. **Клонування репозиторію:**
   ```bash
   git clone <URL-вашого-репозиторію>
   cd LibraryDesk
   ```

2. **Складання рішення:**
   ```bash
   dotnet build
   ```

3. **Запуск застосунку:**
   ```bash
   dotnet run --project LibraryDesk.App
   ```

## Стандарт кодування та статичний аналіз
Правила розробки детально викладено у файлі [`CODING_STANDARD.md`](file:///Users/dimagordeev/Desktop/untitled%20folder/Console/CODING_STANDARD.md) та машинно закріплено у [`.editorconfig`](file:///Users/dimagordeev/Desktop/untitled%20folder/Console/.editorconfig), [`Directory.Build.props`](file:///Users/dimagordeev/Desktop/untitled%20folder/Console/Directory.Build.props) і правилах **StyleCop.Analyzers**.

- **Перевірка форматування:**
  ```bash
  dotnet format --verify-no-changes
  ```
- **Автоматичне виправлення форматування:**
  ```bash
  dotnet format
  ```
- **Складання з повною перевіркою правил:**
  ```bash
  dotnet clean && dotnet build
  ```

## Лабораторна робота 5

У гілці `lab05-error-handling` реалізовано надійну обробку помилок:

- зовнішні рядки перетворюються на `LoanItem` у `LoanItemParser`, а ядро працює лише з типізованими даними;
- очікувані помилки введення повертаються як `Result<T>`;
- порушення правил предметної області описує `DomainRuleException` з ідентифікатором правила та внутрішнім винятком;
- `LoanService` використовує структуроване логування рівнів Debug, Information, Warning і Error з областю `Loan:{LoanId}`;
- `FileAuditLog` реалізує `IDisposable`, а тест перевіряє звільнення файла після винятку.

Перевірка роботи:

```bash
dotnet build LibraryDesk.sln
dotnet test LibraryDesk.sln --no-build
dotnet run --project LibraryDesk.App/LibraryDesk.App.csproj --no-build
```

## Автор
Гордєєв Дмитро, група ІПЗ (Варіант № 2)
