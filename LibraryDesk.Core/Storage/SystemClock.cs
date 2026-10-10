using LibraryDesk.Core.Abstractions;

namespace LibraryDesk.Core.Storage;

/// <summary>Надає поточний системний час для робочого застосунку.</summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTimeOffset Now => DateTimeOffset.Now;
}
