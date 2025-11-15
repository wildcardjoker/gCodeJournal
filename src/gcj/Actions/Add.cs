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
            var customerName = await ValidateCustomerNameInputAsync(appLogger).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(customerName))
            {
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

            var manufacturer = await ValidateManufacturerSelectionAsync(appLogger, manufacturers).ConfigureAwait(false);
            if (manufacturer is null)
            {
                return;
            }

            var filamentType = await filamentTypes.GetEntitySelectionAsync().ConfigureAwait(false);
            if (filamentType is null)
            {
                // User chose to go back to the menu
                appLogger.LogReturnToMenu();
                return;
            }

            var colour = await colours.GetEntitySelectionAsync().ConfigureAwait(false);
            if (colour is null)
            {
                // User chose to go back to the menu
                appLogger.LogReturnToMenu();
                return;
            }

            var costPerWeight = await 0m.GetFilamentCostPerWeightAsync().ConfigureAwait(false);
            var productCode   = await string.Empty.GetFilamentProductIdAsync().ConfigureAwait(false);
            var productUrl    = await string.Empty.GetFilamentProductUrlAsync().ConfigureAwait(false);
            var filamentDto   = new FilamentDto(costPerWeight, productCode, productUrl, colour, filamentType, manufacturer);
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
            var filamentType = await string.Empty.GetFilamentTypeAsync().ConfigureAwait(false);
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
            var manufacturerName = await string.Empty.GetManufacturerAsync().ConfigureAwait(false);
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
            var summary     = await string.Empty.GetModelSummary().ConfigureAwait(false);
            var description = await string.Empty.GetModelDescriptionAsync().ConfigureAwait(false);
            var length      = await 0m.GetModelLengthAsync().ConfigureAwait(false);
            var url         = await "model URL".GetUriAsync().ConfigureAwait(false);

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
                appLogger.LogReturnToMenu();
                return;
            }

            appLogger.LogInformation(Emoji.Known.OkButton + " Selected customer: {Customer}", customer);

            var model = await models.GetEntitySelectionAsync().ConfigureAwait(false);
            if (model is null)
            {
                // User chose to go back to the menu
                appLogger.LogReturnToMenu();
                return;
            }

            appLogger.LogInformation(Emoji.Known.OkButton + " Selected model: {Model}", model);

            var selectedFilaments = await filaments.SelectFilamentsAsync(appLogger);
            var cost              = await "cost".GetInputFromConsoleAsync<decimal>().ConfigureAwait(false); // TODO: Calculate from filament and length
            var dateSubmitted = await "submitted".GetDateFromConsoleAsync(DateOnly.FromDateTime(DateTime.Today)).ConfigureAwait(false)
                                ?? DateOnly.FromDateTime(DateTime.Today);
            var dateCompleted = await "completed".GetDateFromConsoleAsync().ConfigureAwait(false);

            appLogger.LogInformation(Emoji.Known.OkButton + " Set cost to {Cost}",                   cost.ToString("C2"));
            appLogger.LogInformation(Emoji.Known.OkButton + " Set DateSubmitted to {DateSubmitted}", dateSubmitted.ToShortDateString());
            if (dateCompleted.HasValue)
            {
                appLogger.LogInformation(Emoji.Known.OkButton + " Set DateCompleted to {DateCompleted}", dateCompleted.Value.ToShortDateString());
            }
            else
            {
                appLogger.LogInformation(Emoji.Known.OkButton + " Not completed");
            }

            await vm.AddPrintingProjectAsync(new PrintingProjectDto(cost, dateSubmitted, dateCompleted, customer, model, selectedFilaments)).ConfigureAwait(false);
            appLogger.LogInformation(
                Emoji.Known.CheckMarkButton + " Added project {Model} for {Customer} with filaments {Filaments}",
                model,
                customer,
                string.Join(", ", selectedFilaments));
        }
    }
}