namespace gCodeJournal.ViewModel;

#region Using Directives
using System.ComponentModel.DataAnnotations;
using DTOs;
using Mapping;
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
    // keep some legacy add methods on the ViewModel — they remain entity-based
    /// <inheritdoc />
    public async Task AddCustomerAsync(Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);
        await _db.Customers.AddAsync(customer).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ValidationResult> AddCustomerAsync(CustomerDto customerDto)
    {
        var validation = ValidateCustomerDto(customerDto);
        if (validation != ValidationResult.Success)
        {
            return validation;
        }

        Customer? existing = null;
        if (customerDto.Id != 0)
        {
            existing = await _db.Customers.FindAsync(customerDto.Id).ConfigureAwait(false);
        }

        // perform case-insensitive lookup
        existing ??= await _db.Customers.FirstOrDefaultAsync(c => c.Name.ToLower() == customerDto.Name.ToLower()).ConfigureAwait(false);
        if (existing != null)
        {
            return ValidationResult.Success; // already exists — treat as successful no-op
        }

        await _db.Customers.AddAsync(customerDto.ToEntity()).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
        return ValidationResult.Success;
    }

    /// <inheritdoc />
    public async Task AddFilamentAsync(Filament filament)
    {
        ArgumentNullException.ThrowIfNull(filament);
        await _db.Filaments.AddAsync(filament).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ValidationResult> AddFilamentAsync(FilamentDto filamentDto)
    {
        var validation = ValidateFilamentDto(filamentDto);
        if (validation != ValidationResult.Success)
        {
            return validation;
        }

        // Build filament entity and attach existing related entities if present
        var filament = new Filament
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
        return ValidationResult.Success;
    }

    /// <inheritdoc />
    public async Task AddFilamentColourAsync(FilamentColour filamentColour)
    {
        ArgumentNullException.ThrowIfNull(filamentColour);
        await _db.FilamentColours.AddAsync(filamentColour).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ValidationResult> AddFilamentColourAsync(FilamentColourDto filamentColourDto)
    {
        var validation = ValidateFilamentColourDto(filamentColourDto);
        if (validation != ValidationResult.Success)
        {
            return validation;
        }

        FilamentColour? existing = null;
        if (filamentColourDto.Id != 0)
        {
            existing = await _db.FilamentColours.FindAsync(filamentColourDto.Id).ConfigureAwait(false);
        }

        // case-insensitive lookup
        existing ??= await _db.FilamentColours.FirstOrDefaultAsync(fc => fc.Description.ToLower() == filamentColourDto.Description.ToLower()).ConfigureAwait(false);
        if (existing != null)
        {
            return ValidationResult.Success;
        }

        await _db.FilamentColours.AddAsync(filamentColourDto.ToEntity()).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
        return ValidationResult.Success;
    }

    /// <inheritdoc />
    public async Task AddFilamentTypeAsync(FilamentType filamentType)
    {
        ArgumentNullException.ThrowIfNull(filamentType);
        await _db.FilamentTypes.AddAsync(filamentType).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ValidationResult> AddFilamentTypeAsync(FilamentTypeDto filamentTypeDto)
    {
        var validation = ValidateFilamentTypeDto(filamentTypeDto);
        if (validation != ValidationResult.Success)
        {
            return validation;
        }

        FilamentType? existing = null;
        if (filamentTypeDto.Id != 0)
        {
            existing = await _db.FilamentTypes.FindAsync(filamentTypeDto.Id).ConfigureAwait(false);
        }

        // case-insensitive lookup
        existing ??= await _db.FilamentTypes.FirstOrDefaultAsync(ft => ft.Description.ToLower() == filamentTypeDto.Description.ToLower()).ConfigureAwait(false);
        if (existing != null)
        {
            return ValidationResult.Success;
        }

        await _db.FilamentTypes.AddAsync(filamentTypeDto.ToEntity()).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
        return ValidationResult.Success;
    }

    #region Implementation of IGCodeJournalViewModel
    /// <inheritdoc />
    public async Task<ValidationResult> AddManufacturerAsync(ManufacturerDto manufacturerDto)
    {
        var validation = ValidateManufacturerDto(manufacturerDto);
        if (validation != ValidationResult.Success)
        {
            return validation;
        }

        Manufacturer? existing = null;
        if (manufacturerDto.Id != 0)
        {
            existing = await _db.Manufacturers.FindAsync(manufacturerDto.Id).ConfigureAwait(false);
        }

        // case-insensitive lookup
        existing ??= await _db.Manufacturers.FirstOrDefaultAsync(m => m.Name.ToLower() == manufacturerDto.Name.ToLower()).ConfigureAwait(false);
        if (existing != null)
        {
            return ValidationResult.Success;
        }

        return await AddManufacturerAsync(manufacturerDto.ToEntity()).ConfigureAwait(false);
    }
    #endregion

    /// <inheritdoc />
    public async Task<ValidationResult> AddManufacturerAsync(Manufacturer manufacturer)
    {
        ArgumentNullException.ThrowIfNull(manufacturer);
        await _db.Manufacturers.AddAsync(manufacturer).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
        return ValidationResult.Success!;
    }

    /// <inheritdoc />
    public async Task AddModelDesignAsync(ModelDesign modelDesign)
    {
        ArgumentNullException.ThrowIfNull(modelDesign);
        await _db.ModelDesigns.AddAsync(modelDesign).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ValidationResult> AddModelDesignAsync(ModelDesignDto modelDesignDto)
    {
        var validation = ValidateModelDesignDto(modelDesignDto);
        if (validation != ValidationResult.Success)
        {
            return validation;
        }

        ModelDesign? existing = null;
        if (modelDesignDto.Id != 0)
        {
            existing = await _db.ModelDesigns.FindAsync(modelDesignDto.Id).ConfigureAwait(false);
        }

        // case-insensitive lookup
        existing ??= await _db.ModelDesigns.FirstOrDefaultAsync(md => md.Description.ToLower() == modelDesignDto.Description.ToLower()).ConfigureAwait(false);
        if (existing != null)
        {
            return ValidationResult.Success;
        }

        await _db.ModelDesigns.AddAsync(modelDesignDto.ToEntity()).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
        return ValidationResult.Success;
    }

    /// <inheritdoc />
    public async Task AddPrintingProjectAsync(PrintingProject project)
    {
        ArgumentNullException.ThrowIfNull(project);
        await _db.PrintingProjects.AddAsync(project).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ValidationResult> AddPrintingProjectAsync(PrintingProjectDto projectDto)
    {
        var validation = ValidatePrintingProjectDto(projectDto);
        if (validation != ValidationResult.Success)
        {
            return validation;
        }

        // Resolve or create Customer
        Customer? customer = null;
        if (projectDto.Customer != null)
        {
            if (projectDto.Customer.Id != 0)
            {
                customer = await _db.Customers.FindAsync(projectDto.Customer.Id).ConfigureAwait(false);
            }

            // case-insensitive lookup
            customer ??= await _db.Customers.FirstOrDefaultAsync(c => c.Name.ToLower() == projectDto.Customer.Name.ToLower()).ConfigureAwait(false);
            if (customer == null)
            {
                customer = projectDto.Customer.ToEntity();
                await _db.Customers.AddAsync(customer).ConfigureAwait(false);
            }
        }

        // Resolve or create ModelDesign
        ModelDesign? model = null;
        if (projectDto.ModelDesign != null)
        {
            if (projectDto.ModelDesign.Id != 0)
            {
                model = await _db.ModelDesigns.FindAsync(projectDto.ModelDesign.Id).ConfigureAwait(false);
            }

            // case-insensitive lookup
            model ??= await _db.ModelDesigns.FirstOrDefaultAsync(md => md.Description.ToLower() == projectDto.ModelDesign.Description.ToLower()).ConfigureAwait(false);
            if (model == null)
            {
                model = projectDto.ModelDesign.ToEntity();
                await _db.ModelDesigns.AddAsync(model).ConfigureAwait(false);
            }
        }

        // Create project entity and attach resolved relations
        var project = new PrintingProject
        {
            Cost       = projectDto.Cost,
            Submitted  = projectDto.Submitted,
            Completed  = projectDto.Completed,
            Customer   = customer!,
            Model      = model!,
            FilamentId = projectDto.Filaments.First().Id // TODO: handle multiple filaments properly
        };

        await _db.PrintingProjects.AddAsync(project).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
        return ValidationResult.Success;
    }

    /// <inheritdoc />
    public Task<List<CustomerDto>> GetAllCustomersAsync()
    {
        return _db.Customers.OrderBy(c => c.Name).Select(c => new CustomerDto(c.Id, c.Name)).ToListAsync();
    }

    /// <inheritdoc />
    public Task<List<FilamentColourDto>> GetAllFilamentColoursAsync()
    {
        return _db.FilamentColours.OrderBy(fc => fc.Description).Select(fc => new FilamentColourDto(fc.Id, fc.Description)).ToListAsync();
    }

    /// <inheritdoc />
    public Task<List<FilamentDto>> GetAllFilamentsAsync()
    {
        return _db.Filaments.Include(f => f.Colour)
                  .Include(f => f.Manufacturer)
                  .Include(f => f.Type)
                  .OrderBy(f => f.ManufacturerId)
                  .Select(f => new FilamentDto(
                              f.Id,
                              f.CostPerWeight,
                              f.ProductId,
                              f.ReorderLink,
                              new FilamentColourDto(f.Colour.Id, f.Colour.Description),
                              new FilamentTypeDto(f.Type.Id, f.Type.Description),
                              new ManufacturerDto(f.Manufacturer.Id, f.Manufacturer.Name)))
                  .ToListAsync();
    }

    /// <inheritdoc />
    public Task<List<FilamentTypeDto>> GetAllFilamentTypesAsync()
    {
        return _db.FilamentTypes.OrderBy(ft => ft.Description).Select(ft => new FilamentTypeDto(ft.Id, ft.Description)).ToListAsync();
    }

    /// <inheritdoc />
    public Task<List<ManufacturerDto>> GetAllManufacturersAsync()
    {
        return _db.Manufacturers.OrderBy(m => m.Name).Select(m => new ManufacturerDto(m.Id, m.Name)).ToListAsync();
    }

    /// <inheritdoc />
    public Task<List<ModelDesignDto>> GetAllModelDesignsAsync()
    {
        return _db.ModelDesigns.OrderBy(md => md.Summary).Select(md => new ModelDesignDto(md.Id, md.Description, md.Length, md.Summary, md.Url)).ToListAsync();
    }

    /// <inheritdoc />
    public Task<List<PrintingProjectDto>> GetAllPrintingProjectsAsync()
    {
        return _db.PrintingProjects.Include(p => p.Customer)
                  .Include(p => p.Model)
                  .Include(p => p.Filaments)
                  .ThenInclude(f => f.Manufacturer)
                  .Include(p => p.Filaments)
                  .ThenInclude(f => f.Colour)
                  .Include(p => p.Filaments)
                  .ThenInclude(f => f.Type)
                  .Select(p => new PrintingProjectDto(
                              p.Id,
                              p.Cost,
                              p.Submitted,
                              p.Completed,
                              p.Customer == null ? null : new CustomerDto(p.Customer.Id, p.Customer.Name),
                              p.Model    == null ? null : new ModelDesignDto(p.Model.Id, p.Model.Description, p.Model.Length, p.Model.Summary, p.Model.Url),
                              p.Filaments.Select(f => new FilamentDto(
                                                     f.Id,
                                                     f.CostPerWeight,
                                                     f.ProductId,
                                                     f.ReorderLink,
                                                     new FilamentColourDto(f.Colour.Id, f.Colour.Description),
                                                     new FilamentTypeDto(f.Type.Id, f.Type.Description),
                                                     new ManufacturerDto(f.Manufacturer.Id, f.Manufacturer.Name)))
                               .ToList()))
                  .ToListAsync();
    }
    #endregion

    #region Validation helpers
    private static ValidationResult ValidateCustomerDto(CustomerDto dto)
    {
        if (dto is null)
        {
            return new ValidationResult("Customer DTO is required");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return new ValidationResult("Customer name is required", new[] {nameof(dto.Name)});
        }

        return ValidationResult.Success!;
    }

    private static ValidationResult ValidateManufacturerDto(ManufacturerDto dto)
    {
        if (dto is null)
        {
            return new ValidationResult("Manufacturer DTO is required");
        }

        return string.IsNullOrWhiteSpace(dto.Name) ? new ValidationResult("Manufacturer name is required", [nameof(dto.Name)]) : ValidationResult.Success!;
    }

    private static ValidationResult ValidateFilamentColourDto(FilamentColourDto dto)
    {
        if (dto is null)
        {
            return new ValidationResult("Filament colour DTO is required");
        }

        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            return new ValidationResult("Filament colour description is required", new[] {nameof(dto.Description)});
        }

        return ValidationResult.Success!;
    }

    private static ValidationResult ValidateFilamentTypeDto(FilamentTypeDto dto)
    {
        if (dto is null)
        {
            return new ValidationResult("Filament type DTO is required");
        }

        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            return new ValidationResult("Filament type description is required", new[] {nameof(dto.Description)});
        }

        return ValidationResult.Success!;
    }

    private static ValidationResult ValidateModelDesignDto(ModelDesignDto dto)
    {
        if (dto is null)
        {
            return new ValidationResult("ModelDesign DTO is required");
        }

        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            return new ValidationResult("ModelDesign description is required", new[] {nameof(dto.Description)});
        }

        if (dto.Length < 0)
        {
            return new ValidationResult("ModelDesign length must be non-negative", new[] {nameof(dto.Length)});
        }

        return ValidationResult.Success!;
    }

    private static ValidationResult ValidateFilamentDto(FilamentDto dto)
    {
        if (dto is null)
        {
            return new ValidationResult("Filament DTO is required");
        }

        var errors = new List<ValidationResult>();
        if (dto.CostPerWeight < 0)
        {
            errors.Add(new ValidationResult("Filament cost must be non-negative", new[] {nameof(dto.CostPerWeight)}));
        }

        if (dto.Manufacturer is null)
        {
            errors.Add(new ValidationResult("Filament manufacturer is required", new[] {nameof(dto.Manufacturer)}));
        }

        if (dto.FilamentColour is null)
        {
            errors.Add(new ValidationResult("Filament colour is required", new[] {nameof(dto.FilamentColour)}));
        }

        if (dto.FilamentType is null)
        {
            errors.Add(new ValidationResult("Filament type is required", new[] {nameof(dto.FilamentType)}));
        }

        if (errors.Count > 0)
        {
            return new ValidationResult(string.Join("; ", errors.Select(e => e.ErrorMessage)));
        }

        return ValidationResult.Success!;
    }

    private static ValidationResult ValidatePrintingProjectDto(PrintingProjectDto dto)
    {
        if (dto is null)
        {
            return new ValidationResult("PrintingProject DTO is required");
        }

        var errors = new List<ValidationResult>();
        if (dto.Cost < 0)
        {
            errors.Add(new ValidationResult("Printing project cost must be non-negative", new[] {nameof(dto.Cost)}));
        }

        if (dto.Customer is null)
        {
            errors.Add(new ValidationResult("Printing project must have a customer", new[] {nameof(dto.Customer)}));
        }

        if (dto.ModelDesign is null)
        {
            errors.Add(new ValidationResult("Printing project must have a model design", new[] {nameof(dto.ModelDesign)}));
        }

        if (errors.Count > 0)
        {
            return new ValidationResult(string.Join("; ", errors.Select(e => e.ErrorMessage)));
        }

        return ValidationResult.Success!;
    }
    #endregion

    #region Helper lookups
    private async Task<Manufacturer> GetOrCreateManufacturerAsync(ManufacturerDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        Manufacturer? existing = null;
        if (dto.Id != 0)
        {
            existing = await _db.Manufacturers.FindAsync(dto.Id).ConfigureAwait(false);
        }

        // case-insensitive lookup
        existing ??= await _db.Manufacturers.FirstOrDefaultAsync(m => m.Name.ToLower() == dto.Name.ToLower()).ConfigureAwait(false);
        if (existing != null)
        {
            return existing;
        }

        var created = dto.ToEntity();
        await _db.Manufacturers.AddAsync(created).ConfigureAwait(false);

        // Note: do not SaveChanges here; caller will save once after composing related entities
        return created;
    }

    private async Task<FilamentColour> GetOrCreateFilamentColourAsync(FilamentColourDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        FilamentColour? existing = null;
        if (dto.Id != 0)
        {
            existing = await _db.FilamentColours.FindAsync(dto.Id).ConfigureAwait(false);
        }

        // case-insensitive lookup
        existing ??= await _db.FilamentColours.FirstOrDefaultAsync(fc => fc.Description.ToLower() == dto.Description.ToLower()).ConfigureAwait(false);
        if (existing != null)
        {
            return existing;
        }

        var created = dto.ToEntity();
        await _db.FilamentColours.AddAsync(created).ConfigureAwait(false);
        return created;
    }

    private async Task<FilamentType> GetOrCreateFilamentTypeAsync(FilamentTypeDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        FilamentType? existing = null;
        if (dto.Id != 0)
        {
            existing = await _db.FilamentTypes.FindAsync(dto.Id).ConfigureAwait(false);
        }

        // case-insensitive lookup
        existing ??= await _db.FilamentTypes.FirstOrDefaultAsync(ft => ft.Description.ToLower() == dto.Description.ToLower()).ConfigureAwait(false);
        if (existing != null)
        {
            return existing;
        }

        var created = dto.ToEntity();
        await _db.FilamentTypes.AddAsync(created).ConfigureAwait(false);
        return created;
    }
    #endregion
}