namespace gCodeJournal.ViewModel;

#region Using Directives
using Microsoft.EntityFrameworkCore;
using Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs;
using System.Linq;
using Mapping;
using System;
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
    public async Task AddFilamentAsync(Filament filament)
    {
        ArgumentNullException.ThrowIfNull(filament);
        await _db.Filaments.AddAsync(filament).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    // DTO-based overload with lookups to avoid duplicates
    public async Task AddFilamentAsync(FilamentDto filamentDto)
    {
        ValidateFilamentDto(filamentDto);

        // Build filament entity and attach existing related entities if present
        var filament = new Filament
        {
            CostPerWeight = filamentDto.CostPerWeight,
            ProductId = filamentDto.ProductId,
            ReorderLink = filamentDto.ReorderLink
        };

        // Manufacturer
        if (filamentDto.Manufacturer != null)
        {
            var manufacturer = await GetOrCreateManufacturerAsync(filamentDto.Manufacturer).ConfigureAwait(false);
            filament.ManufacturerId = manufacturer.Id;
        }

        // Colour
        if (filamentDto.FilamentColour != null)
        {
            var colour = await GetOrCreateFilamentColourAsync(filamentDto.FilamentColour).ConfigureAwait(false);
            filament.FilamentColourId = colour.Id;
        }

        // Type
        if (filamentDto.FilamentType != null)
        {
            var type = await GetOrCreateFilamentTypeAsync(filamentDto.FilamentType).ConfigureAwait(false);
            filament.FilamentTypeId = type.Id;
        }

        await _db.Filaments.AddAsync(filament).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<List<CustomerDto>> GetAllCustomersAsync()
    {
        return _db.Customers
            .OrderBy(c => c.Name)
            .Select(c => new CustomerDto(c.Id, c.Name))
            .ToListAsync();
    }

    /// <inheritdoc />
    public Task<List<FilamentDto>> GetAllFilamentsAsync()
    {
        return _db.Filaments
            .Include(f => f.Colour)
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
                new ManufacturerDto(f.Manufacturer.Id, f.Manufacturer.Name)
            ))
            .ToListAsync();
    }

    /// <inheritdoc />
    public Task<List<ManufacturerDto>> GetAllManufacturersAsync()
    {
        return _db.Manufacturers
            .OrderBy(m => m.Name)
            .Select(m => new ManufacturerDto(m.Id, m.Name))
            .ToListAsync();
    }

    /// <inheritdoc />
    public Task<List<FilamentColourDto>> GetAllFilamentColoursAsync()
    {
        return _db.FilamentColours
            .OrderBy(fc => fc.Description)
            .Select(fc => new FilamentColourDto(fc.Id, fc.Description))
            .ToListAsync();
    }

    /// <inheritdoc />
    public Task<List<FilamentTypeDto>> GetAllFilamentTypesAsync()
    {
        return _db.FilamentTypes
            .OrderBy(ft => ft.Description)
            .Select(ft => new FilamentTypeDto(ft.Id, ft.Description))
            .ToListAsync();
    }

    /// <inheritdoc />
    public Task<List<ModelDesignDto>> GetAllModelDesignsAsync()
    {
        return _db.ModelDesigns
            .OrderBy(md => md.Description)
            .Select(md => new ModelDesignDto(md.Id, md.Description, md.Length, md.Summary, md.Url))
            .ToListAsync();
    }

    /// <inheritdoc />
    public Task<List<PrintingProjectDto>> GetAllPrintingProjectsAsync()
    {
        return _db.PrintingProjects
            .Include(p => p.Customer)
            .Include(p => p.Model)
            .Include(p => p.Filaments).ThenInclude(f => f.Manufacturer)
            .Include(p => p.Filaments).ThenInclude(f => f.Colour)
            .Include(p => p.Filaments).ThenInclude(f => f.Type)
            .Select(p => new PrintingProjectDto(
                p.Id,
                p.Cost,
                p.Submitted,
                p.Completed,
                p.Customer == null ? null : new CustomerDto(p.Customer.Id, p.Customer.Name),
                p.Model == null ? null : new ModelDesignDto(p.Model.Id, p.Model.Description, p.Model.Length, p.Model.Summary, p.Model.Url),
                p.Filaments.Select(f => new FilamentDto(
                    f.Id,
                    f.CostPerWeight,
                    f.ProductId,
                    f.ReorderLink,
                    new FilamentColourDto(f.Colour.Id, f.Colour.Description),
                    new FilamentTypeDto(f.Type.Id, f.Type.Description),
                    new ManufacturerDto(f.Manufacturer.Id, f.Manufacturer.Name)
                )).ToList()
            ))
            .ToListAsync();
    }

    // keep some legacy add methods on the ViewModel — they remain entity-based
    /// <inheritdoc />
    public async Task AddCustomerAsync(Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);
        await _db.Customers.AddAsync(customer).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    // DTO-based overload with lookup
    public async Task AddCustomerAsync(CustomerDto customerDto)
    {
        ValidateCustomerDto(customerDto);

        Customer? existing = null;
        if (customerDto.Id !=0)
            existing = await _db.Customers.FindAsync(customerDto.Id).ConfigureAwait(false);
        if (existing == null)
            existing = await _db.Customers.FirstOrDefaultAsync(c => c.Name == customerDto.Name).ConfigureAwait(false);
        if (existing != null)
            return; // already exists

        await _db.Customers.AddAsync(customerDto.ToEntity()).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task AddFilamentColourAsync(FilamentColour filamentColour)
    {
        ArgumentNullException.ThrowIfNull(filamentColour);
        await _db.FilamentColours.AddAsync(filamentColour).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    // DTO-based overload with lookup
    public async Task AddFilamentColourAsync(FilamentColourDto filamentColourDto)
    {
        ValidateFilamentColourDto(filamentColourDto);

        FilamentColour? existing = null;
        if (filamentColourDto.Id !=0)
            existing = await _db.FilamentColours.FindAsync(filamentColourDto.Id).ConfigureAwait(false);
        if (existing == null)
            existing = await _db.FilamentColours.FirstOrDefaultAsync(fc => fc.Description == filamentColourDto.Description).ConfigureAwait(false);
        if (existing != null)
            return;

        await _db.FilamentColours.AddAsync(filamentColourDto.ToEntity()).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task AddFilamentTypeAsync(FilamentType filamentType)
    {
        ArgumentNullException.ThrowIfNull(filamentType);
        await _db.FilamentTypes.AddAsync(filamentType).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    // DTO-based overload with lookup
    public async Task AddFilamentTypeAsync(FilamentTypeDto filamentTypeDto)
    {
        ValidateFilamentTypeDto(filamentTypeDto);

        FilamentType? existing = null;
        if (filamentTypeDto.Id !=0)
            existing = await _db.FilamentTypes.FindAsync(filamentTypeDto.Id).ConfigureAwait(false);
        if (existing == null)
            existing = await _db.FilamentTypes.FirstOrDefaultAsync(ft => ft.Description == filamentTypeDto.Description).ConfigureAwait(false);
        if (existing != null)
            return;

        await _db.FilamentTypes.AddAsync(filamentTypeDto.ToEntity()).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task AddModelDesignAsync(ModelDesign modelDesign)
    {
        ArgumentNullException.ThrowIfNull(modelDesign);
        await _db.ModelDesigns.AddAsync(modelDesign).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    // DTO-based overload with lookup
    public async Task AddModelDesignAsync(ModelDesignDto modelDesignDto)
    {
        ValidateModelDesignDto(modelDesignDto);

        ModelDesign? existing = null;
        if (modelDesignDto.Id !=0)
            existing = await _db.ModelDesigns.FindAsync(modelDesignDto.Id).ConfigureAwait(false);
        if (existing == null)
            existing = await _db.ModelDesigns.FirstOrDefaultAsync(md => md.Description == modelDesignDto.Description).ConfigureAwait(false);
        if (existing != null)
            return;

        await _db.ModelDesigns.AddAsync(modelDesignDto.ToEntity()).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task AddPrintingProjectAsync(PrintingProject project)
    {
        ArgumentNullException.ThrowIfNull(project);
        await _db.PrintingProjects.AddAsync(project).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    // DTO-based overload with lookups and related entity attachment
    public async Task AddPrintingProjectAsync(PrintingProjectDto projectDto)
    {
        ValidatePrintingProjectDto(projectDto);

        // Resolve or create Customer
        Customer? customer = null;
        if (projectDto.Customer != null)
        {
            if (projectDto.Customer.Id !=0)
                customer = await _db.Customers.FindAsync(projectDto.Customer.Id).ConfigureAwait(false);
            if (customer == null)
                customer = await _db.Customers.FirstOrDefaultAsync(c => c.Name == projectDto.Customer.Name).ConfigureAwait(false);
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
            if (projectDto.ModelDesign.Id !=0)
                model = await _db.ModelDesigns.FindAsync(projectDto.ModelDesign.Id).ConfigureAwait(false);
            if (model == null)
                model = await _db.ModelDesigns.FirstOrDefaultAsync(md => md.Description == projectDto.ModelDesign.Description).ConfigureAwait(false);
            if (model == null)
            {
                model = projectDto.ModelDesign.ToEntity();
                await _db.ModelDesigns.AddAsync(model).ConfigureAwait(false);
            }
        }

        // Create project entity and attach resolved relations
        var project = new PrintingProject
        {
            Cost = projectDto.Cost,
            Submitted = projectDto.Submitted,
            Completed = projectDto.Completed,
            Customer = customer,
            Model = model,
            Filaments = new List<Filament>()
        };

        // Resolve or create filaments
        foreach (var fDto in projectDto.Filaments)
        {
            Filament? fEntity = null;
            if (fDto.Id !=0)
            {
                fEntity = await _db.Filaments.FindAsync(fDto.Id).ConfigureAwait(false);
            }

            if (fEntity == null)
            {
                // create filament entity, but attach related lookups
                fEntity = new Filament
                {
                    CostPerWeight = fDto.CostPerWeight,
                    ProductId = fDto.ProductId,
                    ReorderLink = fDto.ReorderLink
                };

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

                // add to context but don't save yet
                await _db.Filaments.AddAsync(fEntity).ConfigureAwait(false);
            }

            project.Filaments.Add(fEntity);
        }

        await _db.PrintingProjects.AddAsync(project).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }
    #endregion

    #region Validation helpers
    private static void ValidateCustomerDto(CustomerDto dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Customer name is required", nameof(dto));
    }

    private static void ValidateFilamentColourDto(FilamentColourDto dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        if (string.IsNullOrWhiteSpace(dto.Description)) throw new ArgumentException("Filament colour description is required", nameof(dto));
    }

    private static void ValidateFilamentTypeDto(FilamentTypeDto dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        if (string.IsNullOrWhiteSpace(dto.Description)) throw new ArgumentException("Filament type description is required", nameof(dto));
    }

    private static void ValidateModelDesignDto(ModelDesignDto dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        if (string.IsNullOrWhiteSpace(dto.Description)) throw new ArgumentException("ModelDesign description is required", nameof(dto));
        if (dto.Length <0) throw new ArgumentException("ModelDesign length must be non-negative", nameof(dto));
    }

    private static void ValidateFilamentDto(FilamentDto dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        if (dto.CostPerWeight <0) throw new ArgumentException("Filament cost must be non-negative", nameof(dto));
        if (dto.Manufacturer is null) throw new ArgumentException("Filament manufacturer is required", nameof(dto));
        if (dto.FilamentColour is null) throw new ArgumentException("Filament colour is required", nameof(dto));
        if (dto.FilamentType is null) throw new ArgumentException("Filament type is required", nameof(dto));
    }

    private static void ValidatePrintingProjectDto(PrintingProjectDto dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        if (dto.Cost <0) throw new ArgumentException("Printing project cost must be non-negative", nameof(dto));
        if (dto.Customer is null) throw new ArgumentException("Printing project must have a customer", nameof(dto));
        if (dto.ModelDesign is null) throw new ArgumentException("Printing project must have a model design", nameof(dto));
    }
    #endregion

    #region Helper lookups
    private async Task<Manufacturer> GetOrCreateManufacturerAsync(ManufacturerDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        Manufacturer? existing = null;
        if (dto.Id !=0)
            existing = await _db.Manufacturers.FindAsync(dto.Id).ConfigureAwait(false);
        if (existing == null)
            existing = await _db.Manufacturers.FirstOrDefaultAsync(m => m.Name == dto.Name).ConfigureAwait(false);
        if (existing != null) return existing;

        var created = dto.ToEntity();
        await _db.Manufacturers.AddAsync(created).ConfigureAwait(false);
        // Note: do not SaveChanges here; caller will save once after composing related entities
        return created;
    }

    private async Task<FilamentColour> GetOrCreateFilamentColourAsync(FilamentColourDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        FilamentColour? existing = null;
        if (dto.Id !=0)
            existing = await _db.FilamentColours.FindAsync(dto.Id).ConfigureAwait(false);
        if (existing == null)
            existing = await _db.FilamentColours.FirstOrDefaultAsync(fc => fc.Description == dto.Description).ConfigureAwait(false);
        if (existing != null) return existing;

        var created = dto.ToEntity();
        await _db.FilamentColours.AddAsync(created).ConfigureAwait(false);
        return created;
    }

    private async Task<FilamentType> GetOrCreateFilamentTypeAsync(FilamentTypeDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        FilamentType? existing = null;
        if (dto.Id !=0)
            existing = await _db.FilamentTypes.FindAsync(dto.Id).ConfigureAwait(false);
        if (existing == null)
            existing = await _db.FilamentTypes.FirstOrDefaultAsync(ft => ft.Description == dto.Description).ConfigureAwait(false);
        if (existing != null) return existing;

        var created = dto.ToEntity();
        await _db.FilamentTypes.AddAsync(created).ConfigureAwait(false);
        return created;
    }
    #endregion
}