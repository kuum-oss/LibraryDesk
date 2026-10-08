namespace LibraryDesk.Core.Errors;

/// <summary>Результат очікуваної операції, що містить значення або пояснення помилки.</summary>
/// <typeparam name="T">Тип успішного значення.</typeparam>
public readonly struct Result<T>
{
    /// <summary>Ініціалізує результат перевіреними складовими.</summary>
    /// <param name="isSuccess">Ознака успіху.</param>
    /// <param name="value">Значення успішного результату.</param>
    /// <param name="error">Опис помилки невдалого результату.</param>
    internal Result(bool isSuccess, T? value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    /// <summary>Отримує ознаку успішного результату.</summary>
    public bool IsSuccess { get; }

    /// <summary>Отримує значення успішного результату.</summary>
    public T? Value { get; }

    /// <summary>Отримує пояснення помилки.</summary>
    public string Error { get; }
}

/// <summary>Створює типізовані результати без статичних членів в узагальненому типі.</summary>
public static class Result
{
    /// <summary>Створює успішний результат.</summary>
    /// <typeparam name="T">Тип успішного значення.</typeparam>
    /// <param name="value">Перевірене значення.</param>
    /// <returns>Успішний результат.</returns>
    public static Result<T> Ok<T>(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Result<T>(true, value, string.Empty);
    }

    /// <summary>Створює невдалий результат.</summary>
    /// <typeparam name="T">Тип очікуваного значення.</typeparam>
    /// <param name="error">Зрозуміле пояснення помилки.</param>
    /// <returns>Невдалий результат.</returns>
    public static Result<T> Fail<T>(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        return new Result<T>(false, default, error);
    }
}
