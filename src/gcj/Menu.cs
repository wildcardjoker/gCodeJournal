namespace gcj
{
    #region Using Directives
    using Humanizer;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Spectre.Console;
    #endregion

    public static partial class Program
    {
        #region Constants
        private const string MainMenuLevel         = "Main Menu";
        private const string MenuCustomers         = "Customers";
        private const string MenuExit              = "Exit";
        private const string MenuFilamentColours   = "Filament Colours";
        private const string MenuFilaments         = "Filaments";
        private const string MenuFilamentTypes     = "Filament Types";
        private const string MenuManufacturers     = "Manufacturers";
        private const string MenuModelDesigns      = "Model Designs";
        private const string MenuPrintingProjects  = "Printing Projects";
        private const string SubMenuAddNew         = "Add New";
        private const string SubMenuBackToMain     = "Back to Main Menu";
        private const string SubMenuDeleteExisting = "Delete Existing";
        private const string SubMenuListAll        = "List All";
        private const string SubMenuUpdateExisting = "Update Existing";
        #endregion

        #region Fields
        private static readonly string[] MainMenu =
        [
            MenuCustomers,
            MenuManufacturers,
            MenuFilamentColours,
            MenuFilaments,
            MenuFilamentTypes,
            MenuModelDesigns,
            MenuPrintingProjects,
            MenuExit
        ];

        private static readonly string[] SubMenu =
        [
            SubMenuListAll,
            SubMenuAddNew,
            SubMenuUpdateExisting,
            SubMenuDeleteExisting,
            SubMenuBackToMain
        ];
        #endregion

        private static async Task DisplayMenuAsync(ServiceProvider provider, ILogger appLogger)
        {
            var subMenuSelection = await GetMenuSelectionAsync(MainMenuLevel, MainMenu).ConfigureAwait(false);
            while (!subMenuSelection.Equals(MenuExit))
            {
                while (!subMenuSelection.Equals(SubMenuBackToMain))
                {
                    subMenuSelection = await GetSubMenuSelectionAsync(subMenuSelection, provider, appLogger).ConfigureAwait(false);
                }

                subMenuSelection = await GetMenuSelectionAsync(MainMenuLevel, MainMenu).ConfigureAwait(false);
            }
        }

        private static async Task<string> GetMenuSelectionAsync(string menuLevel, string[] choices)
        {
            var prompt   = new SelectionPrompt<string>().Title($"{menuLevel}: Please choose from the following options").PageSize(10).AddChoices(choices);
            var response = await AnsiConsole.PromptAsync(prompt).ConfigureAwait(false);
            return response;
        }

        private static string[] GetMenuWithSection(string menuLevel, string[] choices)
        {
            return choices.Select(choice => choice == SubMenuBackToMain
                                                ? choice
                                                : $"{choice} {(choice.StartsWith("List all", StringComparison.OrdinalIgnoreCase) ? menuLevel : menuLevel.Singularize())}")
                          .ToArray();
        }

        private static async Task<string> GetSubMenuSelectionAsync(string section, ServiceProvider provider, ILogger appLogger)
        {
            var response = await GetMenuSelectionAsync(section, GetMenuWithSection(section, SubMenu)).ConfigureAwait(false);
            while (!response.Equals(SubMenuBackToMain))
            {
                if (!response.Equals(SubMenuBackToMain))
                {
                    await ProcessDatabaseActionAsync(response, provider, appLogger).ConfigureAwait(false);
                }

                response = await GetMenuSelectionAsync(section, GetMenuWithSection(section, SubMenu)).ConfigureAwait(false);
            }

            return response;
        }
    }
}