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
using ILogger = ILogger;
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
    private static IGCodeJournalViewModel _context = null!;
    private static ILogger                _logger  = null!;
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
        var config = BuildConfiguration();

        // runtime toggle for EF query logging (read early so we can adjust Serilog config)
        var enableQueryLogging = config.GetValue("EnableQueryLogging", false);

        // apply EF logging override into configuration
        config = ApplyEfLoggingOverride(config, enableQueryLogging);

        // Ensure log file path and display location
        var logFilePath = EnsureLogFilePath(config);
        $"Log file location: {Path.GetDirectoryName(Environment.ExpandEnvironmentVariables(logFilePath))}".DisplayInfoMessage();

        // Configure Serilog
        ConfigureSerilog(config);

        using var loggerFactory = CreateLoggerFactory(enableQueryLogging);

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
        var optionsBuilder = ConfigureDbContextOptions(dbPath, loggerFactory);

#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging(); // shows parameter values in DEBUG mode
#endif

        // Setup DI
        var services = new ServiceCollection();

        // Register Serilog as the logging provider and clear default providers to avoid duplicate output
        services.AddLogging(b => b.ClearProviders().AddSerilog(Log.Logger));
        services.AddDbContext<GCodeJournalDbContext>(opts => opts.UseLazyLoadingProxies()
                                                                 .UseSqlite($"Data Source={dbPath}")
                                                                 .EnableDetailedErrors()
                                                                 .UseLoggerFactory(loggerFactory));
        services.AddScoped<IGCodeJournalViewModel, GCodeJournalViewModel>();

        await using var provider = services.BuildServiceProvider();

        // Create an application logger from the provider
        var loggerFactoryFromDI = provider.GetRequiredService<ILoggerFactory>();
        var appLogger           = loggerFactoryFromDI.CreateLogger("gcj");
        appLogger.LogTrace($"{ThisAssembly.AssemblyTitle} {ThisAssembly.AssemblyInformationalVersion} ({ThisAssembly.AssemblyConfiguration}) started");
        $"Using DB path: {dbPath}".DisplayInfoMessage();
        appLogger.LogTrace("Using DB Path {DBPath}", dbPath);

        await DisplayMenuAsync(provider, appLogger).ConfigureAwait(false);
        appLogger.LogTrace("Run finished");
        await Log.CloseAndFlushAsync().ConfigureAwait(false);
    }
}