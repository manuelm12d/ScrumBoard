namespace ScrumBoard.Api.Configuration;

/// <summary>
/// Loads the repository .env into the process environment when the API runs outside Docker, so
/// "dotnet run" needs no extra setup while the file itself stays out of version control.
/// Variables already present in the environment always win.
/// </summary>
internal static class DotEnvLoader
{
    internal static void Load(string startDirectory)
    {
        var file = FindEnvFile(startDirectory);
        if (file is null)
        {
            return;
        }

        foreach (var line in File.ReadLines(file))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            {
                continue;
            }

            var separator = trimmed.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            var key = trimmed[..separator].Trim();
            var value = trimmed[(separator + 1)..].Trim().Trim('"');

            if (Environment.GetEnvironmentVariable(key) is null)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    private static string? FindEnvFile(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, ".env");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
