# Аудит SOLID: навчальний `LegacyLab4/LoanManager`

Це **навмисно поганий** код, адаптований з лістинга ЛР № 4 до формуляра видачі `LibraryDesk`. Він не викликається з робочого застосунку. `SmtpNotifier` у прикладі лише імітує сповіщення через консоль.

| Принцип | Доказ у коді | Наслідок майбутньої зміни | Засіб у завданні 6 |
| --- | --- | --- | --- |
| SRP | `LoanManager.Place` рахує суму, викликає `_store.SaveToFile` і `_notifier.Send`; `BuildCsvReport` ще й пише файл | Зміна формату CSV або тексту сповіщення вимагає правити клас розрахунку | `LoanService`, `ILoanRepository`, `LoanCsvReport`, `INotifier` |
| OCP | Ланцюг `if (readerType == "regular") ... else if ("vip") ... else if ("staff")` | Новий тип читача змусить редагувати `Place` | Окремі реалізації `IPricingPolicy`, вибір у `Program.cs` |
| LSP | `ArchivedLoan : LegacyLoan` перевизначає `AddLine` з `throw new NotSupportedException`; `Archive` викликає його через змінну `LegacyLoan` | Спроба архівувати формуляр завершується винятком замість збереження | `ArchivedLoanSnapshot` містить `Loan`, не успадковує його |
| ISP | `ILoanStore` має 7 методів, а `LoanManager` потребує лише `SaveToFile` | Новий спосіб зберігання мусив би реалізувати пошту, друк і резервну копію | Вузькі `ILoanRepository` і `INotifier` |
| DIP | Поля `private readonly FileLoanStore _store = new()` і `SmtpNotifier _notifier = new()` | Для тесту ціни потрібно піднімати файлове сховище й консольний сповіщувач | Передавати абстракції конструктору `LoanService` |

**Кількісна оцінка:** `LoanManager` має **5 причин для зміни**: правило ціни, збереження, сповіщення, формат звіту, архівування. Із **7** методів `ILoanStore` цьому класу потрібен **1** (`SaveToFile`).
