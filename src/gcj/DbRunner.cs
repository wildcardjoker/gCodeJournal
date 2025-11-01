namespace gcj;

#region Using Directives
using System.Globalization;
using gCodeJournal.ViewModel;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console;
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

        // The action coming from the menu will typically look like:
        //   "List All Customers" or "Add New Customer" etc.
        // We want to split that into the SubMenu part ("List All", "Add New", ...) and the remaining part ("Customers", "Customer"...).
        var (firstPart, remainder) = SplitActionIntoSubmenuAndRemainder(action);

        // Normalize values for decision-making
        var subMenuKey = firstPart ?? string.Empty;
        var target     = remainder ?? string.Empty;

        // Log the split:
        appLogger.LogDebug("Action split: Action='{SubMenu}', Target='{Target}'", subMenuKey, target);

        // Now branch on the submenu key and/or section/target as required
        switch (subMenuKey.ToLowerInvariant())
        {
            case var s when s.Equals(SubMenuListAll, StringComparison.OrdinalIgnoreCase):
                // list action -- you can use the 'section' or the 'target' to decide which list to call
                if (section.Equals("Customers", StringComparison.OrdinalIgnoreCase) || target.Equals("Customers", StringComparison.OrdinalIgnoreCase))
                {
                    await LogCustomerDetailsAsync(vm, appLogger).ConfigureAwait(false);
                    return;
                }

                if (section.Equals("Manufacturers", StringComparison.OrdinalIgnoreCase) || target.Equals("Manufacturers", StringComparison.OrdinalIgnoreCase))
                {
                    await LogManufacturerDetailsAsync(vm, appLogger).ConfigureAwait(false);
                    return;
                }

                if (section.Equals("Filaments", StringComparison.OrdinalIgnoreCase) || target.Equals("Filaments", StringComparison.OrdinalIgnoreCase))
                {
                    await LogFilamentDetailsAsync(vm, appLogger).ConfigureAwait(false);
                    return;
                }

                // ... handle other list targets
                break;

            case var s when s.Equals(SubMenuAddNew, StringComparison.OrdinalIgnoreCase):
                appLogger.LogInformation("Add new requested for '{Target}'", target);

                // Implement interactive prompts / DTO creation and call view model Add* methods
                return;

            case var s when s.Equals(SubMenuUpdateExisting, StringComparison.OrdinalIgnoreCase):
                appLogger.LogInformation("Update requested for '{Target}'", target);

                // Implement edit flow
                return;

            case var s when s.Equals(SubMenuDeleteExisting, StringComparison.OrdinalIgnoreCase):
                appLogger.LogInformation("Delete requested for '{Target}'", target);

                // Implement delete flow
                return;

            case var s when s.Equals(SubMenuBackToMain, StringComparison.OrdinalIgnoreCase):
                // no-op, handled by menu
                return;
        }

        // Fallback behaviour: use section to decide what to do
        switch (section)
        {
            case "Customers":
                await LogCustomerDetailsAsync(vm, appLogger).ConfigureAwait(false);
                break;
            case "Manufacturers":
                await LogManufacturerDetailsAsync(vm, appLogger).ConfigureAwait(false);
                break;
            case "Filaments":
                await LogFilamentDetailsAsync(vm, appLogger).ConfigureAwait(false);
                break;
            default:
                appLogger.LogWarning(Emoji.Known.Warning + "  Unhandled section '{Section}' / action '{Action}'", section, action);
                break;
        }
    }

    /// <summary>
    ///     Split an interactive action string into two parts:
    ///     - firstPart: a value from SubMenu that matches the start of the action (case-insensitive), if any
    ///     - remainder: the remainder of the action after the matched SubMenu entry
    ///     If no SubMenu value matches the start, returns (first token, remainder).
    /// </summary>
    private static (string FirstPart, string Remainder) SplitActionIntoSubmenuAndRemainder(string action)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            return (string.Empty, string.Empty);
        }

        var trimmed = action.Trim();

        // Try to match any SubMenu entry at the start.
        // Order by length descending so longer entries (e.g. "Back to Main Menu") are matched before shorter ones ("Add New").
        foreach (var sub in SubMenu.OrderByDescending(s => s.Length))
        {
            if (!trimmed.StartsWith(sub, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var remainder = trimmed[sub.Length..].Trim().Dehumanize();
            return (sub, remainder);
        }

        // If none matched at start, try to find the first SubMenu entry anywhere in the string
        // and treat it as the first part if it appears before other text.
        foreach (var sub in SubMenu.OrderByDescending(s => s.Length))
        {
            var idx = CultureInfo.InvariantCulture.CompareInfo.IndexOf(trimmed, sub, CompareOptions.IgnoreCase);
            if (idx < 0)
            {
                continue;
            }

            // If there is nothing meaningful before the matched item, treat it as the first part.
            var before = trimmed[..idx].Trim();
            var after  = trimmed[(idx + sub.Length)..].Trim();
            if (string.IsNullOrEmpty(before))
            {
                return (sub, after);
            }
        }

        // Fallback: split on the first whitespace token
        var firstSpace = trimmed.IndexOf(' ');
        return firstSpace < 0 ? (trimmed, string.Empty) : (trimmed[..firstSpace], trimmed[(firstSpace + 1)..].Trim());
    }
}