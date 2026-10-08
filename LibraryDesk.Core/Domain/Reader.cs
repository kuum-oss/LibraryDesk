namespace LibraryDesk.Core.Domain;

/// <summary>Незмінний запис читача бібліотеки.</summary>
public sealed record Reader
{
    /// <summary>Створює читача з перевіреними реквізитами.</summary>
    /// <param name="id">Ідентифікатор читача.</param>
    /// <param name="fullName">Повне ім'я читача.</param>
    /// <param name="email">Адреса електронної пошти.</param>
    /// <param name="hasActiveMembership">Ознака активного членства.</param>
    public Reader(int id, string fullName, string email, bool hasActiveMembership)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        if (!email.Contains('@', StringComparison.Ordinal))
        {
            throw new ArgumentException("Пошта має містити символ @.", nameof(email));
        }

        Id = id;
        FullName = fullName;
        Email = email;
        HasActiveMembership = hasActiveMembership;
    }

    /// <summary>Унікальний ідентифікатор читача.</summary>
    public int Id { get; }

    /// <summary>Повне ім'я читача.</summary>
    public string FullName { get; }

    /// <summary>Адреса електронної пошти.</summary>
    public string Email { get; }

    /// <summary>Ознака наявності активного членства у бібліотеці.</summary>
    public bool HasActiveMembership { get; }
}
