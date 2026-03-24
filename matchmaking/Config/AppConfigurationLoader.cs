using System;
using System.IO;
using System.Text.Json;

namespace matchmaking.Config;

public static class AppConfigurationLoader
{
    public static AppConfiguration Load()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(configPath))
        {
            return new AppConfiguration();
        }

        var json = File.ReadAllText(configPath);
        using var document = JsonDocument.Parse(json);

        var configuration = new AppConfiguration();
        if (document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings)
            && connectionStrings.TryGetProperty("SqlServer", out var sqlConnectionString))
        {
            configuration.SqlConnectionString = sqlConnectionString.GetString() ?? string.Empty;
        }

        return configuration;
    }
}
