namespace gcj
{
    #region Using Directives
    using gCodeJournal.ViewModel.DTOs;
    using Microsoft.Extensions.Logging;
    using Spectre.Console;
    #endregion

    public static partial class Program
    {
        private static async Task<string?> ValidateCustomerNameInputAsync(ILogger appLogger)
        {
            var customerName = await "customer's name".GetInputFromConsoleAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(customerName))
            {
                appLogger.LogError(Emoji.Known.Warning + "  Customer name cannot be empty");
            }

            return customerName;
        }

        private static async Task<ManufacturerDto?> ValidateManufacturerSelectionAsync(ILogger appLogger, List<ManufacturerDto> manufacturers)
        {
            var manufacturer = await manufacturers.GetEntitySelectionAsync().ConfigureAwait(false);
            if (manufacturer is not null)
            {
                return manufacturer;
            }

            // User chose to go back to the menu
            appLogger.LogReturnToMenu();
            return null;
        }
    }
}