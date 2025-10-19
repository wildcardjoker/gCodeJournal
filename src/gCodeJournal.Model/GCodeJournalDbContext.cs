// gCodeJournal.Model

namespace gCodeJournal.Model;

#region Using Directives
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
#endregion

/// <inheritdoc />
/// <summary>
///     Represents the application's database context, providing access to the database entities
///     and enabling interaction with the underlying database.
/// </summary>
public class GCodeJournalDbContext : DbContext
{
    #region Constructors
    /// <summary>
    ///     Initializes a new instance of the <see cref="GCodeJournalDbContext" /> class.
    /// </summary>
    /// <remarks>
    ///     This parameterless constructor is provided for design-time tooling and for scenarios
    ///     where the context is created without externally supplied <see cref="DbContextOptions{TContext}" />.
    ///     Configuration fallback (for example, the SQLite file used for design-time operations)
    ///     is applied in <see cref="OnConfiguring(DbContextOptionsBuilder)" />.
    /// </remarks>
    public GCodeJournalDbContext() {}

    /// <summary>
    ///     Initializes a new instance of the <see cref="GCodeJournalDbContext" /> class using the specified options.
    /// </summary>
    /// <param name="options">
    ///     The options to be used by this context. This typically includes the database provider,
    ///     connection string and other provider-specific configuration. The options instance is
    ///     passed to the base <see cref="DbContext" /> constructor.
    /// </param>
    public GCodeJournalDbContext(DbContextOptions<GCodeJournalDbContext> options) : base(options) {}
    #endregion

    #region Properties
    /// <summary>
    ///     Gets or sets the collection of <see cref="Customer" /> entities.
    /// </summary>
    /// <remarks>
    ///     This <see cref="DbSet{Customer}" /> is used by Entity Framework Core to query and persist
    ///     customer records. Each entry represents a customer who can request printing projects.
    /// </remarks>
    public DbSet<Customer> Customers {get; set;} = null!;

    /// <summary>
    ///     Gets or sets the collection of <see cref="FilamentColour" /> lookup entities.
    /// </summary>
    /// <remarks>
    ///     Contains the set of known filament colours used by <see cref="Filament" /> and <see cref="FilamentType" />.
    ///     Treated as a reference/lookup table within the database model.
    /// </remarks>
    public DbSet<FilamentColour> FilamentColours {get; set;} = null!;

    /// <summary>
    ///     Gets or sets the collection of <see cref="Filament" /> entities.
    /// </summary>
    /// <remarks>
    ///     Represents the filaments (spools or purchases) available to the application.
    ///     The <see cref="DbSet{Filament}" /> allows querying, adding, updating and removing filament records.
    /// </remarks>
    public DbSet<Filament> Filaments {get; set;} = null!;

    /// <summary>
    ///     Gets or sets the collection of <see cref="FilamentType" /> lookup entities.
    /// </summary>
    /// <remarks>
    ///     Contains material/type definitions (for example, PLA, ABS) used by <see cref="Filament" />.
    ///     This <see cref="DbSet{FilamentType}" /> is used for queries and persistence of filament type metadata.
    /// </remarks>
    public DbSet<FilamentType> FilamentTypes {get; set;} = null!;

    /// <summary>
    ///     Gets or sets the collection of <see cref="Manufacturer" /> entities.
    /// </summary>
    /// <remarks>
    ///     Represents vendors or brands that produce filaments. Each manufacturer may be associated
    ///     with zero or more <see cref="Filament" /> entries.
    /// </remarks>
    public DbSet<Manufacturer> Manufacturers {get; set;} = null!;

    /// <summary>
    ///     Gets or sets the collection of <see cref="ModelDesign" /> entities.
    /// </summary>
    /// <remarks>
    ///     Stores design metadata describing printable models. Used when associating a design with a
    ///     <see cref="PrintingProject" />.
    /// </remarks>
    public DbSet<ModelDesign> ModelDesigns {get; set;} = null!;

    /// <summary>
    ///     Gets or sets the collection of <see cref="PrintingProject" /> entities.
    /// </summary>
    /// <remarks>
    ///     Represents individual print jobs requested by customers. This <see cref="DbSet{PrintingProject}" />
    ///     is used to track submission/completion timestamps, cost and associated filaments and customer.
    /// </remarks>
    public DbSet<PrintingProject> PrintingProjects {get; set;} = null!;
    #endregion

    internal static string GetDefaultDbPath()
    {
        var appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "gCodeJournal");
        Directory.CreateDirectory(appFolder);
        return Path.Combine(appFolder, "gCodeJournal.db");
    }

    /// <summary>
    ///     Configures the database (and other options) to be used for this context.
    /// </summary>
    /// <param name="optionsBuilder">
    ///     The builder used to configure the context options. Implementations should check
    ///     <see cref="DbContextOptionsBuilder.IsConfigured" /> to determine whether configuration
    ///     has already been supplied by external code (for example, dependency injection).
    /// </param>
    /// <remarks>
    ///     When no configuration has been provided (for example, during design-time operations
    ///     when running EF Core tools from a class library), this method applies a sensible
    ///     fallback configuration: a SQLite database file named <c>gcodejournal.db</c>.
    ///     Finally, the base implementation is invoked to allow any further configuration by EF.
    /// </remarks>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Design-time fallback so migrations / tooling can run from this class library
            var dbPath = GetDefaultDbPath();
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        base.OnConfiguring(optionsBuilder);
    }
}

// ReSharper disable once UnusedType.Global
/// <inheritdoc />
/// <summary>
///     Provides a factory for creating instances of
///     <see cref="T:gCodeJournal.Model.GCodeJournalDbContext">GCodeJournalDbContext</see> at design time.
/// </summary>
/// <remarks>
///     This class is used by Entity Framework tools to create a
///     <see cref="T:gCodeJournal.Model.GCodeJournalDbContext">GCodeJournalDbContext</see> instance
///     for design-time operations, such as migrations. It configures the context to use a SQLite database
///     with a default file path.
/// </remarks>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<GCodeJournalDbContext>
{
    #region IDesignTimeDbContextFactory<GCodeJournalDbContext> Members
    /// <summary>
    ///     Creates a new instance of <see cref="T:gCodeJournal.Model.GCodeJournalDbContext">GCodeJournalDbContext</see>.
    /// </summary>
    /// <param name="args">
    ///     An array of command-line arguments. This parameter is not used in the current implementation
    ///     but is required by the <see cref="T:Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory{TContext}" />
    ///     interface.
    /// </param>
    /// <returns>
    ///     A new instance of <see cref="T:gCodeJournal.Model.GCodeJournalDbContext">GCodeJournalDbContext</see>
    ///     configured to use a SQLite database with a default file path.
    /// </returns>
    /// <remarks>
    ///     This method is invoked by Entity Framework tools during design-time operations, such as migrations.
    ///     It ensures that the database context is properly configured for these operations.
    /// </remarks>
    public GCodeJournalDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GCodeJournalDbContext>();
        optionsBuilder.UseSqlite($"Data Source={GCodeJournalDbContext.GetDefaultDbPath()}");

        return new GCodeJournalDbContext(optionsBuilder.Options);
    }
    #endregion
}