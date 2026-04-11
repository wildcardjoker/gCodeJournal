// gcj

namespace gcj;

#region Using Directives
using System.Diagnostics;
using gCodeJournal.ViewModel;
using gCodeJournal.ViewModel.Import;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console;
#endregion

public static partial class Program
{
    #region Constants
    const string MainMenuLevel         = "Main Menu";
    const string MenuCustomers         = "Customers";
    const string MenuExit              = "Exit";
    const string MenuFilamentColours   = "Filament Colours";
    const string MenuFilaments         = "Filaments";
    const string MenuFilamentTypes     = "Filament Types";
    const string MenuImportData        = "Import data";
    const string MenuManufacturers     = "Manufacturers";
    const string MenuModelDesigns      = "Model Designs";
    const string MenuPrinters          = "Printers";
    const string MenuPrintingProjects  = "Printing Projects";
    const string SubMenuAddNew         = "Add New";
    const string SubMenuBackToMain     = "Back to Main Menu";
    const string SubMenuDeleteExisting = "Delete Existing";
    const string SubMenuListAll        = "List All";
    const string SubMenuUpdateExisting = "Update Existing";
    #endregion

    #region Fields
    static readonly string[] MainMenu =
    [
        MenuCustomers,
        MenuManufacturers,
        MenuFilamentColours,
        MenuFilaments,
        MenuFilamentTypes,
        MenuModelDesigns,
        MenuPrinters,
        MenuPrintingProjects,
        MenuImportData,
        MenuExit
    ];

    static readonly string[] SubMenu =
    [
        SubMenuListAll,
        SubMenuAddNew,
        SubMenuUpdateExisting,
        SubMenuDeleteExisting,
        SubMenuBackToMain
    ];
    #endregion

    static async Task DisplayMenuAsync(ServiceProvider provider, ILogger appLogger)
    {
        var subMenuSelection = await GetMenuSelectionAsync(MainMenuLevel, MainMenu).ConfigureAwait(false);
        while (!subMenuSelection.Equals(MenuExit))
        {
            if (subMenuSelection.Equals(MenuImportData))
            {
                await ImportData(provider, appLogger).ConfigureAwait(false);
            }
            else
            {
                while (!subMenuSelection.Equals(SubMenuBackToMain))
                {
                    subMenuSelection = await GetSubMenuSelectionAsync(subMenuSelection, provider, appLogger).ConfigureAwait(false);
                }
            }

            subMenuSelection = await GetMenuSelectionAsync(MainMenuLevel, MainMenu).ConfigureAwait(false);
        }
    }

    static async Task<string> GetMenuSelectionAsync(string menuLevel, string[] choices)
    {
        var prompt   = new SelectionPrompt<string>().Title($"{menuLevel}: Please choose from the following options").PageSize(10).AddChoices(choices);
        var response = await AnsiConsole.PromptAsync(prompt).ConfigureAwait(false);

        return response;
    }

    static string[] GetMenuWithSection(string menuLevel, string[] choices) =>
        choices
            .Select(choice =>
                        choice == SubMenuBackToMain
                            ? choice
                            : $"{choice} {(choice.StartsWith("List all", StringComparison.OrdinalIgnoreCase) ? menuLevel : menuLevel.Singularize())}")
            .ToArray();

    static async Task<string> GetSubMenuSelectionAsync(string section, ServiceProvider provider, ILogger appLogger)
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

    static async Task ImportData(ServiceProvider provider, ILogger appLogger)
    {
        using var scope = provider.CreateScope();
        var       vm    = scope.ServiceProvider.GetRequiredService<IGCodeJournalViewModel>();

        var importPath = vm.GetLastImportPath();
        var path =
            await AnsiConsole
                  .PromptAsync(
                      new TextPrompt<string?>("Please enter the path to the CSV file(s) (ENTER to cancel import):").AllowEmpty().DefaultValue(importPath))
                  .ConfigureAwait(false);

        if (!path.IsPathValid(appLogger))
        {
            appLogger.LogReturnToMenu();

            return;
        }

        vm.SetImportPath(path!);
        appLogger.LogInformation("Import: starting CSV import for path '{Path}'.", path);
        List<CsvImporter.ImportFileResult>? fileResults = null;
        var                                 stopwatch   = Stopwatch.StartNew();
        try
        {
            fileResults = await vm.ImportFromCsvAsync(path!, appLogger).ConfigureAwait(false);
            stopwatch.Stop();

            appLogger.LogInformation("Import: completed successfully for path '{Path}' in {ElapsedMilliseconds} ms.", path, stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            appLogger.LogInformation("Import: cancelled by user for path '{Path}' after {ElapsedMilliseconds} ms.", path, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            appLogger.LogError(ex, "Import: failed for path '{Path}' after {ElapsedMilliseconds} ms.", path, stopwatch.ElapsedMilliseconds);
        }
        finally
        {
            if (fileResults is not null)
            {
                var totalCreated = 0;
                var totalUpdated = 0;
                var totalSkipped = 0;
                var totalFailed  = 0;
                foreach (var fr in fileResults)
                {
                    var r = fr.Result;
                    totalCreated += r.Created;
                    totalUpdated += r.Updated;
                    totalSkipped += r.Skipped;
                    totalFailed  += r.Failed;

                    appLogger.LogInformation(
                        "File: {File} - {Created} created, {Updated} updated, {Skipped} skipped, {Failed} failed",
                        fr.FileName,
                        r.Created,
                        r.Updated,
                        r.Skipped,
                        r.Failed);

                    if (r.Errors.Any())
                    {
                        appLogger.LogError(Emoji.Known.Warning + "  Errors for {File}:", fr.FileName);
                        foreach (var err in r.Errors)
                        {
                            appLogger.LogError(Emoji.Known.Warning + "  {Error}", err);
                        }
                    }
                }

                appLogger.LogInformation(
                    "Import totals -- Created: {Created}, Updated: {Updated}, Skipped: {Skipped}, Failed: {Failed}",
                    totalCreated,
                    totalUpdated,
                    totalSkipped,
                    totalFailed);
            }
        }
    }
}