namespace gcj;

#region Using Directives
using gCodeJournal.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
#endregion

public static partial class Program
{
    private static async Task LogCustomerDetailsAsync(IGCodeJournalViewModel vm, ILogger logger)
    {
        // Extract data from the database before writing to the console; this will log any warnings if we're logging sensitive data
        var customers = await vm.GetAllCustomersAsync().ConfigureAwait(false);
        logger.LogInformation("Customers:");
        foreach (var customer in customers)
        {
            logger.LogInformation(" {CustomerId}: {Customer}", customer.Id, customer.Name);
        }
    }

    private static async Task LogFilamentDetailsAsync(IGCodeJournalViewModel vm, ILogger logger)
    {
        logger.LogInformation("Filaments:");
        var filaments = await vm.GetAllFilamentsAsync().ConfigureAwait(false);
        foreach (var filament in filaments)
        {
            logger.LogInformation(" {FilamentId}: {Filament}", filament.Id, filament);
        }
    }

    private static async Task LogManufacturerDetailsAsync(IGCodeJournalViewModel vm, ILogger logger)
    {
        // Get and log Manufacturers and Filaments
        var manufacturers = await vm.GetAllManufacturersAsync().ConfigureAwait(false);
        logger.LogInformation("Manufacturers:");
        foreach (var manufacturer in manufacturers)
        {
            logger.LogInformation(" {ManufacturerId}: {Manufacturer}", manufacturer.Id, manufacturer);
        }
    }

    private static async Task ProcessDatabaseActionAsync(string section, string action, ServiceProvider provider, ILogger appLogger)
    {
        using var scope = provider.CreateScope();
        var       vm    = scope.ServiceProvider.GetRequiredService<IGCodeJournalViewModel>();

        // Process based on section and action
        // For now, we'll just log some details as a demonstration
        await LogFilamentDetailsAsync(vm, appLogger).ConfigureAwait(false);
    }
}