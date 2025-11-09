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
            var customerName = await "customer's name".GetInputFromConsoleAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(customerName))
            {
                appLogger.LogError(Emoji.Known.Warning + "  Customer name cannot be empty");
                return;
            }

            await vm.AddCustomerAsync(new CustomerDto(customerName)).ConfigureAwait(false);
            appLogger.LogInformation(Emoji.Known.CheckMarkButton + " Added customer {Name}", customerName);
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
            appLogger.LogInformation(Emoji.Known.CheckMarkButton + " Added filament {Filament}", filamentDto);
        }

        private static async Task AddFilamentColourAsync(IGCodeJournalViewModel vm, ILogger appLogger)
        {
            var filamentColour = await "filament colour".GetInputFromConsoleAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(filamentColour))
            {
                appLogger.LogError(Emoji.Known.Warning + "  filament colour cannot be empty");
                return;
            }

            await vm.AddFilamentColourAsync(new FilamentColourDto(filamentColour)).ConfigureAwait(false);
            appLogger.LogInformation(Emoji.Known.CheckMarkButton + " Added filament colour {Colour}", filamentColour);
        }

        private static async Task AddFilamentTypeAsync(IGCodeJournalViewModel vm, ILogger appLogger)
        {
            var filamentType = await "filament type".GetInputFromConsoleAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(filamentType))
            {
                appLogger.LogError(Emoji.Known.Warning + "  filament type cannot be empty");
                return;
            }

            await vm.AddFilamentTypeAsync(new FilamentTypeDto(filamentType)).ConfigureAwait(false);
            appLogger.LogInformation(Emoji.Known.CheckMarkButton + " Added filament type {Type}", filamentType);
        }

        private static async Task AddManufacturerAsync(IGCodeJournalViewModel vm, ILogger appLogger)
        {
            var manufacturerName = await AnsiConsole.AskAsync<string>("Please enter the manufacturer's name").ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(manufacturerName))
            {
                appLogger.LogError(Emoji.Known.Warning + "  Manufacturer name cannot be empty");
                return;
            }

            await vm.AddManufacturerAsync(new ManufacturerDto(manufacturerName)).ConfigureAwait(false);
            appLogger.LogInformation(Emoji.Known.CheckMarkButton + " Added manufacturer {Name}", manufacturerName);
        }

        private static async Task AddModelDesignAsync(IGCodeJournalViewModel vm, ILogger appLogger)
        {
            var summary     = await "model summary".GetInputFromConsoleAsync().ConfigureAwait(false);
            var description = await "model description".GetMultiLineInputAsync().ConfigureAwait(false);
            var length      = await "model length in m".GetInputFromConsoleAsync<decimal>().ConfigureAwait(false);
            var url         = await "model URL".GetInputFromConsoleAsync().ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(summary))
            {
                appLogger.LogError(Emoji.Known.Warning + "  Summary cannot be empty");
                return;
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                appLogger.LogError(Emoji.Known.Warning + "  Description cannot be empty");
                return;
            }

            await vm.AddModelDesignAsync(new ModelDesignDto(description, length, summary, url)).ConfigureAwait(false);
            appLogger.LogInformation(Emoji.Known.CheckMarkButton + " Added model design {Summary}", summary);
        }

        private static async Task AddPrintingProjectAsync(IGCodeJournalViewModel vm, ILogger appLogger) => throw new NotImplementedException();
    }
}