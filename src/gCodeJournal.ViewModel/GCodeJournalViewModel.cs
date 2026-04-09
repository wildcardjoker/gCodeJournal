// gCodeJournal.ViewModel

namespace gCodeJournal.ViewModel;

#region Using Directives
using System.ComponentModel.DataAnnotations;
using DTOs;
using Import;
using Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Model;
#endregion

/// <inheritdoc />
public class GCodeJournalViewModel : IGCodeJournalViewModel
{
    #region Constants
    const string GCodeJournalRegistryPath = @"HKEY_CURRENT_USER\SOFTWARE\WildCardJoker\gCodeJournal";
    const string ImportPathRegistryKey    = "ImportPath";
    #endregion

    #region Fields
    readonly GCodeJournalDbContext _db;
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
    // keep some legacy add methods on the ViewModel — they remain entity-based
    /// <inheritdoc />
    public async Task AddCustomerAsync(Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);
        await _db.Customers.AddAsync(customer).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<DbUpdateResult> AddCustomerAsync(CustomerDto customerDto)
    {
        var validation = ValidateCustomerDto(customerDto);
        if (validation != ValidationResult.Success)
        {
            return new DbUpdateResult(validation, AddRecordResult.Failed);
        }

        // Use helper to resolve or create customer (ensures tracked entity)
        var customer = await GetOrCreateCustomerAsync(customerDto).ConfigureAwait(false);

        // Assume record already exists
        var result = AddRecordResult.Exists;

        // If new entity was created its Id will be 0 until saved; save to persist
        if (customer.Id != 0)
        {
            return new DbUpdateResult(ValidationResult.Success, result);
        }

        // New entity created
        result = AddRecordResult.Added;
        await _db.SaveChangesAsync().ConfigureAwait(false);

        return new DbUpdateResult(ValidationResult.Success, result);
    }

    /// <inheritdoc />
    public async Task AddFilamentAsync(Filament filament)
    {
        ArgumentNullException.ThrowIfNull(filament);
        await _db.Filaments.AddAsync(filament).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<DbUpdateResult> AddFilamentAsync(FilamentDto filamentDto)
    {
        var validation = ValidateFilamentDto(filamentDto);
        if (validation != ValidationResult.Success)
        {
            return new DbUpdateResult(validation, AddRecordResult.Failed);
        }

        var existing = await GetFilamentAsync(filamentDto.Id).ConfigureAwait(false);

        // Assume filament already exists
        var result = AddRecordResult.Exists;
        if (existing is null)
        {
            // Build filament entity and attach existing related entities if present
            var filament =
                new Filament
                {
                    CostPerWeight    = filamentDto.CostPerWeight,
                    ProductId        = filamentDto.ProductId,
                    ReorderLink      = filamentDto.ReorderLink,
                    FilamentColourId = filamentDto.FilamentColour.Id,
                    FilamentTypeId   = filamentDto.FilamentType.Id,
                    ManufacturerId   = filamentDto.Manufacturer.Id
                };
            await _db.Filaments.AddAsync(filament).ConfigureAwait(false);
            await _db.SaveChangesAsync().ConfigureAwait(false);
            result = AddRecordResult.Added;
        }

        return new DbUpdateResult(ValidationResult.Success, result);
    }

    /// <inheritdoc />
    public async Task AddFilamentColourAsync(FilamentColour filamentColour)
    {
        ArgumentNullException.ThrowIfNull(filamentColour);
        await _db.FilamentColours.AddAsync(filamentColour).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<DbUpdateResult> AddFilamentColourAsync(FilamentColourDto filamentColourDto)
    {
        var validation = ValidateFilamentColourDto(filamentColourDto);
        if (validation != ValidationResult.Success)
        {
            return new DbUpdateResult(validation, AddRecordResult.Failed);
        }

        var col = await GetOrCreateFilamentColourAsync(filamentColourDto).ConfigureAwait(false);

        // Assume colour already exists
        var result = AddRecordResult.Exists;
        if (col.Id == 0)
        {
            await _db.SaveChangesAsync().ConfigureAwait(false);
            result = AddRecordResult.Added;
        }

        return new DbUpdateResult(ValidationResult.Success, result);
    }

    /// <inheritdoc />
    public async Task AddFilamentTypeAsync(FilamentType filamentType)
    {
        ArgumentNullException.ThrowIfNull(filamentType);
        await _db.FilamentTypes.AddAsync(filamentType).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<DbUpdateResult> AddFilamentTypeAsync(FilamentTypeDto filamentTypeDto)
    {
        var validation = ValidateFilamentTypeDto(filamentTypeDto);
        if (validation != ValidationResult.Success)
        {
            return new DbUpdateResult(validation, AddRecordResult.Failed);
        }

        var typ = await GetOrCreateFilamentTypeAsync(filamentTypeDto).ConfigureAwait(false);

        // Assume filament type already exists
        var result = AddRecordResult.Exists;

        if (typ.Id == 0)
        {
            await _db.SaveChangesAsync().ConfigureAwait(false);
            result = AddRecordResult.Added;
        }

        return new DbUpdateResult(ValidationResult.Success, result);
    }

    #region Implementation of IGCodeJournalViewModel
    /// <inheritdoc />
    public async Task<DbUpdateResult> AddManufacturerAsync(ManufacturerDto manufacturerDto)
    {
        var validation = ValidateManufacturerDto(manufacturerDto);
        if (validation != ValidationResult.Success)
        {
            return new DbUpdateResult(validation, AddRecordResult.Failed);
        }

        var man = await GetOrCreateManufacturerAsync(manufacturerDto).ConfigureAwait(false);

        // Assume manufacturer already exists
        var result = AddRecordResult.Exists;
        if (man.Id == 0)
        {
            await _db.SaveChangesAsync().ConfigureAwait(false);
            result = AddRecordResult.Added;
        }

        return new DbUpdateResult(ValidationResult.Success, result);
    }
    #endregion

    /// <inheritdoc />
    public async Task AddManufacturerAsync(Manufacturer manufacturer)
    {
        ArgumentNullException.ThrowIfNull(manufacturer);
        await _db.Manufacturers.AddAsync(manufacturer).ConfigureAwait(false);
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
    public async Task<DbUpdateResult> AddModelDesignAsync(ModelDesignDto modelDesignDto)
    {
        var validation = ValidateModelDesignDto(modelDesignDto);
        if (validation != ValidationResult.Success)
        {
            return new DbUpdateResult(validation, AddRecordResult.Failed);
        }

        var model = await GetOrCreateModelDesignAsync(modelDesignDto).ConfigureAwait(false);

        // Assume design already exists
        var result = AddRecordResult.Exists;
        if (model.Id == 0)
        {
            await _db.SaveChangesAsync().ConfigureAwait(false);
            result = AddRecordResult.Added;
        }

        return new DbUpdateResult(ValidationResult.Success, result);
    }

    /// <inheritdoc />
    public async Task AddPrintingProjectAsync(PrintingProject project)
    {
        ArgumentNullException.ThrowIfNull(project);
        await _db.PrintingProjects.AddAsync(project).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<DbUpdateResult> AddPrintingProjectAsync(PrintingProjectDto projectDto)
    {
        var validation = ValidatePrintingProjectDto(projectDto);
        if (validation != ValidationResult.Success)
        {
            return new DbUpdateResult(validation, AddRecordResult.Failed);
        }

        var existing = await GetPrintingProjectAsync(projectDto.Id).ConfigureAwait(false);
        if (existing is not null)
        {
            return new DbUpdateResult(ValidationResult.Success, AddRecordResult.Exists);
        }

        // Resolve or create Customer
        Customer? customer = null;
        if (projectDto.Customer != null)
        {
            customer = await GetOrCreateCustomerAsync(projectDto.Customer).ConfigureAwait(false);
        }

        // Resolve or create ModelDesign
        ModelDesign? model = null;
        if (projectDto.ModelDesign != null)
        {
            model = await GetOrCreateModelDesignAsync(projectDto.ModelDesign).ConfigureAwait(false);
        }

        // Create project entity and attach resolved relations
        var filaments = new List<Filament>();
        if (projectDto.Filaments != null)
        {
            foreach (var fDto in projectDto.Filaments)
            {
                Filament? fEntity = null;
                if (fDto.Id != 0)
                {
                    fEntity = await _db.Filaments.FindAsync(fDto.Id).ConfigureAwait(false);
                }

                if (fEntity == null)
                {
                    // create filament entity, but attach related lookups (may be newly created and tracked)
                    fEntity = new Filament {CostPerWeight = fDto.CostPerWeight, ProductId = fDto.ProductId, ReorderLink = fDto.ReorderLink};

                    if (fDto.Manufacturer != null)
                    {
                        var man = await GetOrCreateManufacturerAsync(fDto.Manufacturer).ConfigureAwait(false);

                        // prefer navigation property to ensure correct relationship when 'man' is newly added
                        fEntity.Manufacturer = man;
                    }

                    if (fDto.FilamentColour != null)
                    {
                        var col = await GetOrCreateFilamentColourAsync(fDto.FilamentColour).ConfigureAwait(false);
                        fEntity.Colour = col;
                    }

                    if (fDto.FilamentType != null)
                    {
                        var typ = await GetOrCreateFilamentTypeAsync(fDto.FilamentType).ConfigureAwait(false);
                        fEntity.Type = typ;
                    }

                    await _db.Filaments.AddAsync(fEntity).ConfigureAwait(false);
                }

                filaments.Add(fEntity);
            }
        }

        var project =
            new PrintingProject
            {
                Cost      = projectDto.Cost,
                Submitted = projectDto.Submitted.ToDateTime(TimeOnly.MinValue),
                Completed = projectDto.Completed?.ToDateTime(TimeOnly.MinValue),
                Customer  = customer!,
                Model     = model!,
                Filaments = filaments
            };

        await _db.PrintingProjects.AddAsync(project).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);

        return new DbUpdateResult(ValidationResult.Success, AddRecordResult.Added);
    }

    // --- Edit operations for DTOs -------------------------------------------------

    public async Task<ValidationResult> EditCustomerAsync(CustomerDto customerDto)
    {
        var validation = ValidateCustomerDto(customerDto);
        if (validation != ValidationResult.Success)
        {
            return validation;
        }

        if (customerDto.Id == 0)
        {
            return new ValidationResult("Customer Id is required for editing");
        }

        var existing = await _db.Customers.FindAsync(customerDto.Id).ConfigureAwait(false);
        if (existing == null)
        {
            return new ValidationResult("Customer not found");
        }

        existing.Name = customerDto.Name;
        await _db.SaveChangesAsync().ConfigureAwait(false);

        return ValidationResult.Success;
    }

    public async Task<ValidationResult> EditFilamentAsync(FilamentDto filamentDto)
    {
        var validation = ValidateFilamentDto(filamentDto);
        if (validation != ValidationResult.Success)
        {
            return validation;
        }

        if (filamentDto.Id == 0)
        {
            return new ValidationResult("Filament Id is required for editing");
        }

        var existing = await _db.Filaments.FindAsync(filamentDto.Id).ConfigureAwait(false);
        if (existing == null)
        {
            return new ValidationResult("Filament not found");
        }

        existing.CostPerWeight = filamentDto.CostPerWeight;
        existing.ProductId     = filamentDto.ProductId;
        existing.ReorderLink   = filamentDto.ReorderLink;

        // Resolve or create and attach related lookup entities using helpers so EF tracking is correct
        if (filamentDto.Manufacturer != null)
        {
            var man = await GetOrCreateManufacturerAsync(filamentDto.Manufacturer).ConfigureAwait(false);
            existing.Manufacturer = man; // attach tracked navigation property
        }

        if (filamentDto.FilamentColour != null)
        {
            var col = await GetOrCreateFilamentColourAsync(filamentDto.FilamentColour).ConfigureAwait(false);
            existing.Colour = col; // attach tracked navigation property
        }

        if (filamentDto.FilamentType != null)
        {
            var typ = await GetOrCreateFilamentTypeAsync(filamentDto.FilamentType).ConfigureAwait(false);
            existing.Type = typ; // attach tracked navigation property
        }

        await _db.SaveChangesAsync().ConfigureAwait(false);

        return ValidationResult.Success;
    }

    public async Task<ValidationResult> EditFilamentColourAsync(FilamentColourDto filamentColourDto)
    {
        var validation = ValidateFilamentColourDto(filamentColourDto);
        if (validation != ValidationResult.Success)
        {
            return validation;
        }

        if (filamentColourDto.Id == 0)
        {
            return new ValidationResult("Filament colour Id is required for editing");
        }

        var existing = await _db.FilamentColours.FindAsync(filamentColourDto.Id).ConfigureAwait(false);
        if (existing == null)
        {
            return new ValidationResult("Filament colour not found");
        }

        existing.Description = filamentColourDto.Description;
        await _db.SaveChangesAsync().ConfigureAwait(false);

        return ValidationResult.Success;
    }

    public async Task<ValidationResult> EditFilamentTypeAsync(FilamentTypeDto filamentTypeDto)
    {
        var validation = ValidateFilamentTypeDto(filamentTypeDto);
        if (validation != ValidationResult.Success)
        {
            return validation;
        }

        if (filamentTypeDto.Id == 0)
        {
            return new ValidationResult("Filament type Id is required for editing");
        }

        var existing = await _db.FilamentTypes.FindAsync(filamentTypeDto.Id).ConfigureAwait(false);
        if (existing == null)
        {
            return new ValidationResult("Filament type not found");
        }

        existing.Description = filamentTypeDto.Description;
        await _db.SaveChangesAsync().ConfigureAwait(false);

        return ValidationResult.Success;
    }

    public async Task<ValidationResult> EditManufacturerAsync(ManufacturerDto manufacturerDto)
    {
        var validation = ValidateManufacturerDto(manufacturerDto);
        if (validation != ValidationResult.Success)
        {
            return validation;
        }

        if (manufacturerDto.Id == 0)
        {
            return new ValidationResult("Manufacturer Id is required for editing");
        }

        var existing = await _db.Manufacturers.FindAsync(manufacturerDto.Id).ConfigureAwait(false);
        if (existing == null)
        {
            return new ValidationResult("Manufacturer not found");
        }

        existing.Name = manufacturerDto.Name;
        await _db.SaveChangesAsync().ConfigureAwait(false);

        return ValidationResult.Success;
    }

    public async Task<ValidationResult> EditModelDesignAsync(ModelDesignDto modelDesignDto)
    {
        var validation = ValidateModelDesignDto(modelDesignDto);
        if (validation != ValidationResult.Success)
        {
            return validation;
        }

        if (modelDesignDto.Id == 0)
        {
            return new ValidationResult("ModelDesign Id is required for editing");
        }

        var existing = await _db.ModelDesigns.FindAsync(modelDesignDto.Id).ConfigureAwait(false);
        if (existing == null)
        {
            return new ValidationResult("ModelDesign not found");
        }

        existing.Description = modelDesignDto.Description;
        existing.Length      = modelDesignDto.Length;
        existing.Summary     = modelDesignDto.Summary;
        existing.Url         = modelDesignDto.Url;

        await _db.SaveChangesAsync().ConfigureAwait(false);

        return ValidationResult.Success;
    }

    public async Task<ValidationResult> EditPrintingProjectAsync(PrintingProjectDto printingProjectDto)
    {
        var validation = ValidatePrintingProjectDto(printingProjectDto);
        if (validation != ValidationResult.Success)
        {
            return validation;
        }

        if (printingProjectDto.Id == 0)
        {
            return new ValidationResult("Printing project Id is required for editing");
        }

        var existing = await _db.PrintingProjects.Include(p => p.Filaments).FirstOrDefaultAsync(p => p.Id == printingProjectDto.Id).ConfigureAwait(false);
        if (existing == null)
        {
            return new ValidationResult("Printing project not found");
        }

        // Resolve or create customer
        Customer? customer = null;
        if (printingProjectDto.Customer != null)
        {
            customer = await GetOrCreateCustomerAsync(printingProjectDto.Customer).ConfigureAwait(false);
            if (customer.Id == 0)
            {
                await _db.SaveChangesAsync().ConfigureAwait(false); // ensure customer.Id is populated
            }
        }

        // Resolve or create model design
        ModelDesign? model = null;
        if (printingProjectDto.ModelDesign != null)
        {
            model = await GetOrCreateModelDesignAsync(printingProjectDto.ModelDesign).ConfigureAwait(false);
            if (model.Id == 0)
            {
                await _db.SaveChangesAsync().ConfigureAwait(false); // ensure model.Id is populated
            }
        }

        existing.Cost      = printingProjectDto.Cost;
        existing.Submitted = printingProjectDto.Submitted.ToDateTime(TimeOnly.MinValue);
        existing.Completed = printingProjectDto.Completed?.ToDateTime(TimeOnly.MinValue);
        if (customer != null)
        {
            existing.CustomerId = customer.Id;
        }

        if (model != null)
        {
            existing.ModelDesignId = model.Id;
        }

        // Resolve first filament (legacy single-filament support)
        if (printingProjectDto.Filaments?.Any() == true)
        {
            var       fDto    = printingProjectDto.Filaments.First();
            Filament? fEntity = null;
            if (fDto.Id != 0)
            {
                fEntity = await _db.Filaments.FindAsync(fDto.Id).ConfigureAwait(false);
            }

            if (fEntity == null)
            {
                // create filament entity, but attach related lookups
                fEntity = new Filament {CostPerWeight = fDto.CostPerWeight, ProductId = fDto.ProductId, ReorderLink = fDto.ReorderLink};

                if (fDto.Manufacturer != null)
                {
                    var man = await GetOrCreateManufacturerAsync(fDto.Manufacturer).ConfigureAwait(false);
                    fEntity.ManufacturerId = man.Id;
                }

                if (fDto.FilamentColour != null)
                {
                    var col = await GetOrCreateFilamentColourAsync(fDto.FilamentColour).ConfigureAwait(false);
                    fEntity.FilamentColourId = col.Id;
                }

                if (fDto.FilamentType != null)
                {
                    var typ = await GetOrCreateFilamentTypeAsync(fDto.FilamentType).ConfigureAwait(false);
                    fEntity.FilamentTypeId = typ.Id;
                }

                await _db.Filaments.AddAsync(fEntity).ConfigureAwait(false);
                await _db.SaveChangesAsync().ConfigureAwait(false); // ensure new filament has Id
            }
        }

        await _db.SaveChangesAsync().ConfigureAwait(false);

        return ValidationResult.Success;
    }

    /// <inheritdoc />
    public Task<List<CustomerDto>> GetAllCustomersAsync() => _db.Customers.OrderBy(c => c.Name).Select(c => new CustomerDto(c.Id, c.Name)).ToListAsync();

    /// <inheritdoc />
    public Task<List<FilamentColourDto>> GetAllFilamentColoursAsync() =>
        _db.FilamentColours.OrderBy(fc => fc.Description).Select(fc => new FilamentColourDto(fc.Id, fc.Description)).ToListAsync();

    /// <inheritdoc />
    public Task<List<FilamentDto>> GetAllFilamentsAsync() =>
        _db
            .Filaments.Include(f => f.Colour)
            .Include(f => f.Manufacturer)
            .Include(f => f.Type)
            .OrderBy(f => f.Manufacturer.Name)
            .ThenBy(f => f.Type.Description)
            .ThenBy(f => f.Colour.Description)
            .Select(f =>
                        new FilamentDto(
                            f.Id,
                            f.CostPerWeight,
                            f.ProductId,
                            f.ReorderLink,
                            new FilamentColourDto(f.Colour.Id, f.Colour.Description),
                            new FilamentTypeDto(f.Type.Id, f.Type.Description),
                            new ManufacturerDto(f.Manufacturer.Id, f.Manufacturer.Name)))
            .ToListAsync();

    /// <inheritdoc />
    public Task<List<FilamentTypeDto>> GetAllFilamentTypesAsync() =>
        _db.FilamentTypes.OrderBy(ft => ft.Description).Select(ft => new FilamentTypeDto(ft.Id, ft.Description)).ToListAsync();

    /// <inheritdoc />
    public Task<List<ManufacturerDto>> GetAllManufacturersAsync() =>
        _db.Manufacturers.OrderBy(m => m.Name).Select(m => new ManufacturerDto(m.Id, m.Name)).ToListAsync();

    /// <inheritdoc />
    public Task<List<ModelDesignDto>> GetAllModelDesignsAsync() => _db
                                                                   .ModelDesigns.OrderBy(md => md.Summary)
                                                                   .Select(md => new ModelDesignDto(md.Id, md.Description, md.Length, md.Summary, md.Url))
                                                                   .ToListAsync();

    /// <inheritdoc />
    public Task<List<PrintingProjectDto>> GetAllPrintingProjectsAsync() =>
        _db
            .PrintingProjects.Include(p => p.Customer)
            .Include(p => p.Model)
            .Include(p => p.Filaments)
            .ThenInclude(f => f.Manufacturer)
            .Include(p => p.Filaments)
            .ThenInclude(f => f.Colour)
            .Include(p => p.Filaments)
            .ThenInclude(f => f.Type)
            .Select(p =>
                        new PrintingProjectDto(
                            p.Id,
                            p.Cost,
                            DateOnly.FromDateTime(p.Submitted),
                            p.Completed == null ? null : DateOnly.FromDateTime(p.Completed.Value),
                            p.Customer  == null ? null : new CustomerDto(p.Customer.Id, p.Customer.Name),
                            p.Model     == null ? null : new ModelDesignDto(p.Model.Id, p.Model.Description, p.Model.Length, p.Model.Summary, p.Model.Url),
                            p
                                .Filaments.Select(f =>
                                                      new FilamentDto(
                                                          f.Id,
                                                          f.CostPerWeight,
                                                          f.ProductId,
                                                          f.ReorderLink,
                                                          new FilamentColourDto(f.Colour.Id, f.Colour.Description),
                                                          new FilamentTypeDto(f.Type.Id, f.Type.Description),
                                                          new ManufacturerDto(f.Manufacturer.Id, f.Manufacturer.Name)))
                                .ToList()))
            .ToListAsync();

    /// <inheritdoc />
    public async Task<CustomerDto?> GetCustomerAsync(int id)
    {
        var c = await _db.Customers.FindAsync(id).ConfigureAwait(false);

        return c == null ? null : new CustomerDto(c.Id, c.Name);
    }

    /// <inheritdoc />
    public async Task<FilamentDto?> GetFilamentAsync(int id)
    {
        var f =
            await _db
                  .Filaments.Include(x => x.Colour)
                  .Include(x => x.Manufacturer)
                  .Include(x => x.Type)
                  .FirstOrDefaultAsync(x => x.Id == id)
                  .ConfigureAwait(false);
        if (f == null)
        {
            return null;
        }

        return new FilamentDto(
            f.Id,
            f.CostPerWeight,
            f.ProductId,
            f.ReorderLink,
            new FilamentColourDto(f.Colour.Id, f.Colour.Description),
            new FilamentTypeDto(f.Type.Id, f.Type.Description),
            new ManufacturerDto(f.Manufacturer.Id, f.Manufacturer.Name));
    }

    /// <inheritdoc />
    public async Task<FilamentColourDto?> GetFilamentColourAsync(int id)
    {
        var fc = await _db.FilamentColours.FindAsync(id).ConfigureAwait(false);

        return fc == null ? null : new FilamentColourDto(fc.Id, fc.Description);
    }

    /// <inheritdoc />
    public async Task<FilamentTypeDto?> GetFilamentTypeAsync(int id)
    {
        var ft = await _db.FilamentTypes.FindAsync(id).ConfigureAwait(false);

        return ft == null ? null : new FilamentTypeDto(ft.Id, ft.Description);
    }

    /// <inheritdoc />
    public string? GetLastImportPath() =>
        OperatingSystem.IsWindows() ? Registry.GetValue(GCodeJournalRegistryPath, ImportPathRegistryKey, null) as string : null;

    /// <inheritdoc />
    public async Task<ManufacturerDto?> GetManufacturerAsync(int id)
    {
        var m = await _db.Manufacturers.FindAsync(id).ConfigureAwait(false);

        return m == null ? null : new ManufacturerDto(m.Id, m.Name);
    }

    /// <inheritdoc />
    public async Task<ModelDesignDto?> GetModelDesignAsync(int id)
    {
        var md = await _db.ModelDesigns.FindAsync(id).ConfigureAwait(false);

        return md == null ? null : new ModelDesignDto(md.Id, md.Description, md.Length, md.Summary, md.Url);
    }

    /// <inheritdoc />
    public async Task<PrintingProjectDto?> GetPrintingProjectAsync(int id)
    {
        var p =
            await _db
                  .PrintingProjects.Include(pr => pr.Customer)
                  .Include(pr => pr.Model)
                  .Include(pr => pr.Filaments)
                  .ThenInclude(f => f.Manufacturer)
                  .Include(pr => pr.Filaments)
                  .ThenInclude(f => f.Colour)
                  .Include(pr => pr.Filaments)
                  .ThenInclude(f => f.Type)
                  .FirstOrDefaultAsync(pr => pr.Id == id)
                  .ConfigureAwait(false);

        if (p == null)
        {
            return null;
        }

        var filaments =
            p
                .Filaments.Select(f =>
                                      new FilamentDto(
                                          f.Id,
                                          f.CostPerWeight,
                                          f.ProductId,
                                          f.ReorderLink,
                                          new FilamentColourDto(f.Colour.Id, f.Colour.Description),
                                          new FilamentTypeDto(f.Type.Id, f.Type.Description),
                                          new ManufacturerDto(f.Manufacturer.Id, f.Manufacturer.Name)))
                .ToList();

        return new PrintingProjectDto(
            p.Id,
            p.Cost,
            DateOnly.FromDateTime(p.Submitted),
            p.Completed == null ? null : DateOnly.FromDateTime(p.Completed.Value),
            p.Customer  == null ? null : new CustomerDto(p.Customer.Id, p.Customer.Name),
            p.Model     == null ? null : new ModelDesignDto(p.Model.Id, p.Model.Description, p.Model.Length, p.Model.Summary, p.Model.Url),
            filaments);
    }

    /// <inheritdoc />
    public async Task<List<CsvImporter.ImportFileResult>> ImportFromCsvAsync(
        string            csvPath,
        ILogger           appLogger,
        bool              updateExisting = true,
        char              delimiter      = ',',
        CancellationToken ct             = default)
    {
        var importer = new CsvImporter(_db, this);

        return await importer.ImportFromPathAsync(csvPath, appLogger, updateExisting, delimiter, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<CsvImporter.ImportFileResult> ImportFromCsvAsync(
        Stream            stream,
        ILogger           appLogger,
        string?           fileName       = null,
        bool              updateExisting = true,
        char              delimiter      = ',',
        CancellationToken ct             = default)
    {
        var importer = new CsvImporter(_db, this);

        return await importer.ImportStreamAsync(stream, appLogger, fileName, updateExisting, delimiter, ct).ConfigureAwait(false);
    }

    public void SetImportPath(string path)
    {
        if (OperatingSystem.IsWindows())
        {
            Registry.SetValue(GCodeJournalRegistryPath, ImportPathRegistryKey, path);
        }
    }
    #endregion

    #region Validation helpers
    static ValidationResult ValidateCustomerDto(CustomerDto dto)
    {
        if (dto is null)
        {
            return new ValidationResult("Customer DTO is required");
        }

        return string.IsNullOrWhiteSpace(dto.Name) ? new ValidationResult("Customer name is required", [nameof(dto.Name)]) : ValidationResult.Success!;
    }

    static ValidationResult ValidateManufacturerDto(ManufacturerDto dto)
    {
        if (dto is null)
        {
            return new ValidationResult("Manufacturer DTO is required");
        }

        return string.IsNullOrWhiteSpace(dto.Name) ? new ValidationResult("Manufacturer name is required", [nameof(dto.Name)]) : ValidationResult.Success!;
    }

    static ValidationResult ValidateFilamentColourDto(FilamentColourDto dto)
    {
        if (dto is null)
        {
            return new ValidationResult("Filament colour DTO is required");
        }

        return string.IsNullOrWhiteSpace(dto.Description)
                   ? new ValidationResult("Filament colour description is required", [nameof(dto.Description)])
                   : ValidationResult.Success!;
    }

    static ValidationResult ValidateFilamentTypeDto(FilamentTypeDto dto)
    {
        if (dto is null)
        {
            return new ValidationResult("Filament type DTO is required");
        }

        return string.IsNullOrWhiteSpace(dto.Description)
                   ? new ValidationResult("Filament type description is required", [nameof(dto.Description)])
                   : ValidationResult.Success!;
    }

    static ValidationResult ValidateModelDesignDto(ModelDesignDto dto)
    {
        if (dto is null)
        {
            return new ValidationResult("ModelDesign DTO is required");
        }

        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            return new ValidationResult("ModelDesign description is required", [nameof(dto.Description)]);
        }

        return dto.Length < 0 ? new ValidationResult("ModelDesign length must be non-negative", [nameof(dto.Length)]) : ValidationResult.Success!;
    }

    static ValidationResult ValidateFilamentDto(FilamentDto dto)
    {
        if (dto is null)
        {
            return new ValidationResult("Filament DTO is required");
        }

        var errors = new List<ValidationResult>();
        if (dto.CostPerWeight < 0)
        {
            errors.Add(new ValidationResult("Filament cost must be non-negative", [nameof(dto.CostPerWeight)]));
        }

        if (dto.Manufacturer is null)
        {
            errors.Add(new ValidationResult("Filament manufacturer is required", [nameof(dto.Manufacturer)]));
        }

        if (dto.FilamentColour is null)
        {
            errors.Add(new ValidationResult("Filament colour is required", [nameof(dto.FilamentColour)]));
        }

        if (dto.FilamentType is null)
        {
            errors.Add(new ValidationResult("Filament type is required", [nameof(dto.FilamentType)]));
        }

        return errors.Count > 0 ? new ValidationResult(string.Join("; ", errors.Select(e => e.ErrorMessage))) : ValidationResult.Success!;
    }

    static ValidationResult ValidatePrintingProjectDto(PrintingProjectDto dto)
    {
        if (dto is null)
        {
            return new ValidationResult("PrintingProject DTO is required");
        }

        var errors = new List<ValidationResult>();
        if (dto.Cost < 0)
        {
            errors.Add(new ValidationResult("Printing project cost must be non-negative", [nameof(dto.Cost)]));
        }

        if (dto.Customer is null)
        {
            errors.Add(new ValidationResult("Printing project must have a customer", [nameof(dto.Customer)]));
        }

        if (dto.ModelDesign is null)
        {
            errors.Add(new ValidationResult("Printing project must have a model design", [nameof(dto.ModelDesign)]));
        }

        return errors.Count > 0 ? new ValidationResult(string.Join("; ", errors.Select(e => e.ErrorMessage))) : ValidationResult.Success!;
    }
    #endregion

    #region Delete operations
    public async Task<ValidationResult> DeleteCustomerAsync(CustomerDto customerDto)
    {
        if (customerDto == null)
        {
            return new ValidationResult("Customer DTO is required");
        }

        if (customerDto.Id == 0)
        {
            return new ValidationResult("Customer Id is required for deletion");
        }

        var existing = await _db.Customers.FindAsync(customerDto.Id).ConfigureAwait(false);
        if (existing == null)
        {
            return new ValidationResult("Customer not found");
        }

        _db.Customers.Remove(existing);
        await _db.SaveChangesAsync().ConfigureAwait(false);

        return ValidationResult.Success!;
    }

    public async Task<ValidationResult> DeleteFilamentAsync(FilamentDto filamentDto)
    {
        if (filamentDto == null)
        {
            return new ValidationResult("Filament DTO is required");
        }

        if (filamentDto.Id == 0)
        {
            return new ValidationResult("Filament Id is required for deletion");
        }

        var existing = await _db.Filaments.FindAsync(filamentDto.Id).ConfigureAwait(false);
        if (existing == null)
        {
            return new ValidationResult("Filament not found");
        }

        _db.Filaments.Remove(existing);
        await _db.SaveChangesAsync().ConfigureAwait(false);

        return ValidationResult.Success!;
    }

    public async Task<ValidationResult> DeleteFilamentColourAsync(FilamentColourDto filamentColourDto)
    {
        var validation = ValidateFilamentColourDto(filamentColourDto);
        if (validation != ValidationResult.Success)
        {
            return validation;
        }

        if (filamentColourDto.Id == 0)
        {
            return new ValidationResult("Filament colour Id is required for deletion");
        }

        var existing = await _db.FilamentColours.FindAsync(filamentColourDto.Id).ConfigureAwait(false);
        if (existing == null)
        {
            return new ValidationResult("Filament colour not found");
        }

        // Prevent deletion if in use
        var inUse = await _db.Filaments.AnyAsync(f => f.FilamentColourId == existing.Id).ConfigureAwait(false);
        if (inUse)
        {
            return new ValidationResult("Filament colour is in use and cannot be deleted");
        }

        _db.FilamentColours.Remove(existing);
        await _db.SaveChangesAsync().ConfigureAwait(false);

        return ValidationResult.Success;
    }

    public async Task<ValidationResult> DeleteFilamentTypeAsync(FilamentTypeDto filamentTypeDto)
    {
        var validation = ValidateFilamentTypeDto(filamentTypeDto);
        if (validation != ValidationResult.Success)
        {
            return validation;
        }

        if (filamentTypeDto.Id == 0)
        {
            return new ValidationResult("Filament type Id is required for deletion");
        }

        var existing = await _db.FilamentTypes.FindAsync(filamentTypeDto.Id).ConfigureAwait(false);
        if (existing == null)
        {
            return new ValidationResult("Filament type not found");
        }

        // Prevent deletion if in use
        var inUse = await _db.Filaments.AnyAsync(f => f.FilamentTypeId == existing.Id).ConfigureAwait(false);
        if (inUse)
        {
            return new ValidationResult("Filament type is in use and cannot be deleted");
        }

        _db.FilamentTypes.Remove(existing);
        await _db.SaveChangesAsync().ConfigureAwait(false);

        return ValidationResult.Success;
    }

    public async Task<ValidationResult> DeleteManufacturerAsync(ManufacturerDto manufacturerDto)
    {
        var validation = ValidateManufacturerDto(manufacturerDto);
        if (validation != ValidationResult.Success)
        {
            return validation;
        }

        if (manufacturerDto.Id == 0)
        {
            return new ValidationResult("Manufacturer Id is required for deletion");
        }

        var existing = await _db.Manufacturers.FindAsync(manufacturerDto.Id).ConfigureAwait(false);
        if (existing == null)
        {
            return new ValidationResult("Manufacturer not found");
        }

        // Prevent deletion if in use
        var inUse = await _db.Filaments.AnyAsync(f => f.ManufacturerId == existing.Id).ConfigureAwait(false);
        if (inUse)
        {
            return new ValidationResult("Manufacturer is in use and cannot be deleted");
        }

        _db.Manufacturers.Remove(existing);
        await _db.SaveChangesAsync().ConfigureAwait(false);

        return ValidationResult.Success;
    }

    public async Task<ValidationResult> DeleteModelDesignAsync(ModelDesignDto modelDesignDto)
    {
        var validation = ValidateModelDesignDto(modelDesignDto);
        if (validation != ValidationResult.Success)
        {
            return validation;
        }

        if (modelDesignDto.Id == 0)
        {
            return new ValidationResult("ModelDesign Id is required for deletion");
        }

        var existing = await _db.ModelDesigns.FindAsync(modelDesignDto.Id).ConfigureAwait(false);
        if (existing == null)
        {
            return new ValidationResult("ModelDesign not found");
        }

        // Prevent deletion if in use
        var inUse = await _db.PrintingProjects.AnyAsync(p => p.ModelDesignId == existing.Id).ConfigureAwait(false);
        if (inUse)
        {
            return new ValidationResult("Model design is in use and cannot be deleted");
        }

        _db.ModelDesigns.Remove(existing);
        await _db.SaveChangesAsync().ConfigureAwait(false);

        return ValidationResult.Success;
    }

    public async Task<ValidationResult> DeletePrintingProjectAsync(PrintingProjectDto printingProjectDto)
    {
        if (printingProjectDto == null)
        {
            return new ValidationResult("Printing project DTO is required");
        }

        if (printingProjectDto.Id == 0)
        {
            return new ValidationResult("Printing project Id is required for deletion");
        }

        var existing = await _db.PrintingProjects.FindAsync(printingProjectDto.Id).ConfigureAwait(false);
        if (existing == null)
        {
            return new ValidationResult("Printing project not found");
        }

        _db.PrintingProjects.Remove(existing);
        await _db.SaveChangesAsync().ConfigureAwait(false);

        return ValidationResult.Success!;
    }
    #endregion

    #region Helper lookups
    public async Task<Manufacturer> GetOrCreateManufacturerAsync(ManufacturerDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        Manufacturer? existing = null;
        if (dto.Id != 0)
        {
            existing = await _db.Manufacturers.FindAsync(dto.Id).ConfigureAwait(false);
        }

        // case-insensitive lookup using DB collation (SQLite NOCASE)
        existing ??= await _db.Manufacturers.FirstOrDefaultAsync(m => EF.Functions.Collate(m.Name, "NOCASE") == dto.Name).ConfigureAwait(false);
        if (existing != null)
        {
            return existing;
        }

        var created = dto.ToEntity();

        // ensure new entity doesn't carry an explicit Id
        created.Id = 0;
        await _db.Manufacturers.AddAsync(created).ConfigureAwait(false);

        // Note: do not SaveChanges here; caller will save once after composing related entities
        return created;
    }

    public async Task<FilamentColour> GetOrCreateFilamentColourAsync(FilamentColourDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        FilamentColour? existing = null;
        if (dto.Id != 0)
        {
            existing = await _db.FilamentColours.FindAsync(dto.Id).ConfigureAwait(false);
        }

        // case-insensitive lookup using DB collation (SQLite NOCASE)
        existing ??=
            await _db.FilamentColours.FirstOrDefaultAsync(fc => EF.Functions.Collate(fc.Description, "NOCASE") == dto.Description).ConfigureAwait(false);
        if (existing != null)
        {
            return existing;
        }

        if (ValidateFilamentColourDto(dto) != ValidationResult.Success)
        {
            throw new ValidationException($"Filament colour {dto.Id} ({(string.IsNullOrWhiteSpace(dto.Description) ? "[null]" : dto.Description)}) is invalid");
        }

        var created = dto.ToEntity();
        created.Id = 0;
        await _db.FilamentColours.AddAsync(created).ConfigureAwait(false);

        return created;
    }

    public async Task<FilamentType> GetOrCreateFilamentTypeAsync(FilamentTypeDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        FilamentType? existing = null;
        if (dto.Id != 0)
        {
            existing = await _db.FilamentTypes.FindAsync(dto.Id).ConfigureAwait(false);

            // Check that the Description matches if Id is provided
            if (!existing?.Description.Equals(dto.Description, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                existing = null;
            }
        }

        // case-insensitive lookup using DB collation (SQLite NOCASE)
        existing ??= await _db.FilamentTypes.FirstOrDefaultAsync(ft => EF.Functions.Collate(ft.Description, "NOCASE") == dto.Description).ConfigureAwait(false);
        if (existing != null)
        {
            return existing;
        }

        var created = dto.ToEntity();
        created.Id = 0;
        await _db.FilamentTypes.AddAsync(created).ConfigureAwait(false);

        return created;
    }

    async Task<Customer> GetOrCreateCustomerAsync(CustomerDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        // Prefer matching by name. If a name is supplied, use case-insensitive lookup
        // and return existing customer or create a new one with that name.
        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            var existingByName = await _db.Customers.FirstOrDefaultAsync(c => EF.Functions.Collate(c.Name, "NOCASE") == dto.Name).ConfigureAwait(false);
            if (existingByName != null)
            {
                return existingByName;
            }

            var created = dto.ToEntity();
            created.Id = 0;
            await _db.Customers.AddAsync(created).ConfigureAwait(false);
            return created;
        }

        // If no name was provided, fall back to Id lookup if available
        if (dto.Id != 0)
        {
            var existingById = await _db.Customers.FindAsync(dto.Id).ConfigureAwait(false);
            if (existingById != null)
            {
                return existingById;
            }
        }

        // As a last resort create from dto (will likely fail validation elsewhere if name missing)
        var fallback = dto.ToEntity();
        fallback.Id = 0;
        await _db.Customers.AddAsync(fallback).ConfigureAwait(false);
        return fallback;
    }

    async Task<ModelDesign> GetOrCreateModelDesignAsync(ModelDesignDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ModelDesign? existing = null;
        if (dto.Id != 0)
        {
            existing = await _db.ModelDesigns.FindAsync(dto.Id).ConfigureAwait(false);
        }

        existing ??= await _db.ModelDesigns.FirstOrDefaultAsync(md => EF.Functions.Collate(md.Summary, "NOCASE") == dto.Summary).ConfigureAwait(false);
        if (existing != null)
        {
            return existing;
        }

        var created = dto.ToEntity();
        created.Id = 0;
        await _db.ModelDesigns.AddAsync(created).ConfigureAwait(false);

        return created;
    }
    #endregion
}