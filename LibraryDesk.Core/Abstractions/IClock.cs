namespace LibraryDesk.Core.Abstractions;

/// <summary>Надає поточний час без прямої залежності від системного годинника.</summary>
public interface IClock
{
    /// <summary>Поточні дата й час із часовим зміщенням.</summary>
    DateTimeOffset Now { get; }
}
