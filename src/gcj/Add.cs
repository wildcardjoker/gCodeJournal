namespace gcj
{
    #region Using Directives
    using gCodeJournal.ViewModel;
    using gCodeJournal.ViewModel.DTOs;
    using Microsoft.Extensions.Logging;
    using Spectre.Console;
    #endregion

    public static partial class Program
    {
        private static async Task AddCustomerAsync(IGCodeJournalViewModel vm, ILogger appLogger)
        {
            var customerName = await AnsiConsole.AskAsync<string>("Please enter the customer's name").ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(customerName))
            {
                appLogger.LogError(Emoji.Known.Warning + "  Customer name cannot be empty");
                return;
            }

            await vm.AddCustomerAsync(new CustomerDto(customerName)).ConfigureAwait(false);
            appLogger.LogInformation(Emoji.Known.CheckMark + " Added customer {Name}", customerName);
        }

        private static async Task AddFilamentAsync(IGCodeJournalViewModel vm, ILogger appLogger)
        {
            var colours       = await vm.GetAllFilamentColoursAsync().ConfigureAwait(false);
            var filamentTypes = await vm.GetAllFilamentTypesAsync().ConfigureAwait(false);
            var manufacturers = await vm.GetAllManufacturersAsync().ConfigureAwait(false);
            if (!colours.Any() || !filamentTypes.Any() || !manufacturers.Any())
            {
                string missing;
                if (colours.Any())
                {
                    missing = filamentTypes.Any() ? "manufacturers" : "filament types";
                }
                else
                {
                    missing = "colours";
                }

                appLogger.LogError("No {MissingElement} found; cannot create a Filament without colour", missing);
                return;
            }

            var manufacturer = await manufacturers.GetEntitySelectionAsync().ConfigureAwait(false);
            if (manufacturer is null)
            {
                // User chose to go back to the menu
                appLogger.LogInformation("Returning to menu");
                return;
            }

            var filamentType = await filamentTypes.GetEntitySelectionAsync().ConfigureAwait(false);
            if (filamentType is null)
            {
                // User chose to go back to the menu
                appLogger.LogInformation("Returning to menu");
                return;
            }

            var colour = await colours.GetEntitySelectionAsync().ConfigureAwait(false);
            if (colour is null)
            {
                // User chose to go back to the menu
                appLogger.LogInformation("Returning to menu");
                return;
            }

            var costPerWeight = await AnsiConsole.PromptAsync(new TextPrompt<decimal?>("Please enter the cost/weight (ENTER for no cost):").AllowEmpty())
                                                 .ConfigureAwait(false)
                                ?? 0m;
            var productCode = await AnsiConsole.PromptAsync(new TextPrompt<string?>("Please enter the product code (ENTER for none):").AllowEmpty()).ConfigureAwait(false);
            var productUrl  = await AnsiConsole.PromptAsync(new TextPrompt<string?>("Please enter the product URL (ENTER for none):").AllowEmpty()).ConfigureAwait(false);
            var filamentDto = new FilamentDto(costPerWeight, productCode, productUrl, colour, filamentType, manufacturer);
            await vm.AddFilamentAsync(filamentDto).ConfigureAwait(false);
            appLogger.LogInformation(Emoji.Known.CheckMark + " Added filament {Filament}", filamentDto);
        }

        private static async Task AddFilamentColourAsync(IGCodeJournalViewModel vm, ILogger appLogger) => throw new NotImplementedException();

        private static async Task AddFilamentTypeAsync(IGCodeJournalViewModel vm, ILogger appLogger) => throw new NotImplementedException();

        private static async Task AddManufacturerAsync(IGCodeJournalViewModel vm, ILogger appLogger)
        {
            var manufacturerName = await AnsiConsole.AskAsync<string>("Please enter the manufacturer's name").ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(manufacturerName))
            {
                appLogger.LogError(Emoji.Known.Warning + "  Manufacturer name cannot be empty");
                return;
            }

            await vm.AddManufacturerAsync(new ManufacturerDto(manufacturerName)).ConfigureAwait(false);
            appLogger.LogInformation(Emoji.Known.CheckMark + " Added manufacturer {Name}", manufacturerName);
        }

        private static async Task AddModelDesignAsync(IGCodeJournalViewModel vm, ILogger appLogger) => throw new NotImplementedException();

        private static async Task AddPrintingProjectAsync(IGCodeJournalViewModel vm, ILogger appLogger) => throw new NotImplementedException();
    }
}