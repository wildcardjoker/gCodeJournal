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
    public async Task AddFilamentAsync(Filament filament)
    {
        ArgumentNullException.ThrowIfNull(filament);
        await _db.Filaments.AddAsync(filament).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    ///     Retrieves a list of customers from the database, ordered by their names.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a list of
    ///     <see cref="T:gCodeJournal.Model.Customer">Customer</see> entities, ordered by their names.
    /// </returns>
    /// <remarks>
    ///     This method queries the <see cref="P:gCodeJournal.Model.GCodeJournalDbContext.Customers">Customers</see> DbSet
    ///     and orders the results by the <see cref="P:gCodeJournal.Model.Customer.Name">Name</see> property.
    /// </remarks>
    public Task<List<Customer>> GetAllCustomersAsync()
    {
        return _db.Customers.OrderBy(c => c.Name).ToListAsync();
    }

    /// <summary>
    ///     Retrieves a list of filaments from the database, ordered by manufacturer and color.
    /// </summary>
    /// <returns>
    ///     A task representing the asynchronous operation. The task result contains a list of <see cref="Filament" />
    ///     entities.
    /// </returns>
    public Task<List<Filament>> GetAllFilamentsAsync()
    {
        return _db.Filaments.OrderBy(f => f.Manufacturer).ThenBy(c => c.Colour).ToListAsync();
    }

    /// <summary>
    ///     Retrieves a list of manufacturers from the database, ordered by name.
    /// </summary>
    /// <returns>
    ///     A task representing the asynchronous operation. The task result contains a list of <see cref="Manufacturer" />
    ///     entities.
    /// </returns>
    public Task<List<Manufacturer>> GetAllManufacturersAsync()
    {
        return _db.Manufacturers.OrderBy(m => m.Name).ToListAsync();
    }
    #endregion

    /// <summary>
    ///     Asynchronously adds a new customer to the database.
    /// </summary>
    /// <param name="customer">
    ///     The <see cref="T:gCodeJournal.Model.Customer">Customer</see> instance to add.
    ///     This parameter must not be <see langword="null" />.
    /// </param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when the <paramref name="customer" /> parameter is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     This method adds the specified customer to the <see cref="P:gCodeJournal.Model.GCodeJournalDbContext.Customers" />
    ///     collection and persists the changes to the database.
    /// </remarks>
    /// <returns>
    ///     A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.
    /// </returns>
    public async Task AddCustomerAsync(Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);
        await _db.Customers.AddAsync(customer).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    ///     Adds a new filament colour to the database.
    /// </summary>
    /// <param name="filamentColour">The filament colour entity to be added.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if the <paramref name="filamentColour" /> is <c>null</c>.
    /// </exception>
    /// <remarks>
    ///     This method ensures that the provided <see cref="FilamentColour" /> entity is added to the
    ///     <see cref="GCodeJournalDbContext.FilamentColours" /> collection and persists the changes to the database.
    /// </remarks>
    public async Task AddFilamentColourAsync(FilamentColour filamentColour)
    {
        ArgumentNullException.ThrowIfNull(filamentColour);
        await _db.FilamentColours.AddAsync(filamentColour).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    ///     Adds a new filament type to the database.
    /// </summary>
    /// <param name="filamentType">The filament type entity to be added.</param>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="filamentType" /> is null.</exception>
    /// <remarks>
    ///     This method ensures that the provided <see cref="T:gCodeJournal.Model.FilamentType" />
    ///     is added to the <see cref="P:gCodeJournal.Model.GCodeJournalDbContext.FilamentTypes" /> collection
    ///     and persists the changes to the database.
    /// </remarks>
    public async Task AddFilamentTypeAsync(FilamentType filamentType)
    {
        ArgumentNullException.ThrowIfNull(filamentType);
        await _db.FilamentTypes.AddAsync(filamentType).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    ///     Adds a new <see cref="T:gCodeJournal.Model.ModelDesign">ModelDesign</see> entity to the database.
    /// </summary>
    /// <param name="filamentColour">
    ///     The <see cref="T:gCodeJournal.Model.ModelDesign">ModelDesign</see> instance to be added.
    /// </param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when the <paramref name="filamentColour" /> parameter is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     This method asynchronously adds the provided <see cref="T:gCodeJournal.Model.ModelDesign">ModelDesign</see>
    ///     to the <see cref="P:gCodeJournal.Model.GCodeJournalDbContext.ModelDesigns">ModelDesigns</see> DbSet
    ///     and commits the changes to the database.
    /// </remarks>
    /// <returns>
    ///     A <see cref="T:System.Threading.Tasks.Task">Task</see> that represents the asynchronous operation.
    /// </returns>
    public async Task AddModelDesignAsync(ModelDesign filamentColour)
    {
        ArgumentNullException.ThrowIfNull(filamentColour);
        await _db.ModelDesigns.AddAsync(filamentColour).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    ///     Asynchronously adds a new printing project to the database.
    /// </summary>
    /// <param name="project">
    ///     The <see cref="T:gCodeJournal.Model.PrintingProject" /> instance representing the printing project to be added.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when the <paramref name="project" /> parameter is <c>null</c>.
    /// </exception>
    /// <remarks>
    ///     This method adds the specified <see cref="T:gCodeJournal.Model.PrintingProject" /> to the database context and
    ///     saves the changes.
    /// </remarks>
    /// <returns>
    ///     A <see cref="T:System.Threading.Tasks.Task" /> representing the asynchronous operation.
    /// </returns>
    public async Task AddPrintingProjectAsync(PrintingProject project)
    {
        ArgumentNullException.ThrowIfNull(project);
        await _db.PrintingProjects.AddAsync(project).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    ///     Retrieves a list of filament colours from the database, ordered by their description.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a list of
    ///     <see cref="T:gCodeJournal.Model.FilamentColour">FilamentColour</see> objects, ordered by their description.
    /// </returns>
    /// <remarks>
    ///     This method queries the
    ///     <see cref="P:gCodeJournal.Model.GCodeJournalDbContext.FilamentColours">FilamentColours</see>
    ///     DbSet and returns the results as a list. The results are sorted alphabetically by the
    ///     <see cref="P:gCodeJournal.Model.FilamentColour.Description">Description</see>.
    /// </remarks>
    public Task<List<FilamentColour>> GetFilamentColoursAsync()
    {
        return _db.FilamentColours.OrderBy(fc => fc.Description).ToListAsync();
    }

    /// <summary>
    ///     Retrieves a list of all available filament types from the database, ordered by their description.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a list of
    ///     <see cref="T:gCodeJournal.Model.FilamentType">FilamentType</see> objects.
    /// </returns>
    /// <remarks>
    ///     This method queries the <see cref="P:gCodeJournal.Model.GCodeJournalDbContext.FilamentTypes">FilamentTypes</see>
    ///     DbSet and orders the results by the <see cref="P:gCodeJournal.Model.FilamentType.Description">Description</see>
    ///     property.
    /// </remarks>
    public Task<List<FilamentType>> GetFilamentTypesAsync()
    {
        return _db.FilamentTypes.OrderBy(ft => ft.Description).ToListAsync();
    }

    /// <summary>
    ///     Retrieves a list of <see cref="T:gCodeJournal.Model.ModelDesign">ModelDesign</see> entities
    ///     ordered by their description.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a list of
    ///     <see cref="T:gCodeJournal.Model.ModelDesign">ModelDesign</see> objects.
    /// </returns>
    /// <remarks>
    ///     This method queries the <see cref="P:gCodeJournal.Model.GCodeJournalDbContext.ModelDesigns">ModelDesigns</see>
    ///     DbSet, orders the results by the <see cref="P:gCodeJournal.Model.ModelDesign.Description">Description</see>
    ///     property, and returns the results as a list.
    /// </remarks>
    public Task<List<ModelDesign>> GetModelDesignsAsync()
    {
        return _db.ModelDesigns.OrderBy(md => md.Description).ToListAsync();
    }

    /// <summary>
    ///     Retrieves a list of <see cref="PrintingProject" /> entities from the database.
    /// </summary>
    /// <remarks>
    ///     The resulting list is ordered by the description of the associated <see cref="ModelDesign" />,
    ///     followed by the <see cref="Customer" /> who requested the project, and then by the associated
    ///     <see cref="Filament" /> entities in ascending order.
    /// </remarks>
    /// <returns>
    ///     A <see cref="Task{TResult}" /> representing the asynchronous operation. The task result contains
    ///     a list of <see cref="PrintingProject" /> entities.
    /// </returns>
    public Task<List<PrintingProject>> GetPrintingProjectsAsync()
    {
        return _db.PrintingProjects.OrderBy(pp => pp.Model.Description).ThenBy(c => c.Customer).ThenBy(f => f.Filaments.OrderBy(f1 => f1.ToString())).ToListAsync();
    }
}