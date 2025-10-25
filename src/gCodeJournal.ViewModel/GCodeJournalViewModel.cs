namespace gCodeJournal.ViewModel;

#region Using Directives
using Microsoft.EntityFrameworkCore;
using Model;
#endregion

/// <inheritdoc />
public class GCodeJournalViewModel : IGCodeJournalViewModel
{
    #region Fields
    private readonly GCodeJournalDbContext _db;
    #endregion

    #region Constructors
    /// <summary>
    ///     Initializes a new instance of the
    ///     <see cref="T:gCodeJournal.ViewModel.GCodeJournalViewModel">GCodeJournalViewModel</see> class using the specified
    ///     database context
    ///     options.
    /// </summary>
    /// <param name="db">The database context to be used by the ViewModel.</param>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown when the <paramref name="db" /> parameter is <see langword="null" />.
    /// </exception>
    public GCodeJournalViewModel(GCodeJournalDbContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));
    #endregion

    #region IGCodeJournalViewModel Members
    /// <inheritdoc />
    public async Task AddCustomerAsync(Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);
        await _db.Customers.AddAsync(customer).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task AddFilamentAsync(Filament filament)
    {
        ArgumentNullException.ThrowIfNull(filament);
        await _db.Filaments.AddAsync(filament).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task AddFilamentColourAsync(FilamentColour filamentColour)
    {
        ArgumentNullException.ThrowIfNull(filamentColour);
        await _db.FilamentColours.AddAsync(filamentColour).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task AddFilamentTypeAsync(FilamentType filamentType)
    {
        ArgumentNullException.ThrowIfNull(filamentType);
        await _db.FilamentTypes.AddAsync(filamentType).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task AddModelDesignAsync(ModelDesign modelDesign)
    {
        ArgumentNullException.ThrowIfNull(modelDesign);
        await _db.ModelDesigns.AddAsync(modelDesign).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task AddPrintingProjectAsync(PrintingProject project)
    {
        ArgumentNullException.ThrowIfNull(project);
        await _db.PrintingProjects.AddAsync(project).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<List<Customer>> GetAllCustomersAsync() => _db.Customers.OrderBy(c => c.Name).ToListAsync();

    /// <inheritdoc />
    public Task<List<FilamentColour>> GetAllFilamentColoursAsync() => _db.FilamentColours.OrderBy(fc => fc.Description).ToListAsync();

    /// <inheritdoc />
    public Task<List<Filament>> GetAllFilamentsAsync() => _db.Filaments.OrderBy(f => f.Manufacturer).ThenBy(c => c.Colour).ToListAsync();

    /// <inheritdoc />
    public Task<List<FilamentType>> GetAllFilamentTypesAsync() => _db.FilamentTypes.OrderBy(ft => ft.Description).ToListAsync();

    /// <inheritdoc />
    public Task<List<Manufacturer>> GetAllManufacturersAsync() => _db.Manufacturers.OrderBy(m => m.Name).ToListAsync();

    /// <inheritdoc />
    public Task<List<ModelDesign>> GetAllModelDesignsAsync() => _db.ModelDesigns.OrderBy(md => md.Description).ToListAsync();

    /// <inheritdoc />
    public Task<List<PrintingProject>> GetAllPrintingProjectsAsync() => _db.PrintingProjects.OrderBy(pp => pp.Model.Description)
                                                                           .ThenBy(c => c.Customer)
                                                                           .ThenBy(f => f.Filaments.OrderBy(f1 => f1))
                                                                           .ToListAsync();
    #endregion
}