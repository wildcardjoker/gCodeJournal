// gcj

namespace gcj;

#region Using Directives
using gCodeJournal.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Spectre.Console;
#endregion

public static partial class Program
{
    private static IConfiguration ApplyEfLoggingOverride(IConfiguration config, bool enableQueryLogging)
    {
        const string efOverrideKey = "Serilog:MinimumLevel:Override:Microsoft.EntityFrameworkCore.Database.Command";
        var          efLevel       = enableQueryLogging ? "Verbose" : "Warning";
        return new ConfigurationBuilder().AddConfiguration(config).AddInMemoryCollection([new KeyValuePair<string, string?>(efOverrideKey, efLevel)]).Build();
    }

    private static IConfiguration BuildConfiguration() => new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build();

    private static DbContextOptionsBuilder<GCodeJournalDbContext> ConfigureDbContextOptions(string dbPath, ILoggerFactory loggerFactory) =>
        new DbContextOptionsBuilder<GCodeJournalDbContext>().UseLazyLoadingProxies()
                                                            .UseSqlite($"Data Source={dbPath}")
                                                            .EnableDetailedErrors()
                                                            .UseLoggerFactory(loggerFactory);

    private static void ConfigureSerilog(IConfiguration config)
    {
        var loggerConfig = CreateLoggerConfiguration(config);
        Log.Logger = loggerConfig.CreateLogger();
    }

    private static LoggerConfiguration CreateLoggerConfiguration(IConfiguration config)
    {
        var loggerConfig = new LoggerConfiguration().ReadFrom.Configuration(config).Enrich.FromLogContext();

        var writeTos = config.GetSection("Serilog:WriteTo").GetChildren().ToList();
        if (!writeTos.Any(s => string.Equals(s["Name"], "Console", StringComparison.OrdinalIgnoreCase)))
        {
            loggerConfig = loggerConfig.WriteTo.Console();
        }

        return loggerConfig;
    }

    private static ILoggerFactory CreateLoggerFactory(bool enableQueryLogging)
    {
        return LoggerFactory.Create(builder =>
                                    {
                                        // wire Serilog into Microsoft logging
                                        builder.AddSerilog(Log.Logger);

                                        // Adjust EF command logging based on runtime flag for Microsoft logging pipeline too
                                        if (!enableQueryLogging)
                                        {
                                            builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
                                        }
                                    });
    }

    private static string EnsureLogFilePath(IConfiguration config)
    {
        var writeTos    = config.GetSection("Serilog:WriteTo").GetChildren().ToList();
        var fileSink    = writeTos.FirstOrDefault(s => string.Equals(s["Name"], "File", StringComparison.OrdinalIgnoreCase));
        var logFilePath = fileSink?["Args:path"]; // Get the path to the log file. Can be null or empty (not configured)
        if (string.IsNullOrEmpty(logFilePath))
        {
            var defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "gCodeJournal", "gCodeJournal-.log");
            AnsiConsole.MarkupLine(":warning: WARNING: The log file path wasn't specified (Serilog:WriteTo:File:Args:path)");
            AnsiConsole.MarkupLine($":warning: Defaulting to {defaultPath}");
            logFilePath = defaultPath;
        }

        return logFilePath!;
    }
}