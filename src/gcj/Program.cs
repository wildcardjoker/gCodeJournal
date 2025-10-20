#region Using Directives
using gCodeJournal.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
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
var writeTos    = config.GetSection("Serilog:WriteTo").GetChildren();
var fileSink    = writeTos.FirstOrDefault(s => string.Equals(s["Name"], "File", StringComparison.OrdinalIgnoreCase));
var logFilePath = fileSink?["Args:path"]; // may be "%LOCALAPPDATA%\\gCodeJournal\\gCodeJournal-.log"
if (string.IsNullOrEmpty(logFilePath))
{
    var defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "gCodeJournal", "gCodeJournal-.log");
    Console.WriteLine("*** WARNING: The log file path wasn't specified (Serilog:WriteTo:File:Args:path)");
    Console.WriteLine($"*** Defaulting to {defaultPath}");
    logFilePath = defaultPath;
}

// expand environment variables (you already do this when configuring Serilog)
var expanded = Environment.ExpandEnvironmentVariables(logFilePath);

// If you used the Serilog rolling pattern "name-.log" compute today's concrete file name.
// Serilog.Sinks.File usually appends the date as yyyyMMdd for RollingInterval.Day.
var activeLogFile = expanded;
if (expanded.EndsWith("-.log", StringComparison.OrdinalIgnoreCase))
{
    activeLogFile = expanded.Replace("-.log", $"-{DateTime.Now:yyyyMMdd}.log");
}

// Now you have the path to the file Serilog will write to (or wrote to) today:
Console.WriteLine($"Serilog configured file (expanded): {expanded}");
Console.WriteLine($"Current log file: {activeLogFile}");

// Configure Serilog from configuration
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(config).Enrich.FromLogContext().CreateLogger();

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
    logger.LogError("Could not find database {Path}", dbPath);
    Log.CloseAndFlush();
    return;
}

// Configure DbContext options - enable lazy-loading proxies and attach LoggerFactory so EF logs flow through the same pipeline
var optionsBuilder = new DbContextOptionsBuilder<GCodeJournalDbContext>().UseLazyLoadingProxies()
                                                                         .UseSqlite($"Data Source={dbPath}")
                                                                         .EnableDetailedErrors()
                                                                         .UseLoggerFactory(loggerFactory);

#if DEBUG
logger.LogWarning("*** DEBUG: sensitive data will be logged ***");
optionsBuilder.EnableSensitiveDataLogging(); // shows parameter values in DEBUG mode
#endif

var             options = optionsBuilder.Options;
await using var context = new GCodeJournalDbContext(options);
logger.LogInformation("Using DB path: {Path}", dbPath);
Console.WriteLine($"Using DB path: {dbPath}");
logger.LogInformation("Manufacturers:");
Console.WriteLine("\nManufacturers:");
var manufacturers = context.Manufacturers.OrderBy(m => m.Name);
foreach (var manufacturer in manufacturers)
{
    logger.LogInformation("  {Id}: {Manufacturer}", manufacturer.Id, manufacturer.ToString());
    Console.WriteLine($"  {manufacturer.Id}: {manufacturer}");
}

logger.LogInformation("Filaments:");
Console.WriteLine("\nFilaments:");
var filaments = context.Filaments.OrderBy(f => f.ManufacturerId);
foreach (var filament in filaments)
{
    logger.LogInformation("  {Id}: {Filament}", filament.Id, filament.ToString());
    Console.WriteLine($"  {filament.Id}: {filament}");
}

Log.CloseAndFlush();