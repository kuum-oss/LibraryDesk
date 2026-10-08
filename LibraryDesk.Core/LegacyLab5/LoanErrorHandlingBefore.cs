#pragma warning disable CA1031, CA1822, CA2200, CS1591, SA1600

using LibraryDesk.Core.Abstractions;
using LibraryDesk.Core.Domain;

namespace LibraryDesk.Core.LegacyLab5;

// Навмисний стан "до": чотири антипатерни мають бути видні в історії Git.
public sealed class LoanErrorHandlingBefore
{
    private readonly ILoanRepository _repository;

    public LoanErrorHandlingBefore(ILoanRepository repository)
    {
        _repository = repository;
    }

    public Loan? Load(int loanId)
    {
        try
        {
            return _repository.GetById(loanId);
        }
        catch (Exception)
        {
        }

        return null;
    }

    public void Cancel(int loanId, string reason)
    {
        try
        {
            Loan? loan = _repository.GetById(loanId);
            loan!.Cancel();
        }
        catch (Exception exception)
        {
            Console.WriteLine("Щось пішло не так");
            throw exception;
        }
    }

    public int CountValidDays(string[] rows)
    {
        int valid = 0;
        foreach (string row in rows)
        {
            try
            {
                int days = int.Parse(row);
                if (days > 0)
                {
                    valid++;
                }
            }
            catch (FormatException)
            {
            }
        }

        return valid;
    }
}
