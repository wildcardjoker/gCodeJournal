#region Using Directives
using gCodeJournal.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
#endregion

// Build a configuration object from JSON file
IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

// Get database path from config, with a default value
// Expands %VAR% on Windows or other env vars
var dbPath = Environment.ExpandEnvironmentVariables(config["gcodeJournalDbPath"] ?? "gCodeJournal.db");

if (!File.Exists(dbPath))
{
    Console.WriteLine($"ERROR: Could not find database {dbPath}");
    return;
}

// Configure DbContext options - enable lazy-loading proxies
var optionsBuilder = new DbContextOptionsBuilder<GCodeJournalDbContext>().UseLazyLoadingProxies().UseSqlite($"Data Source={dbPath}").EnableDetailedErrors();

if (config.GetValue("EnableQueryLogging", false)) // Enable query logging if specified in config
{
    Console.WriteLine("*** Queries will be logged ***");
    optionsBuilder.LogTo(Console.WriteLine, [DbLoggerCategory.Database.Command.Name], LogLevel.Information);
}

#if DEBUG
Console.WriteLine("*** DEBUG: sensitive data will be logged ***");
optionsBuilder.EnableSensitiveDataLogging(); // shows parameter values in DEBUG mode
#endif

var             options = optionsBuilder.Options;
await using var context = new GCodeJournalDbContext(options);
Console.WriteLine($"Using DB path: {dbPath}");

Console.WriteLine("\nManufacturers:");
var manufacturers = context.Manufacturers.OrderBy(m => m.Name);
foreach (var manufacturer in manufacturers)
{
    Console.WriteLine($"  {manufacturer.Id}: {manufacturer}");
}

Console.WriteLine("\nFilaments:");
var filaments = context.Filaments.OrderBy(f => f.Manufacturer);
foreach (var filament in filaments)
{
    Console.WriteLine($"  {filament.Id}: {filament}");
}