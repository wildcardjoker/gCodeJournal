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

            var customerName = await ValidateCustomerName(appLogger).ConfigureAwait(false);
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
                appLogger.LogError(Emoji.Known.CrossMark + "  Error saving data: {ValidationResult}", result);
            }
        }

        private static async Task EditFilamentAsync(IGCodeJournalViewModel vm, ILogger appLogger) => throw new NotImplementedException();

        private static async Task EditFilamentColourAsync(IGCodeJournalViewModel vm, ILogger appLogger) => throw new NotImplementedException();

        private static async Task EditFilamentTypeAsync(IGCodeJournalViewModel vm, ILogger appLogger) => throw new NotImplementedException();

        private static async Task EditManufacturerAsync(IGCodeJournalViewModel vm, ILogger appLogger) => throw new NotImplementedException();

        private static async Task EditModelDesignAsync(IGCodeJournalViewModel vm, ILogger appLogger) => throw new NotImplementedException();

        private static async Task EditPrintingProjectAsync(IGCodeJournalViewModel vm, ILogger appLogger) => throw new NotImplementedException();
    }
}