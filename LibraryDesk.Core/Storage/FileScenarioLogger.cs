using System.Globalization;
using LibraryDesk.Core.Abstractions;
using LibraryDesk.Core.Domain;

namespace LibraryDesk.Core.Storage;

/// <summary>Дописує структуровані події сценаріїв до текстового файла.</summary>
public sealed class FileScenarioLogger : IScenarioLogger
{
    private readonly string _path;
    private readonly IClock _clock;

    /// <summary>Створює файловий журнал із замінним джерелом часу.</summary>
    /// <param name="path">Шлях до файла журналу.</param>
    /// <param name="clock">Джерело часу запису.</param>
    public FileScenarioLogger(string path, IClock clock)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _path = path;
    }

    /// <inheritdoc />
    public void Write(ScenarioLogLevel level, string scenario, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scenario);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        string? directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string safeMessage = message.Replace('\r', ' ').Replace('\n', ' ');
        string line = string.Create(
            CultureInfo.InvariantCulture,
            $"{_clock.Now:O};{level};{scenario};{safeMessage}{Environment.NewLine}");
        File.AppendAllText(_path, line);
    }
}
