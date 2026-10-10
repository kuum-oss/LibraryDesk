using System.Text;
using LibraryDesk.App;
using LibraryDesk.Core;
using LibraryDesk.Core.Abstractions;
using LibraryDesk.Core.Pricing;
using LibraryDesk.Core.Services;
using LibraryDesk.Core.Storage;

Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine($"LibraryDesk v{VersionInfo.Current()}");

string dataDirectory = Path.Combine(Environment.CurrentDirectory, "data");
string dataPath = Path.Combine(dataDirectory, "library.json");
string logPath = Path.Combine(dataDirectory, "librarydesk.log");
IClock clock = new SystemClock();
ILibraryRepository repository = new JsonLibraryRepository(dataPath);
FileScenarioLogger scenarioLogger = new(logPath, clock);
SampleDataSeeder.Seed(repository);
LibraryService service = new(repository, clock, new LateFeePolicy(), scenarioLogger);
ConsoleApplication application = new(service, Console.In, Console.Out);

try
{
    application.Run();
}
catch (Exception exception)
{
    scenarioLogger.Write(
        LibraryDesk.Core.Domain.ScenarioLogLevel.Error,
        "APP",
        $"Непередбачений збій: {exception.GetType().Name}.");
    Console.Error.WriteLine($"Непередбачена помилка: {exception.Message}");
    Environment.ExitCode = 2;
}
