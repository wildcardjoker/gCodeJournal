#region Using Directives
using ILogger = Microsoft.Extensions.Logging.ILogger;
#endregion

namespace gcj;

#region Using Directives
using gCodeJournal.Model;
using gCodeJournal.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Spectre.Console;
using ILogger = Microsoft.Extensions.Logging.ILogger;
#endregion

/// <summary>
///     Represents the main application class for gCodeJournal.
/// </summary>
/// <remarks>
///     This class serves as the entry point for the application and is responsible for initializing
///     the application's configuration, logging, and database context. It includes functionality
///     for configuring Serilog, setting up Entity Framework Core, and executing database operations.
/// </remarks>
public static partial class Program
{
    #region Fields
    private static GCodeJournalViewModel _context = null!;
    private static ILogger               _logger  = null!;
    #endregion

    /// <summary>
    ///     The entry point of the application.
    /// </summary>
    /// <param name="args">An array of command-line arguments passed to the application.</param>
    /// <remarks>
    ///     This method initializes the application configuration, logging, and database context.
    ///     It performs the following tasks:
    ///     - Configures Serilog for logging.
    ///     - Reads runtime settings from the configuration file.
    ///     - Validates and sets up the database path.
    ///     - Configures Entity Framework Core with lazy-loading proxies and logging.
    ///     - Executes database queries and logs details about customers, manufacturers, and filaments.
    /// </remarks>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build();

        // runtime toggle for EF query logging (read early so we can adjust Serilog config)
        var enableQueryLogging = config.GetValue("EnableQueryLogging", false);

        // Always set Serilog override for EF command category depending on runtime flag
        const string efOverrideKey = "Serilog:MinimumLevel:Override:Microsoft.EntityFrameworkCore.Database.Command";
        var          efLevel       = enableQueryLogging ? "Verbose" : "Warning";
        config = new ConfigurationBuilder().AddConfiguration(config).AddInMemoryCollection([new KeyValuePair<string, string?>(efOverrideKey, efLevel)]).Build();

        // Get the log file path from configuration
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

        // Get and display the expanded log file path
        AnsiConsole.MarkupLine($":information:  Serilog configured file (expanded): {Path.GetDirectoryName(Environment.ExpandEnvironmentVariables(logFilePath))}");

        // Configure Serilog from configuration and ensure Console sink is enabled by default if not configured explicitly.
        var loggerConfig = new LoggerConfiguration().ReadFrom.Configuration(config).Enrich.FromLogContext();

        // If no Console sink is configured in appsettings, add one so logs also go to the console.
        if (!writeTos.Any(s => string.Equals(s["Name"], "Console", StringComparison.OrdinalIgnoreCase)))
        {
            loggerConfig = loggerConfig.WriteTo.Console();
        }

        Log.Logger = loggerConfig.CreateLogger();

        using var loggerFactory = LoggerFactory.Create(builder =>
                                                       {
                                                           // wire Serilog into Microsoft logging
                                                           builder.AddSerilog(Log.Logger);

                                                           // Adjust EF command logging based on runtime flag for Microsoft logging pipeline too
                                                           if (!enableQueryLogging)
                                                           {
                                                               builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
                                                           }
                                                       });

        // Get database path from config, with a default value
        var dbPath = Environment.ExpandEnvironmentVariables(config["gcodeJournalDbPath"] ?? "gCodeJournal.db");

        if (!File.Exists(dbPath))
        {
            var tempLogger = loggerFactory.CreateLogger("gcj");
            tempLogger.LogError("Could not find path {DbPath}", dbPath);
            await Log.CloseAndFlushAsync().ConfigureAwait(false);
            return;
        }

        // Configure DbContext options - enable lazy-loading proxies and attach LoggerFactory so EF logs flow through the same pipeline
        var optionsBuilder = new DbContextOptionsBuilder<GCodeJournalDbContext>().UseLazyLoadingProxies()
                                                                                 .UseSqlite($"Data Source={dbPath}")
                                                                                 .EnableDetailedErrors()
                                                                                 .UseLoggerFactory(loggerFactory);

#if DEBUG

        optionsBuilder.EnableSensitiveDataLogging(); // shows parameter values in DEBUG mode
#endif

        // Setup DI
        var services = new ServiceCollection();
        services.AddSingleton(loggerFactory);
        services.AddLogging(b => b.AddSerilog(Log.Logger));
        services.AddDbContext<GCodeJournalDbContext>(opts => opts.UseLazyLoadingProxies()
                                                                 .UseSqlite($"Data Source={dbPath}")
                                                                 .EnableDetailedErrors()
                                                                 .UseLoggerFactory(loggerFactory));
        services.AddScoped<GCodeJournalViewModel>();

        using var provider = services.BuildServiceProvider();
        using var scope    = provider.CreateScope();

        // Resolve logger and viewmodel from DI
        var loggerFactoryFromDI = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        _logger  = loggerFactoryFromDI.CreateLogger("gcj");
        _context = scope.ServiceProvider.GetRequiredService<GCodeJournalViewModel>();

        AnsiConsole.MarkupLine($":information:  Using DB path: {dbPath}");
        _logger.LogTrace("Using DB Path {DBPath}", dbPath);

        // Execute using resolved scope (DbContext will be disposed when scope is disposed)
        await LogCustomerDetailsAsync().ConfigureAwait(false);
        await LogManufacturerDetailsAsync().ConfigureAwait(false);
        await LogFilamentDetailsAsync().ConfigureAwait(false);
        await Log.CloseAndFlushAsync().ConfigureAwait(false);
    }
}