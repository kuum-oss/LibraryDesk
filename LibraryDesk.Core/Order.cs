using System;
using System.Collections.Generic;
using System.Globalization;

namespace LibraryDesk.Core;

/// <summary>
/// Представляє замовлення / формуляр видачі в системі.
/// </summary>
public class Order
{
    /// <summary>Ставка податку на додану вартість.</summary>
    private const decimal VatRate = 0.2m;

    /// <summary>Поріг суми для знижки постійному покупцю.</summary>
    private const decimal RegularDiscountThreshold = 1000m;

    /// <summary>Частка знижки постійному покупцю.</summary>
    private const decimal RegularDiscountRate = 0.10m;

    /// <summary>Поріг суми для знижки на велике замовлення.</summary>
    private const decimal LargeOrderThreshold = 5000m;

    /// <summary>Частка знижки на велике замовлення.</summary>
    private const decimal LargeOrderDiscountRate = 0.15m;

    /// <summary>Кількість рядків, з якої діє гуртова знижка.</summary>
    private const int BulkLineCount = 10;

    /// <summary>Сума гуртової знижки, грн.</summary>
    private const decimal BulkDiscountAmount = 100m;

    /// <summary>Максимально допустима кількість рядків у замовленні.</summary>
    private const int MaxLineCount = 100;

    /// <summary>Мінімальна довжина імені замовника.</summary>
    private const int MinCustomerNameLength = 3;

    /// <summary>Стан: новий.</summary>
    private const int StatusNew = 0;

    /// <summary>Стан: оплачено.</summary>
    private const int StatusPaid = 1;

    /// <summary>Стан: відправлено.</summary>
    private const int StatusShipped = 2;

    /// <summary>Стан: скасовано.</summary>
    private const int StatusCancelled = 3;

    private readonly List<string[]> _lines = new();

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
    public int Status { get; private set; } = StatusNew;

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
        line[1] = quantity.ToString(CultureInfo.InvariantCulture);
        line[2] = unitPrice.ToString(CultureInfo.InvariantCulture);
        _lines.Add(line);
    }

    /// <summary>
    /// Обчислює підсумкову суму замовлення з урахуванням знижок та ПДВ.
    /// </summary>
    /// <param name="isRegularCustomer">Ознака постійного покупця.</param>
    /// <returns>Підсумкова сума, заокруглена до двох знаків.</returns>
    public decimal CalculateTotal(bool isRegularCustomer)
    {
        decimal total = Subtotal();
        total = ApplyPercentageDiscount(total, isRegularCustomer);
        total -= _lines.Count > BulkLineCount ? BulkDiscountAmount : 0m;
        total = Math.Max(total, 0m);
        total += total * VatRate;
        return Math.Round(total, 2);
    }

    /// <summary>
    /// Змінює стан замовлення, якщо перехід дозволений правилами предметної області.
    /// </summary>
    /// <param name="newStatus">Цільовий стан замовлення.</param>
    /// <returns>true, якщо перехід виконано; false, якщо перехід заборонений.</returns>
    public bool TryChangeStatus(int newStatus)
    {
        if (Status == StatusNew && newStatus == StatusPaid)
        {
            Status = StatusPaid;
            return true;
        }

        if (Status == StatusPaid && newStatus == StatusShipped)
        {
            Status = StatusShipped;
            return true;
        }

        if (Status == StatusNew && newStatus == StatusCancelled)
        {
            Status = StatusCancelled;
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
            && Id.Length > 0
            && CustomerName != null
            && CustomerName.Length >= MinCustomerNameLength
            && _lines.Count > 0
            && _lines.Count < MaxLineCount
            && Status >= StatusNew
            && Status <= StatusCancelled)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Формує текстове подання звіту по рядках замовлення.
    /// </summary>
    /// <returns>Текстовий звіт із деталізацією позицій та загальною сумою.</returns>
    public string BuildReport()
    {
        string reportText = string.Empty;
        for (int i = 0; i < _lines.Count; i++)
        {
            reportText = reportText
                + "Товар: " + _lines[i][0]
                + "; кількість: " + _lines[i][1]
                + "; ціна: " + _lines[i][2]
                + "; сума: "
                + (int.Parse(_lines[i][1], CultureInfo.InvariantCulture)
                    * decimal.Parse(_lines[i][2], CultureInfo.InvariantCulture))
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

    private decimal Subtotal()
    {
        decimal total = 0m;
        foreach (string[] line in _lines)
        {
            int quantity = int.Parse(line[1], CultureInfo.InvariantCulture);
            decimal unitPrice = decimal.Parse(line[2], CultureInfo.InvariantCulture);
            total += quantity * unitPrice;
        }

        return total;
    }

    private static decimal ApplyPercentageDiscount(decimal total, bool isRegularCustomer)
    {
        if (isRegularCustomer && total > RegularDiscountThreshold)
        {
            return total * (1m - RegularDiscountRate);
        }

        return total > LargeOrderThreshold ? total * (1m - LargeOrderDiscountRate) : total;
    }
}
