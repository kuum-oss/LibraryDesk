using System.Reflection;

namespace LibraryDesk.Core;

/// <summary>Надає семантичну версію збірки ядра.</summary>
public static class VersionInfo
{
    /// <summary>Повертає версію без метаданих складання.</summary>
    /// <returns>Версія з <c>Directory.Build.props</c>.</returns>
    public static string Current()
    {
        Assembly assembly = typeof(VersionInfo).Assembly;
        AssemblyInformationalVersionAttribute? attribute =
            assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        string raw = attribute?.InformationalVersion ?? "0.0.0";
        int plus = raw.IndexOf('+', StringComparison.Ordinal);
        return plus < 0 ? raw : raw[..plus];
    }
}
