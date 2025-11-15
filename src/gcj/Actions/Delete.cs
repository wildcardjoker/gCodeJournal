namespace gcj
{
    #region Using Directives
    #region Using Directives
    using gCodeJournal.Model;
    using gCodeJournal.ViewModel;
    using gCodeJournal.ViewModel.DTOs;
    using Microsoft.Extensions.Logging;
    using Spectre.Console;
    using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;
    #endregion
    #endregion

    public static partial class Program
    {
        private static async Task DeleteCustomerAsync(IGCodeJournalViewModel vm, ILogger appLogger)
        {
            var customers = await vm.GetAllCustomersAsync().ConfigureAwait(false);
            var customer  = await customers.GetEntitySelectionAsync().ConfigureAwait(false);
            if (customer is null)
            {
                appLogger.LogReturnToMenu();
                return;
            }

            if (await customer.ConfirmDeleteAsync().ConfigureAwait(false))
            {
                var result = await vm.DeleteCustomerAsync(customer).ConfigureAwait(false);
                if (result == ValidationResult.Success)
                {
                    appLogger.DisplayDeleteConfirmedMessage<Customer>(customer.ToString());
                }
                else
                {
                    appLogger.LogSaveFailure(result);
                }
            }
            else
            {
                DisplayCancelDeleteMessage<CustomerDto>(appLogger);
            }
        }

        private static async Task DeleteFilamentAsync(IGCodeJournalViewModel vm, ILogger appLogger)
        {
            var filaments = await vm.GetAllFilamentsAsync().ConfigureAwait(false);
            var filament  = await filaments.GetEntitySelectionAsync().ConfigureAwait(false);
            if (filament is null)
            {
                appLogger.LogReturnToMenu();
                return;
            }

            if (await filament.ConfirmDeleteAsync().ConfigureAwait(false))
            {
                var result = await vm.DeleteFilamentAsync(filament).ConfigureAwait(false);
                if (result == ValidationResult.Success)
                {
                    appLogger.DisplayDeleteConfirmedMessage<Filament>(filament.ToString());
                }
                else
                {
                    appLogger.LogSaveFailure(result);
                }
            }
            else
            {
                DisplayCancelDeleteMessage<Filament>(appLogger);
            }
        }

        private static async Task DeleteFilamentColourAsync(IGCodeJournalViewModel vm, ILogger appLogger)
        {
            var colours        = await vm.GetAllFilamentColoursAsync().ConfigureAwait(false);
            var selectedColour = await colours.GetEntitySelectionAsync().ConfigureAwait(false);
            if (selectedColour is null)
            {
                appLogger.LogReturnToMenu();
                return;
            }

            if (await selectedColour.ConfirmDeleteAsync().ConfigureAwait(false))
            {
                var result = await vm.DeleteFilamentColourAsync(selectedColour).ConfigureAwait(false);
                if (result == ValidationResult.Success)
                {
                    appLogger.DisplayDeleteConfirmedMessage<FilamentColour>(selectedColour.ToString());
                }
                else
                {
                    appLogger.LogSaveFailure(result);
                }
            }
            else
            {
                DisplayCancelDeleteMessage<FilamentColour>(appLogger);
            }
        }

        private static async Task DeleteFilamentTypeAsync(IGCodeJournalViewModel vm, ILogger appLogger) => throw new NotImplementedException();

        private static async Task DeleteManufacturerAsync(IGCodeJournalViewModel vm, ILogger appLogger)
        {
            var manufacturers        = await vm.GetAllManufacturersAsync().ConfigureAwait(false);
            var selectedManufacturer = await manufacturers.GetEntitySelectionAsync().ConfigureAwait(false);
            if (await selectedManufacturer.ConfirmDeleteAsync().ConfigureAwait(false))
            {
                var result = await vm.DeleteManufacturerAsync(selectedManufacturer).ConfigureAwait(false);
                if (result == ValidationResult.Success)
                {
                    appLogger.DisplayDeleteConfirmedMessage<Manufacturer>(selectedManufacturer.ToString());
                }
                else
                {
                    appLogger.LogSaveFailure(result);
                }
            }
            else
            {
                DisplayCancelDeleteMessage<Manufacturer>(appLogger);
            }
        }

        private static async Task DeleteModelDesignAsync(IGCodeJournalViewModel vm, ILogger appLogger)
        {
            var designs        = await vm.GetAllModelDesignsAsync().ConfigureAwait(false);
            var selectedDesign = await designs.GetEntitySelectionAsync().ConfigureAwait(false);
            if (selectedDesign is null)
            {
                appLogger.LogReturnToMenu();
                return;
            }

            if (await selectedDesign.ConfirmDeleteAsync().ConfigureAwait(false))
            {
                var result = await vm.DeleteModelDesignAsync(selectedDesign).ConfigureAwait(false);
                if (result == ValidationResult.Success)
                {
                    appLogger.DisplayDeleteConfirmedMessage<ModelDesign>(selectedDesign.ToString());
                }
                else
                {
                    appLogger.LogSaveFailure(result);
                }
            }
            else
            {
                DisplayCancelDeleteMessage<ModelDesign>(appLogger);
            }
        }

        private static async Task DeletePrintingProjectAsync(IGCodeJournalViewModel vm, ILogger appLogger)
        {
            var allProjects = await vm.GetAllPrintingProjectsAsync().ConfigureAwait(false);

            var selectedProject = await allProjects.GetEntitySelectionAsync().ConfigureAwait(false);
            if (selectedProject is null)
            {
                appLogger.LogReturnToMenu();
                return;
            }

            if (await selectedProject.ConfirmDeleteAsync().ConfigureAwait(false))
            {
                var result = await vm.DeletePrintingProjectAsync(selectedProject).ConfigureAwait(false);
                if (result == ValidationResult.Success)
                {
                    appLogger.DisplayDeleteConfirmedMessage<PrintingProject>(selectedProject.ToString());
                }
                else
                {
                    appLogger.LogSaveFailure(result);
                }
            }
            else
            {
                appLogger.DisplayCancelDeleteMessage<PrintingProject>();
            }
        }

        private static void DisplayCancelDeleteMessage<T>(this ILogger appLogger) where T : class
        {
            appLogger.LogInformation(Emoji.Known.BackArrow + "  Deletion of {ObjectType} cancelled by user", typeof(T).Name.HumanizeDtoName());
        }

        private static void DisplayDeleteConfirmedMessage<T>(this ILogger appLogger, string description) where T : class
        {
            appLogger.LogInformation(Emoji.Known.Collision + "  Updated {ObjectType} {ObjectValue}", typeof(T).Name.HumanizeDtoName(), description);
        }
    }
}