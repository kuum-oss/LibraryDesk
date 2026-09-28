using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryDesk.Core;

/// <summary>
/// Представляє замовлення / формуляр видачі в системі.
/// </summary>
public class Order
{
    private readonly List<string[]> _lines = new();

    // public string prim; // примітка, поки не треба

    /// <summary>
    /// Ініціалізує новий екземпляр класу <see cref="Order"/>.
    /// </summary>
    /// <param name="id">Ідентифікатор замовлення.</param>
    /// <param name="customerName">Ім'я замовника / читача.</param>
    public Order(string id, string customerName)
    {
        Id = id;
        CustomerName = customerName;
        CreatedAt = DateTime.Now;
    }

    /// <summary>
    /// Отримує ідентифікатор замовлення.
    /// </summary>
    public string Id { get; private set; }

    /// <summary>
    /// Отримує ім'я замовника / читача.
    /// </summary>
    public string CustomerName { get; private set; }

    /// <summary>
    /// Отримує поточний числовий стан замовлення.
    /// </summary>
    public int Status { get; private set; } = 0;

    /// <summary>
    /// Отримує дату і час створення замовлення.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Додає до замовлення рядок із товаром / книгою.
    /// </summary>
    /// <param name="sku">Артикул товару / ISBN книги.</param>
    /// <param name="quantity">Кількість одиниць або днів.</param>
    /// <param name="unitPrice">Ціна за одиницю або тариф.</param>
    public void AddLine(string sku, int quantity, decimal unitPrice)
    {
        string[] line = new string[3];
        line[0] = sku;
        line[1] = quantity.ToString();
        line[2] = unitPrice.ToString();
        _lines.Add(line);
    }

    /// <summary>
    /// Обчислює підсумкову суму замовлення з урахуванням знижок та ПДВ.
    /// </summary>
    /// <param name="isRegularCustomer">Ознака постійного покупця.</param>
    /// <returns>Підсумкова сума, заокруглена до двох знаків.</returns>
    public decimal CalculateTotal(bool isRegularCustomer)
    {
        decimal total = 0;
        int lineCount = 0;

        for (int i = 0; i < _lines.Count; i++)
        {
            int quantity = int.Parse(_lines[i][1]);
            decimal unitPrice = decimal.Parse(_lines[i][2]);
            total = total + (quantity * unitPrice);
            lineCount = lineCount + 1;
        }

        // if (sum1 > 500) { sum1 = sum1 - 50; } // стара знижка
        if (isRegularCustomer == true && total > 1000)
        {
            total = total * 0.9m;
        }
        else if (total > 5000)
        {
            total = total * 0.85m;
        }
        else
        {
            total = total;
        }

        if (lineCount > 10)
        {
            total = total - 100;
        }

        if (total < 0)
        {
            total = 0;
        }

        // Ставка ПДВ 20% нараховується на суму після знижок
        total = total + (total * 0.2m);
        return Math.Round(total, 2);
    }

    /// <summary>
    /// Змінює стан замовлення, якщо перехід дозволений правилами предметної області.
    /// </summary>
    /// <param name="newStatus">Цільовий стан замовлення.</param>
    /// <returns>true, якщо перехід виконано; false, якщо перехід заборонений.</returns>
    public bool TryChangeStatus(int newStatus)
    {
        if (Status == 0 && newStatus == 1)
        {
            Status = 1;
            return true;
        }

        if (Status == 1 && newStatus == 2)
        {
            Status = 2;
            return true;
        }

        if (Status == 0 && newStatus == 3)
        {
            Status = 3;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Перевіряє коректність заповнення обов'язкових полів замовлення.
    /// </summary>
    /// <returns>true, якщо замовлення валідне; інакше false.</returns>
    public bool IsValid()
    {
        if (Id != null
            && Id != ""
            && CustomerName != null
            && CustomerName.Length > 2
            && _lines.Count > 0
            && _lines.Count < 100
            && Status >= 0
            && Status <= 3)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Формує текстове подання звіту по рядках замовлення.
    /// </summary>
    /// <returns>Текстовий звіт із деталізацією позицій та загальною сумою.</returns>
    public string BuildReport()
    {
        string reportText = "";
        for (int i = 0; i < _lines.Count; i++)
        {
            reportText = reportText
                + "Товар: " + _lines[i][0]
                + "; кількість: " + _lines[i][1]
                + "; ціна: " + _lines[i][2]
                + "; сума: "
                + (int.Parse(_lines[i][1]) * decimal.Parse(_lines[i][2]))
                + "\n";
        }

        reportText = reportText + "Разом: " + CalculateTotal(false) + "\n";
        return reportText;
    }

    /// <summary>
    /// Виконує пошук замовлення у списку за його ідентифікатором.
    /// </summary>
    /// <param name="orders">Колекція замовлень для пошуку.</param>
    /// <param name="orderId">Шуканий ідентифікатор замовлення.</param>
    /// <returns>Знайдений екземпляр <see cref="Order"/> або null, якщо не знайдено.</returns>
    public static Order? FindById(List<Order> orders, string orderId)
    {
        for (int i = 0; i < orders.Count; i++)
        {
            if (orders[i].Id == orderId)
            {
                return orders[i];
            }
        }

        return null;
    }
}
