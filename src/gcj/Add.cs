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

        private static async Task AddPrintingProjectAsync(IGCodeJournalViewModel vm, ILogger appLogger)
        {
            var customers = await vm.GetAllCustomersAsync().ConfigureAwait(false);
            var models    = await vm.GetAllModelDesignsAsync().ConfigureAwait(false);
            var filaments = await vm.GetAllFilamentsAsync().ConfigureAwait(false);
            if (!customers.Any() || !models.Any() || !filaments.Any())
            {
                string missing;
                if (customers.Any())
                {
                    missing = models.Any() ? "filaments" : "models";
                }
                else
                {
                    missing = "customers";
                }

                appLogger.LogError("No {MissingElement} found; cannot create a project without it", missing);
                return;
            }

            var customer = await customers.GetEntitySelectionAsync().ConfigureAwait(false);
            if (customer is null)
            {
                // User chose to go back to the menu
                appLogger.LogInformation("Returning to menu");
                return;
            }

            appLogger.LogInformation(Emoji.Known.OkButton + " Selected customer: {Customer}", customer);

            var model = await models.GetEntitySelectionAsync().ConfigureAwait(false);
            if (model is null)
            {
                // User chose to go back to the menu
                appLogger.LogInformation("Returning to menu");
                return;
            }

            appLogger.LogInformation(Emoji.Known.OkButton + " Selected model: {Model}", model);

            // Collect multiple filaments until the user elects to return to the menu.
            var selectedFilaments = new List<FilamentDto>();
            while (true)
            {
                var selected = await filaments.GetEntitySelectionAsync().ConfigureAwait(false);
                if (selected is null)
                {
                    if (!selectedFilaments.Any())
                    {
                        // User chose to return immediately -> cancel project creation.
                        appLogger.LogInformation("Returning to menu");
                        return;
                    }

                    // User has finished selecting filaments; exit selection loop and continue.
                    break;
                }

                selectedFilaments.Add(selected);
                appLogger.LogInformation(Emoji.Known.OkButton + " Added filament {Filament}", selected);

                // Loop will prompt again to allow multiple selections until the user chooses to return.
            }

            var       cost          = await "cost".GetInputFromConsoleAsync<decimal>().ConfigureAwait(false);
            var       dateSubmitted = await "date submitted (yyyy-MM-dd)".GetInputFromConsoleAsync<DateOnly>().ConfigureAwait(false);
            DateOnly? dateCompleted = await "date completed (yyyy-MM-dd)".GetInputFromConsoleAsync<DateOnly>().ConfigureAwait(false);
            dateCompleted = dateCompleted.Equals(DateOnly.MinValue) ? null : dateCompleted;
            appLogger.LogInformation(Emoji.Known.OkButton + " Set cost to {Cost}", cost.ToString("C2"));
            if (dateCompleted.HasValue)
            {
                appLogger.LogInformation(Emoji.Known.OkButton + " Set DateSubmitted to {DateSubmitted}", dateSubmitted.ToShortDateString());
            }
            else
            {
                appLogger.LogInformation(Emoji.Known.OkButton + " Not completed");
            }

            await vm.AddPrintingProjectAsync(
                        new PrintingProjectDto(
                            cost,
                            dateSubmitted.ToDateTime(TimeOnly.MinValue),
                            dateCompleted?.ToDateTime(TimeOnly.MinValue),
                            customer,
                            model,
                            selectedFilaments))
                    .ConfigureAwait(false);
            appLogger.LogInformation(Emoji.Known.CheckMarkButton + " Added project {Model} ({Customer})", model, customer);
        }
    }
}