namespace gcj
{
    #region Using Directives
    using gCodeJournal.ViewModel;
    using Microsoft.Extensions.Logging;
    using Spectre.Console;
    using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;
    #endregion

    public static partial class Program
    {
        private static async Task EditCustomerAsync(IGCodeJournalViewModel vm, ILogger appLogger)
        {
            var customers = await vm.GetAllCustomersAsync().ConfigureAwait(false);
            var customer  = await customers.GetEntitySelectionAsync().ConfigureAwait(false);
            if (customer is null)
            {
                appLogger.LogReturnToMenu();
                return;
            }

            var customerName = await ValidateCustomerNameInputAsync(appLogger).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(customerName))
            {
                return;
            }

            customer.Name = customerName;
            var result = await vm.EditCustomerAsync(customer).ConfigureAwait(false);
            if (result == ValidationResult.Success)
            {
                appLogger.LogInformation(Emoji.Known.CheckMarkButton + "  Updated customer {CustomerName}", customer.Name);
            }
            else
            {
                LogSaveFailure(appLogger, result);
            }
        }

        private static async Task EditFilamentAsync(IGCodeJournalViewModel vm, ILogger appLogger)
        {
            var colours       = await vm.GetAllFilamentColoursAsync().ConfigureAwait(false);
            var filamentTypes = await vm.GetAllFilamentTypesAsync().ConfigureAwait(false);
            var manufacturers = await vm.GetAllManufacturersAsync().ConfigureAwait(false);
            var filaments     = await vm.GetAllFilamentsAsync().ConfigureAwait(false);
            var filament      = await filaments.GetEntitySelectionAsync().ConfigureAwait(false);
            if (filament is null)
            {
                appLogger.LogReturnToMenu();
                return;
            }

            var newManufacturer = await manufacturers.GetEntitySelectionAsync().ConfigureAwait(false);
            var newFilamentType = await filamentTypes.GetEntitySelectionAsync().ConfigureAwait(false);
            var newColour       = await colours.GetEntitySelectionAsync().ConfigureAwait(false);
            if (newColour is null || newFilamentType is null || newManufacturer is null)
            {
                appLogger.LogReturnToMenu();
                return;
            }

            filament!.FilamentColour = newColour;
            filament.FilamentType    = newFilamentType;
            filament.Manufacturer    = newManufacturer;
            filament.CostPerWeight   = await filament.CostPerWeight.GetFilamentCostPerWeightAsync().ConfigureAwait(false);
            filament.ProductId       = await (filament.ProductId   ?? string.Empty).GetFilamentProductIdAsync().ConfigureAwait(false);
            filament.ReorderLink     = await (filament.ReorderLink ?? string.Empty).GetFilamentReorderLinkAsync().ConfigureAwait(false);
            var result = await vm.EditFilamentAsync(filament).ConfigureAwait(false);
            if (result == ValidationResult.Success)
            {
                appLogger.LogInformation(Emoji.Known.CheckMarkButton + "  Updated filament {FilamentDescription}", filament);
            }
            else
            {
                LogSaveFailure(appLogger, result);
            }
        }

        private static async Task EditFilamentColourAsync(IGCodeJournalViewModel vm, ILogger appLogger)
        {
            var colours        = await vm.GetAllFilamentColoursAsync().ConfigureAwait(false);
            var selectedColour = await colours.GetEntitySelectionAsync().ConfigureAwait(false);
            if (selectedColour is null)
            {
                appLogger.LogReturnToMenu();
                return;
            }

            var updatedDescription = await selectedColour.Description.GetFilamentColourAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(updatedDescription))
            {
                "Description cannot be null; no changes made".DisplayWarningMessage();
                appLogger.LogReturnToMenu();
                return;
            }

            if (updatedDescription.Equals(selectedColour.Description))
            {
                "No changes detected".DisplayWarningMessage();
                appLogger.LogReturnToMenu();
                return;
            }

            selectedColour.Description = updatedDescription;
            var result = await vm.EditFilamentColourAsync(selectedColour).ConfigureAwait(false);
            if (result == ValidationResult.Success)
            {
                appLogger.LogInformation(Emoji.Known.CheckMarkButton + "  Updated filament colour {FilamentDescription}", selectedColour);
            }
            else
            {
                LogSaveFailure(appLogger, result);
            }
        }

        private static async Task EditFilamentTypeAsync(IGCodeJournalViewModel vm, ILogger appLogger) => throw new NotImplementedException();

        private static async Task EditManufacturerAsync(IGCodeJournalViewModel vm, ILogger appLogger)
        {
            var manufacturers        = await vm.GetAllManufacturersAsync().ConfigureAwait(false);
            var selectedManufacturer = await manufacturers.GetEntitySelectionAsync().ConfigureAwait(false);
            if (selectedManufacturer is null)
            {
                appLogger.LogReturnToMenu();
                return;
            }

            var updatedManufacturer = await selectedManufacturer.Name.GetManufacturerAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(updatedManufacturer))
            {
                "Manufacturer name cannot be null; no changes made".DisplayWarningMessage();
                appLogger.LogReturnToMenu();
                return;
            }

            if (updatedManufacturer.Equals(selectedManufacturer.Name))
            {
                "No changes detected".DisplayWarningMessage();
                appLogger.LogReturnToMenu();
                return;
            }

            selectedManufacturer.Name = updatedManufacturer;
            var result = await vm.EditManufacturerAsync(selectedManufacturer).ConfigureAwait(false);
            if (result == ValidationResult.Success)
            {
                appLogger.LogInformation(Emoji.Known.CheckMarkButton + "  Updated Manufacturer {Manufacturer}", selectedManufacturer);
            }
            else
            {
                LogSaveFailure(appLogger, result);
            }
        }

        private static async Task EditModelDesignAsync(IGCodeJournalViewModel vm, ILogger appLogger)
        {
            var designs        = await vm.GetAllModelDesignsAsync().ConfigureAwait(false);
            var selectedDesign = await designs.GetEntitySelectionAsync().ConfigureAwait(false);
            if (selectedDesign is null)
            {
                appLogger.LogReturnToMenu();
                return;
            }

            var summary     = await selectedDesign.Summary.GetModelSummary().ConfigureAwait(false);
            var description = await "[Press ENTER to retain existing description]".GetModelDescriptionAsync().ConfigureAwait(false);
            var length      = await selectedDesign.Length.GetModelLengthAsync().ConfigureAwait(false);
            var url         = await "model URL".GetUriAsync(selectedDesign.Url).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(summary))
            {
                appLogger.LogWarning("Summary cannot be empty");
                appLogger.LogReturnToMenu();
                return;
            }

            selectedDesign.Description = description ?? selectedDesign.Description;
            selectedDesign.Summary     = summary;
            selectedDesign.Length      = length;
            selectedDesign.Url         = url;

            var result = await vm.EditModelDesignAsync(selectedDesign).ConfigureAwait(false);
            if (result == ValidationResult.Success)
            {
                appLogger.LogInformation(Emoji.Known.CheckMarkButton + "  Updated Model {ModelDesign}", selectedDesign);
            }
            else
            {
                LogSaveFailure(appLogger, result);
            }
        }

        private static async Task EditPrintingProjectAsync(IGCodeJournalViewModel vm, ILogger appLogger) => throw new NotImplementedException();

        private static void LogSaveFailure(ILogger appLogger, ValidationResult result)
        {
            // TODO: Move to extension method
            appLogger.LogError(Emoji.Known.CrossMark + "  Error saving data: {ValidationResult}", result);
        }
    }
}