#region Using Directives
using gCodeJournal.Model;
using gCodeJournal.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Spectre.Console;
#endregion

// build configuration
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

var logger = loggerFactory.CreateLogger("gcj");

// Get database path from config, with a default value
var dbPath = Environment.ExpandEnvironmentVariables(config["gcodeJournalDbPath"] ?? "gCodeJournal.db");

if (!File.Exists(dbPath))
{
    logger.LogError("Could not find path {DbPath}", dbPath);
    Log.CloseAndFlush();
    return;
}

// Configure DbContext options - enable lazy-loading proxies and attach LoggerFactory so EF logs flow through the same pipeline
var optionsBuilder = new DbContextOptionsBuilder<GCodeJournalDbContext>().UseLazyLoadingProxies()
                                                                         .UseSqlite($"Data Source={dbPath}")
                                                                         .EnableDetailedErrors()
                                                                         .UseLoggerFactory(loggerFactory);

#if DEBUG

//logger.LogWarning("*** DEBUG: sensitive data will be logged ***");
optionsBuilder.EnableSensitiveDataLogging(); // shows parameter values in DEBUG mode
#endif

// Create ViewModel (which inherits GCodeJournalDbContext) and query some data
var             options = optionsBuilder.Options;
await using var context = new GCodeJournalViewModel(options);

AnsiConsole.MarkupLine($":information:  Using DB path: {dbPath}");
logger.LogTrace("Using DB Path {DBPath}", dbPath);

// Extract data from the database before writing to the console; this will log any warnings if we're logging sensitive data
var customers = await context.GetCustomersAsync().ConfigureAwait(false);
logger.LogInformation("Customers");
foreach (var customer in customers)
{
    logger.LogInformation(" {CustomerId}: {Customer}", customer.Id, customer.Name);
}

// Get and log Manufacturers and Filaments
var manufacturers = await context.GetManufacturersAsync().ConfigureAwait(false);
logger.LogInformation("Manufacturers:");
foreach (var manufacturer in manufacturers)
{
    logger.LogInformation(" {ManufacturerId}: {Manufacturer}", manufacturer.Id, manufacturer);
}

logger.LogInformation("Filaments:");
var filaments = await context.GetFilamentsAsync().ConfigureAwait(false);
foreach (var filament in filaments)
{
    logger.LogInformation(" {FilamentId}: {Filament}", filament.Id, filament);
}

Log.Information("End of run");
Log.CloseAndFlush();