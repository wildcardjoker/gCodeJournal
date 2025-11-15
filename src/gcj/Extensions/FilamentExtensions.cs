namespace gcj
{
    #region Using Directives
    using gCodeJournal.ViewModel.DTOs;
    using Microsoft.Extensions.Logging;
    using Spectre.Console;
    #endregion

    public static partial class Program
    {
        private static async Task<List<FilamentDto>> SelectFilamentsAsync(
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
    }
}