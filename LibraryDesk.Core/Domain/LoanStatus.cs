namespace LibraryDesk.Core.Domain;

/// <summary>Стан видачі книг у її життєвому циклі.</summary>
public enum LoanStatus
{
    /// <summary>Видача активна.</summary>
    Active = 0,

    /// <summary>Книги повернуто.</summary>
    Returned = 1,

    /// <summary>Термін повернення прострочено.</summary>
    Overdue = 2,

    /// <summary>Видачу скасовано.</summary>
    Cancelled = 3,

    /// <summary>Книги втрачено.</summary>
    Lost = 4,
}
