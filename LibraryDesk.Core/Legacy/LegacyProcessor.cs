namespace LibraryDesk.Core.Legacy;

/// <summary>Початкова реалізація розрахунку для характеризаційних тестів.</summary>
public static class LegacyProcessor
{
    /// <summary>Обчислює суму формуляра одним методом-монстром.</summary>
    public static decimal Process(
        int loanId, string readerName, string readerEmail,
        bool isRegular, List<LoanLine> lines,
        DateOnly issuedAt, string couponCode, string status,
        bool printToConsole, decimal deliveryPrice,
        out string newStatus)
    {
        newStatus = status;
        decimal total = 0;
        if (loanId <= 0)
        {
            Console.WriteLine("Помилка: номер формуляра");
            return -1;
        }
        if (readerName == null || readerName == "")
        {
            Console.WriteLine("Помилка: немає читача");
            return -1;
        }
        if (readerEmail == null || !readerEmail.Contains("@"))
        {
            Console.WriteLine("Помилка: пошта читача");
            return -1;
        }
        if (lines == null || lines.Count == 0)
        {
            Console.WriteLine("Помилка: немає книжок");
            return -1;
        }
        if (status != "Active" && status != "Returned" && status != "Overdue" && status != "Cancelled")
        {
            Console.WriteLine("Помилка: невідомий стан");
            return -1;
        }
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Days <= 0)
            {
                Console.WriteLine("Помилка: кількість днів");
                return -1;
            }
            if (lines[i].DailyRate < 0)
            {
                Console.WriteLine("Помилка: тариф");
                return -1;
            }
            decimal sum = lines[i].Days * lines[i].DailyRate;
            if (lines[i].Days >= 10)
            {
                sum = sum * 0.95m;
            }
            total = total + sum;
        }
        if (isRegular)
        {
            total = total * 0.93m;
        }
        if (total > 5000)
        {
            total = total * 0.9m;
        }
        else if (total > 2000)
        {
            total = total * 0.95m;
        }
        if (couponCode == "SALE10")
        {
            total = total - total * 0.1m;
        }
        else if (couponCode == "MINUS200" && total > 1000)
        {
            total = total - 200;
        }
        if (issuedAt.DayOfWeek == DayOfWeek.Sunday)
        {
            total = total * 0.98m;
        }
        if (total < 1000)
        {
            total = total + deliveryPrice;
        }
        if (total < 0)
        {
            total = 0;
        }
        total = Math.Round(total, 2);
        if (status == "Active" && total > 0)
        {
            newStatus = "Returned";
        }
        else if (status == "Returned")
        {
            newStatus = "Overdue";
        }
        else if (status == "Cancelled")
        {
            newStatus = "Cancelled";
            total = 0;
        }
        if (printToConsole)
        {
            Console.WriteLine("Формуляр № " + loanId);
            Console.WriteLine("Читач: " + readerName);
            Console.WriteLine("Пошта: " + readerEmail);
            for (int i = 0; i < lines.Count; i++)
            {
                Console.WriteLine(lines[i].Isbn + " x " + lines[i].Days);
            }
            Console.WriteLine("Разом: " + total);
            Console.WriteLine("Стан: " + newStatus);
        }
        return total;
    }
}
