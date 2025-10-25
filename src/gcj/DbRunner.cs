#region Using Directives
#endregion

namespace gcj;

#region Using Directives
using Microsoft.Extensions.Logging;
#endregion

public static partial class Program
{
    private static async Task LogCustomerDetailsAsync()
    {
        // Extract data from the database before writing to the console; this will log any warnings if we're logging sensitive data
        var customers = await _context.GetCustomersAsync().ConfigureAwait(false);
        _logger.LogInformation("Customers:");
        foreach (var customer in customers)
        {
            _logger.LogInformation(" {CustomerId}: {Customer}", customer.Id, customer.Name);
        }
    }

    private static async Task LogFilamentDetailsAsync()
    {
        _logger.LogInformation("Filaments:");
        var filaments = await _context.GetFilamentsAsync().ConfigureAwait(false);
        foreach (var filament in filaments)
        {
            _logger.LogInformation(" {FilamentId}: {Filament}", filament.Id, filament);
        }
    }

    private static async Task LogManufacturerDetailsAsync()
    {
        // Get and log Manufacturers and Filaments
        var manufacturers = await _context.GetManufacturersAsync().ConfigureAwait(false);
        _logger.LogInformation("Manufacturers:");
        foreach (var manufacturer in manufacturers)
        {
            _logger.LogInformation(" {ManufacturerId}: {Manufacturer}", manufacturer.Id, manufacturer);
        }
    }
}