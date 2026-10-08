# Результат виправлення SOLID

| Принцип | Було (`LegacyLab4`) | Стало |
| --- | --- | --- |
| SRP | `LoanManager.Place` рахує, зберігає, сповіщає; `BuildCsvReport` пише файл | `LoanService` виконує сценарій, `ILoanRepository` і `InMemoryLoanRepository` зберігають, `LoanCsvReport` лише форматує текст |
| OCP | `Place` змінюють для кожного нового `readerType` | `IPricingPolicy` та окремі `StandardPricingPolicy` / `DiscountPricingPolicy`; вибір реалізації в `Program.cs` |
| LSP | `ArchivedLoan : LegacyLoan` кидає виняток із `AddLine` | `ArchivedLoanSnapshot` містить завершений `Loan` і відкриває лише перегляд |
| ISP | `ILoanStore` змішує 7 методів | `ILoanRepository` містить 3 методи зберігання, `INotifier` — 1 метод сповіщення |
| DIP | `_store = new FileLoanStore()` і `_notifier = new SmtpNotifier()` у `LoanManager` | `LoanService` приймає `ILoanRepository`, `IPricingPolicy`, `INotifier` у конструкторі; конкретні типи створює `Program.cs` |

Для зміни політики ціни у поточній програмі достатньо змінити **один рядок** композиційного кореня (`IPricingPolicy pricing = ...`). Новий тип правила можна додати окремим класом, не змінюючи `LoanService`.
