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

    // DTO-based overload with validation
    public Task AddFilamentAsync(FilamentDto filamentDto)
    {
        ValidateFilamentDto(filamentDto);
        return AddFilamentAsync(filamentDto.ToEntity());
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

    // DTO-based overload with validation
    public Task AddCustomerAsync(CustomerDto customerDto)
    {
        ValidateCustomerDto(customerDto);
        return AddCustomerAsync(customerDto.ToEntity());
    }

    /// <inheritdoc />
    public async Task AddFilamentColourAsync(FilamentColour filamentColour)
    {
        ArgumentNullException.ThrowIfNull(filamentColour);
        await _db.FilamentColours.AddAsync(filamentColour).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    // DTO-based overload with validation
    public Task AddFilamentColourAsync(FilamentColourDto filamentColourDto)
    {
        ValidateFilamentColourDto(filamentColourDto);
        return AddFilamentColourAsync(filamentColourDto.ToEntity());
    }

    /// <inheritdoc />
    public async Task AddFilamentTypeAsync(FilamentType filamentType)
    {
        ArgumentNullException.ThrowIfNull(filamentType);
        await _db.FilamentTypes.AddAsync(filamentType).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    // DTO-based overload with validation
    public Task AddFilamentTypeAsync(FilamentTypeDto filamentTypeDto)
    {
        ValidateFilamentTypeDto(filamentTypeDto);
        return AddFilamentTypeAsync(filamentTypeDto.ToEntity());
    }

    /// <inheritdoc />
    public async Task AddModelDesignAsync(ModelDesign modelDesign)
    {
        ArgumentNullException.ThrowIfNull(modelDesign);
        await _db.ModelDesigns.AddAsync(modelDesign).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    // DTO-based overload with validation
    public Task AddModelDesignAsync(ModelDesignDto modelDesignDto)
    {
        ValidateModelDesignDto(modelDesignDto);
        return AddModelDesignAsync(modelDesignDto.ToEntity());
    }

    /// <inheritdoc />
    public async Task AddPrintingProjectAsync(PrintingProject project)
    {
        ArgumentNullException.ThrowIfNull(project);
        await _db.PrintingProjects.AddAsync(project).ConfigureAwait(false);
        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    // DTO-based overload with validation
    public Task AddPrintingProjectAsync(PrintingProjectDto projectDto)
    {
        ValidatePrintingProjectDto(projectDto);
        return AddPrintingProjectAsync(projectDto.ToEntity());
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
}