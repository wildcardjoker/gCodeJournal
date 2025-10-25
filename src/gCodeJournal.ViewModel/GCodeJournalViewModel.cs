namespace gCodeJournal.ViewModel;

#region Using Directives
using Microsoft.EntityFrameworkCore;
using Model;
#endregion

/// <inheritdoc />
/// <summary>
///     Represents the ViewModel for the GCodeJournal application. This class extends the
///     <see cref="T:gCodeJournal.Model.GCodeJournalDbContext">GCodeJournalDbContext</see>
///     to provide additional functionality for managing and querying database entities related to the application.
/// </summary>
public class GCodeJournalViewModel : GCodeJournalDbContext
{
    #region Constructors
    /// <inheritdoc />
    /// <summary>
    ///     Initializes a new instance of the
    ///     <see cref="T:gCodeJournal.ViewModel.GCodeJournalViewModel">GCodeJournalViewModel</see> class.
    /// </summary>
    public GCodeJournalViewModel() {}

    /// <inheritdoc />
    /// <summary>
    ///     Initializes a new instance of the
    ///     <see cref="T:gCodeJournal.ViewModel.GCodeJournalViewModel">GCodeJournalViewModel</see> class using the specified
    ///     database context
    ///     options.
    /// </summary>
    /// <param name="options">The options to configure the database context.</param>
    public GCodeJournalViewModel(DbContextOptions<GCodeJournalDbContext> options) : base(options) {}
    #endregion

    /// <summary>
    ///     Adds a new filament to the database.
    /// </summary>
    /// <param name="filament">The filament entity to be added.</param>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="filament" /> is null.</exception>
    public async Task AddFilamentAsync(Filament filament)
    {
        ArgumentNullException.ThrowIfNull(filament);
        await Filaments.AddAsync(filament).ConfigureAwait(false);
        await SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    ///     Retrieves a list of filaments from the database, ordered by manufacturer and color.
    /// </summary>
    /// <returns>
    ///     A task representing the asynchronous operation. The task result contains a list of <see cref="Filament" />
    ///     entities.
    /// </returns>
    public Task<List<Filament>> GetFilamentsAsync()
    {
        return Filaments.OrderBy(f => f.Manufacturer).ThenBy(c => c.Colour).ToListAsync();
    }

    /// <summary>
    ///     Retrieves a list of manufacturers from the database, ordered by name.
    /// </summary>
    /// <returns>
    ///     A task representing the asynchronous operation. The task result contains a list of <see cref="Manufacturer" />
    ///     entities.
    /// </returns>
    public Task<List<Manufacturer>> GetManufacturersAsync()
    {
        return Manufacturers.OrderBy(m => m.Name).ToListAsync();
    }
}