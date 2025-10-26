namespace gCodeJournal.ViewModel;

#region Using Directives
using Microsoft.EntityFrameworkCore;
using Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs;
using System.Linq;
using Mapping;
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

    // DTO-based overload
    public Task AddFilamentAsync(FilamentDto filamentDto) => AddFilamentAsync(filamentDto.ToEntity());

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

    // DTO-based overload
    public Task AddCustomerAsync(CustomerDto customerDto) => AddCustomerAsync(customerDto.ToEntity());

    /// <inheritdoc />
    public async Task AddFilamentColourAsync(FilamentColour filamentColour)
    {
        ArgumentNullException.ThrowIfNull(filamentColour);
        await _db.FilamentColours.AddAsync(filamentColour).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    // DTO-based overload
    public Task AddFilamentColourAsync(FilamentColourDto filamentColourDto) => AddFilamentColourAsync(filamentColourDto.ToEntity());

    /// <inheritdoc />
    public async Task AddFilamentTypeAsync(FilamentType filamentType)
    {
        ArgumentNullException.ThrowIfNull(filamentType);
        await _db.FilamentTypes.AddAsync(filamentType).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    // DTO-based overload
    public Task AddFilamentTypeAsync(FilamentTypeDto filamentTypeDto) => AddFilamentTypeAsync(filamentTypeDto.ToEntity());

    /// <inheritdoc />
    public async Task AddModelDesignAsync(ModelDesign modelDesign)
    {
        ArgumentNullException.ThrowIfNull(modelDesign);
        await _db.ModelDesigns.AddAsync(modelDesign).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    // DTO-based overload
    public Task AddModelDesignAsync(ModelDesignDto modelDesignDto) => AddModelDesignAsync(modelDesignDto.ToEntity());

    /// <inheritdoc />
    public async Task AddPrintingProjectAsync(PrintingProject project)
    {
        ArgumentNullException.ThrowIfNull(project);
        await _db.PrintingProjects.AddAsync(project).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    // DTO-based overload
    public Task AddPrintingProjectAsync(PrintingProjectDto projectDto) => AddPrintingProjectAsync(projectDto.ToEntity());
    #endregion
}