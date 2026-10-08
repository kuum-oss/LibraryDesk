#pragma warning disable CS1591, CA1716, SA1600, SA1611, SA1615

namespace LibraryDesk.Core.LegacyLab4;

// Навмисно широкий інтерфейс із семи не пов'язаних обов'язків.
public interface ILoanStore
{
    void Add(LegacyLoan loan);

    LegacyLoan GetById(int id);

    void SaveToFile(LegacyLoan loan, string path);

    void SendEmail(string to, string body);

    string ExportCsv();

    void Backup(string folder);

    void PrintReceipt(LegacyLoan loan);
}
