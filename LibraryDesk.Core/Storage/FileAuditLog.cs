using System.Globalization;

namespace LibraryDesk.Core.Storage;

/// <summary>Володіє файловим потоком і послідовно записує події аудиту.</summary>
public sealed class FileAuditLog : IDisposable
{
    private readonly StreamWriter _writer;
    private bool _disposed;

    /// <summary>Відкриває файл аудиту для дописування.</summary>
    /// <param name="path">Шлях до файла аудиту.</param>
    public FileAuditLog(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _writer = new StreamWriter(path, append: true);
    }

    /// <summary>Записує одну структуровану подію аудиту.</summary>
    /// <param name="operation">Назва операції.</param>
    /// <param name="loanId">Ідентифікатор формуляра.</param>
    public void Write(string operation, int loanId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(loanId);
        _writer.WriteLine(
            string.Create(
                CultureInfo.InvariantCulture,
                $"{DateTimeOffset.Now:O};{operation};{loanId}"));
        _writer.Flush();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _writer.Flush();
        _writer.Dispose();
        _disposed = true;
    }
}
