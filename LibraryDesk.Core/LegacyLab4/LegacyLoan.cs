#pragma warning disable CS1591, SA1402, SA1649, SA1600, SA1611, SA1615

namespace LibraryDesk.Core.LegacyLab4;

// Навмисно незахищена модель із методички для аудиту SOLID.
public sealed class LegacyLoanLine
{
    public string Isbn { get; set; } = string.Empty;

    public int Days { get; set; }

    public decimal DailyRate { get; set; }
}

public class LegacyLoan
{
    public int Id { get; set; }

    public int Status { get; set; }

    public decimal Total { get; set; }

    public string Email { get; set; } = string.Empty;

    public List<LegacyLoanLine> Lines { get; set; } = new();

    public virtual void AddLine(LegacyLoanLine line) => Lines.Add(line);
}

public sealed class ArchivedLoan : LegacyLoan
{
    public ArchivedLoan(int id) => Id = id;

    public override void AddLine(LegacyLoanLine line)
    {
        throw new NotSupportedException("Архів змінювати не можна.");
    }
}
