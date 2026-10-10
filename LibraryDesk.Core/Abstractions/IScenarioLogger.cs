using LibraryDesk.Core.Domain;

namespace LibraryDesk.Core.Abstractions;

/// <summary>Записує безпечні події прикладних сценаріїв.</summary>
public interface IScenarioLogger
{
    /// <summary>Записує одну подію без персональних даних.</summary>
    /// <param name="level">Рівень важливості.</param>
    /// <param name="scenario">Ідентифікатор сценарію UC-01–UC-06.</param>
    /// <param name="message">Технічне повідомлення без вмісту файлів і паролів.</param>
    void Write(ScenarioLogLevel level, string scenario, string message);
}
