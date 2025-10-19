#region Using Directives
using gCodeJournal.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
#endregion

// Build a configuration object from JSON file
IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

// Do we want to enable query logging?
var enableQueryLogging = config.GetValue("EnableQueryLogging", false);

// Create LoggerFactory configured from appsettings.json
using var loggerFactory = LoggerFactory.Create(builder =>
                                               {
                                                   builder.AddConfiguration(config.GetSection("Logging")); // Get logging config from appsettings.json
                                                   builder.AddConsole();                                   // Log to console

                                                   // Adjust EF query logging at runtime:
                                                   // - when EnableQueryLogging == true, allow Trace (or whatever level you prefer)
                                                   // - when false, raise EF Command logging to Warning to suppress SQL output
                                                   builder.AddFilter(
                                                       "Microsoft.EntityFrameworkCore.Database.Command",

                                                       // prevent SQL text from being logged unless enabled
                                                       enableQueryLogging ? LogLevel.Trace : LogLevel.Warning);
                                               });

var logger = loggerFactory.CreateLogger("gcj");
if (config.GetValue("EnableQueryLogging", false)) // Enable query logging if specified in config
{
    //optionsBuilder.LogTo(Console.WriteLine, [DbLoggerCategory.Database.Command.Name], LogLevel.Information);
    // Modify loglevel to prevent logging queries
}

// Get database path from config, with a default value
// Expands %VAR% on Windows or other env vars
var dbPath = Environment.ExpandEnvironmentVariables(config["gcodeJournalDbPath"] ?? "gCodeJournal.db");

if (!File.Exists(dbPath))
{
    logger.LogError("Could not find database {Path}", dbPath);
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

logger.LogInformation("\nManufacturers:");
var manufacturers = context.Manufacturers.OrderBy(m => m.Name);
foreach (var manufacturer in manufacturers)
{
    //logger.LogInformation("  {Id}: {Manufacturer}", manufacturer.Id, manufacturer);
    Console.WriteLine($"  {manufacturer.Id}: {manufacturer}");
}

logger.LogInformation("\nFilaments:");
var filaments = context.Filaments.OrderBy(f => f.Manufacturer.Name);
foreach (var filament in filaments)
{
    //logger.LogInformation("  {Id}: {Filament}", filament.Id, filament);
    Console.WriteLine($"  {filament.Id}: {filament}");
}