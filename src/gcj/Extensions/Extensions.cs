// gcj

namespace gcj;

#region Using Directives
using gCodeJournal.ViewModel.DTOs;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;
#endregion

public static partial class Program
{
    static bool IsPathValid(this string? path, ILogger appLogger)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            appLogger.LogError($"{Emoji.Known.Warning}  Path is null or empty");

            return false;
        }

        try
        {
            var fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath))
            {
                return true;
            }

            if (Directory.Exists(fullPath))
            {
                return true;
            }

            appLogger.LogError(Emoji.Known.Warning + "  Failed to find file or directory {Path}", fullPath);

            return false;
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            appLogger.LogError(Emoji.Known.CrossMark + "  The provided path is invalid: {Path}. Error: {ErrorMessage}", path, ex.Message);

            return false;
        }
    }

    static void LogSaveFailure(this ILogger appLogger, ValidationResult result) => appLogger.LogError(
        Emoji.Known.CrossMark + "  Error saving data: {ValidationResult}",
        result);

    static async Task<List<FilamentDto>> SelectFilamentsAsync(
        this List<FilamentDto>    filaments,
        ILogger                   appLogger,
        IEnumerable<FilamentDto>? currentFilaments = null)
    {
        var selectedFilaments = currentFilaments?.ToList() ?? [];
        while (true)
        {
            var selected = await filaments.GetEntitySelectionAsync().ConfigureAwait(false);
            if (selected is null)
            {
                if (!selectedFilaments.Any())
                {
                    // User chose to return immediately -> cancel project creation.
                    return selectedFilaments;
                }

                // User has finished selecting filaments; exit selection loop and continue.
                break;
            }

            selectedFilaments.Add(selected);
            appLogger.LogInformation(Emoji.Known.OkButton + " Added filament {Filament}", selected);

            // Loop will prompt again to allow multiple selections until the user chooses to return.
        }

        return selectedFilaments;
    }

    static async Task<PrinterDto?> SelectPrinterAsync(this List<PrinterDto> printers, ILogger appLogger, PrinterDto? currentPrinter = null)
    {
        //var selectedPrinter = currentPrinter;
        var selected = await printers.GetEntitySelectionAsync().ConfigureAwait(false);
        if (selected is null)
        {
            // User chose to return immediately -> cancel project creation.
            return currentPrinter;
        }

        appLogger.LogInformation(Emoji.Known.OkButton + " Selected printer {Printer}", selected);

        return selected;
    }
}