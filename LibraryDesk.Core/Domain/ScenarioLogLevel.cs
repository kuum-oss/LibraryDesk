namespace LibraryDesk.Core.Domain;

/// <summary>Рівень події прикладного сценарію.</summary>
public enum ScenarioLogLevel
{
    /// <summary>Нормальний початок або завершення операції.</summary>
    Information = 0,

    /// <summary>Очікуване відхилення запиту через правило або ввід.</summary>
    Warning = 1,

    /// <summary>Неочікуваний технічний збій із контекстом сценарію.</summary>
    Error = 2,
}
