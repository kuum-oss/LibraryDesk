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
