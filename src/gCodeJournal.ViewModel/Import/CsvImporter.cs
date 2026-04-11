// gCodeJournal.ViewModel

#pragma warning disable CA2208
#pragma warning disable CA1860
namespace gCodeJournal.ViewModel.Import;

#region Using Directives
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using DTOs;
using Maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Model;
#endregion

public class CsvImporter(GCodeJournalDbContext db, GCodeJournalViewModel vm)
{
    #region ImportEntity Enum
    enum ImportEntity
    {
        Unknown,
        Customers,
        Manufacturers,
        FilamentColours,
        FilamentTypes,
        Filaments,
        Printers,
        ModelDesigns,
        PrintingProjects
    }
    #endregion

    #region Nested type: ImportFileResult
    public record ImportFileResult(string FileName, ImportResult Result);
    #endregion

    public async Task<List<ImportFileResult>> ImportFromPathAsync(string path, ILogger appLogger, bool updateExisting, char delimiter, CancellationToken ct)
    {
        var results = new List<ImportFileResult>();
        if (File.Exists(path))
        {
            // single file - delegate to stream overload
            await using var fs      = File.OpenRead(path);
            var             fileRes = await ImportStreamAsync(fs, appLogger, Path.GetFileName(path), updateExisting, delimiter, ct).ConfigureAwait(false);
            results.Add(fileRes);

            return results;
        }

        if (Directory.Exists(path))
        {
            // process known file names in directory in dependency order
            var ordered =
                new[]
                {
                    ImportEntity.Customers,
                    ImportEntity.Manufacturers,
                    ImportEntity.FilamentColours,
                    ImportEntity.FilamentTypes,
                    ImportEntity.Filaments,
                    ImportEntity.ModelDesigns,
                    ImportEntity.Printers,
                    ImportEntity.PrintingProjects
                };

            // mergedMap carries mappings between files so later files can resolve references
            var mergedMap = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in ordered.Select(EntityFileName).Select(fileName => Path.Combine(path, fileName)).Where(File.Exists))
            {
                await using var fs = File.OpenRead(file);
                var r = await ImportStreamAsync(fs, appLogger, Path.GetFileName(file), updateExisting, delimiter, ct, mergedMap).ConfigureAwait(false);

                // add unmerged per-file result
                results.Add(r);

                // merge id maps into mergedMap for later file resolution
                foreach (var kv in r.Result.IdMap)
                {
                    if (!mergedMap.TryGetValue(kv.Key, out var inner))
                    {
                        inner             = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                        mergedMap[kv.Key] = inner;
                    }

                    foreach (var entry in kv.Value)
                    {
                        inner[entry.Key] = entry.Value;
                    }
                }
            }

            return results;
        }

        var notFound = new ImportResult();
        notFound.Errors.Add($"Path '{path}' does not exist");
        notFound.Failed++;
        results.Add(new ImportFileResult(string.Empty, notFound));

        return results;
    }

    public async Task<ImportFileResult> ImportStreamAsync(
        Stream                                       stream,
        ILogger                                      appLogger,
        string?                                      fileName,
        bool                                         updateExisting,
        char                                         delimiter,
        CancellationToken                            ct,
        Dictionary<string, Dictionary<string, int>>? existingMappings = null)
    {
        var       result = new ImportResult();
        using var sr     = new StreamReader(stream, Encoding.UTF8, true, 8192, true);

        // Configure header normalization so headers like 'ID' match 'Id'/'id'
        var csvConfig =
            new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiter.ToString(), PrepareHeaderForMatch = args => args.Header?.Trim().ToLowerInvariant()
            };

        using var csv = new CsvReader(sr, csvConfig);

        // Register CSV class maps for DTOs that require custom mapping
        csv.Context.RegisterClassMap<CustomerMap>();
        csv.Context.RegisterClassMap<FilamentMap>();
        csv.Context.RegisterClassMap<FilamentColourMap>();
        csv.Context.RegisterClassMap<FilamentTypeMap>();
        csv.Context.RegisterClassMap<ManufacturerMap>();
        csv.Context.RegisterClassMap<PrinterMap>();
        csv.Context.RegisterClassMap<ModelDesignMap>();
        csv.Context.RegisterClassMap<PrintingProjectMap>();

        // Try to detect entity from filename
        var entity = DetectEntityFromFileName(fileName);

        await using var tx = await db.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
        try
        {
            if (entity == ImportEntity.Unknown)
            {
                appLogger.LogWarning("Unknown entity detected: {FileName}", fileName);
            }
            else
            {
                // read strongly-typed per-entity import
                switch (entity)
                {
                    case ImportEntity.Customers:
                        var customers = csv.GetRecords<CustomerDto>().ToList();
                        foreach (var c in customers)
                        {
                            appLogger.LogDebug("Processing Customer DTO: {@Dto}", c);
                            var sourceId = c.Id != 0 ? c.Id.ToString() : null;
                            var r        = await vm.AddCustomerAsync(c).ConfigureAwait(false);
                            switch (r)
                            {
                                case (var v, AddRecordResult.Added) when v == ValidationResult.Success:
                                    // added
                                    result.Created++;

                                    break;

                                case (var v, AddRecordResult.Exists) when v == ValidationResult.Success:
                                    // Exists - check that properties match
                                    // TODO: Match by name or use existing record returned by AddCustomerAsync when record exists to avoid extra DB call here. This requires changing the AddCustomerAsync API to return the existing record DTO when record exists, or at least its properties for comparison.
                                    var customer = await vm.GetCustomerAsync(c.Id).ConfigureAwait(false);
                                    if (customer is null)
                                    {
                                        result.Errors.Add($"Customer with ID {c.Id} not found");
                                        result.Failed++;

                                        break;
                                    }

                                    if (customer.Name.Equals(c.Name, StringComparison.OrdinalIgnoreCase))
                                    {
                                        result.Skipped++;

                                        break;
                                    }

                                    // Modify existing customer
                                    appLogger.LogDebug("Modifying existing customer {@Customer}; updating to {@NewName}", customer, c.Name);
                                    customer.Name = c.Name;
                                    await vm.EditCustomerAsync(customer).ConfigureAwait(false);
                                    result.Updated++;

                                    break;

                                case var (v, _) when v != ValidationResult.Success:
                                    // validation error
                                    result.Errors.Add(v.ErrorMessage ?? $"{nameof(vm.AddCustomerAsync)} failed");

                                    break;
                            }
                        }

                        break;

                    case ImportEntity.Manufacturers:
                        var manufacturers = csv.GetRecords<ManufacturerDto>().ToList();
                        foreach (var m in manufacturers)
                        {
                            appLogger.LogDebug("Processing Manufacturer DTO: {@Dto}", m);
                            var sourceId = m.Id != 0 ? m.Id.ToString() : null;
                            var r        = await vm.AddManufacturerAsync(m).ConfigureAwait(false);
                            switch (r)
                            {
                                case (var v, AddRecordResult.Added) when v == ValidationResult.Success:
                                    // added
                                    result.Created++;

                                    break;

                                case (var v, AddRecordResult.Exists) when v == ValidationResult.Success:
                                    // Exists - check that properties match
                                    var manufacturer = await vm.GetManufacturerAsync(m.Id).ConfigureAwait(false);
                                    if (manufacturer is null)
                                    {
                                        result.Errors.Add($"Manufacturer with ID {m.Id} not found");
                                        result.Failed++;

                                        break;
                                    }

                                    // If name and flags all match, skip. Otherwise update the existing record.
                                    if (manufacturer.Name.Equals(m.Name, StringComparison.OrdinalIgnoreCase)
                                        && manufacturer.IsFilamentManufacturer == m.IsFilamentManufacturer
                                        && manufacturer.IsPrinterManufacturer  == m.IsPrinterManufacturer)
                                    {
                                        result.Skipped++;

                                        break;
                                    }

                                    // Modify existing manufacturer properties via ViewModel API (DTO-based)
                                    appLogger.LogDebug("Modifying existing manufacturer {@Manufacturer}; updating to {@NewValues}", manufacturer, m);
                                    var editDto = new ManufacturerDto(manufacturer.Id, m.Name, m.IsFilamentManufacturer, m.IsPrinterManufacturer);
                                    await vm.EditManufacturerAsync(editDto).ConfigureAwait(false);
                                    result.Updated++;

                                    break;

                                case var (v, _) when v != ValidationResult.Success:
                                    // validation error
                                    result.Errors.Add(v.ErrorMessage ?? $"{nameof(vm.AddManufacturerAsync)} failed");

                                    break;
                            }
                        }

                        break;

                    case ImportEntity.FilamentColours:
                        var cols = csv.GetRecords<FilamentColourDto>().ToList();
                        foreach (var c in cols)
                        {
                            appLogger.LogDebug("Processing FilamentColour DTO: {@Dto}", c);

                            // Prefer matching by Description (case-insensitive) to avoid creating duplicates when CSV
                            // contains Description only (no Id) or when Description matches an existing record.
                            var desc = c.Description?.Trim();
                            if (!string.IsNullOrWhiteSpace(desc))
                            {
                                var existing =
                                    await db
                                          .FilamentColours.FirstOrDefaultAsync(x => EF.Functions.Collate(x.Description, "NOCASE") == desc, ct)
                                          .ConfigureAwait(false);

                                if (existing is not null)
                                {
                                    // Found existing by description - prefer this match.
                                    if (c.Id != 0)
                                    {
                                        result.RecordMapping("filament_colours", c.Id.ToString(), existing.Id);
                                    }

                                    result.Skipped++;

                                    continue;
                                }
                            }

                            var sourceId = c.Id != 0 ? c.Id.ToString() : null;
                            var r        = await vm.AddFilamentColourAsync(c).ConfigureAwait(false);
                            switch (r)
                            {
                                case (var v, AddRecordResult.Added) when v == ValidationResult.Success:
                                    // added
                                    result.Created++;

                                    break;

                                case (var v, AddRecordResult.Exists) when v == ValidationResult.Success:
                                    // Exists - check that properties match
                                    var filamentColour = await vm.GetFilamentColourAsync(c.Id).ConfigureAwait(false);
                                    if (filamentColour is null)
                                    {
                                        result.Errors.Add($"Filament colour with ID {c.Id} not found");
                                        result.Failed++;

                                        break;
                                    }

                                    if (filamentColour.Description.Equals(c.Description, StringComparison.OrdinalIgnoreCase))
                                    {
                                        result.Skipped++;

                                        break;
                                    }

                                    // Modify existing record
                                    appLogger.LogDebug(
                                        "Modifying existing filament colour {@FilamentColour}; updating to {@NewDescription}",
                                        filamentColour,
                                        c.Description);
                                    filamentColour.Description = c.Description;
                                    await vm.EditFilamentColourAsync(filamentColour).ConfigureAwait(false);
                                    result.Updated++;

                                    break;

                                case var (v, _) when v != ValidationResult.Success:
                                    // validation error
                                    result.Errors.Add(v.ErrorMessage ?? $"{nameof(vm.AddFilamentColourAsync)} failed");

                                    break;
                            }
                        }

                        break;

                    case ImportEntity.FilamentTypes:
                        var types = csv.GetRecords<FilamentTypeDto>().ToList();
                        foreach (var t in types)
                        {
                            appLogger.LogDebug("Processing FilamentType DTO: {@Dto}", t);

                            // Prefer matching by Description (case-insensitive). This lets CSVs containing
                            // Description only (no Id) correctly resolve to existing records.
                            var desc = t.Description?.Trim();
                            if (!string.IsNullOrWhiteSpace(desc))
                            {
                                var existing =
                                    await db
                                          .FilamentTypes.FirstOrDefaultAsync(x => EF.Functions.Collate(x.Description, "NOCASE") == desc, ct)
                                          .ConfigureAwait(false);

                                if (existing is not null)
                                {
                                    if (t.Id != 0)
                                    {
                                        result.RecordMapping("filament_types", t.Id.ToString(), existing.Id);
                                    }

                                    result.Skipped++;

                                    continue;
                                }
                            }

                            var sourceId = t.Id != 0 ? t.Id.ToString() : null;
                            var r        = await vm.AddFilamentTypeAsync(t).ConfigureAwait(false);
                            switch (r)
                            {
                                case (var v, AddRecordResult.Added) when v == ValidationResult.Success:
                                    // added
                                    result.Created++;

                                    break;

                                case (var v, AddRecordResult.Exists) when v == ValidationResult.Success:
                                    // Exists - check that properties match
                                    var filamentType = await vm.GetFilamentTypeAsync(t.Id).ConfigureAwait(false);
                                    if (filamentType is null)
                                    {
                                        result.Errors.Add($"Filament type with ID {t.Id} not found");
                                        result.Failed++;

                                        break;
                                    }

                                    if (filamentType.Description.Equals(t.Description, StringComparison.OrdinalIgnoreCase))
                                    {
                                        result.Skipped++;

                                        break;
                                    }

                                    // Modify existing filament type
                                    appLogger.LogDebug(
                                        "Modifying existing filament type {@FilamentType}; updating to {@NewDescription}",
                                        filamentType,
                                        t.Description);
                                    filamentType.Description = t.Description;
                                    await vm.EditFilamentTypeAsync(filamentType).ConfigureAwait(false);
                                    result.Updated++;

                                    break;

                                case var (v, _) when v != ValidationResult.Success:
                                    // validation error
                                    result.Errors.Add(v.ErrorMessage ?? $"{nameof(vm.AddFilamentTypeAsync)} failed");

                                    break;
                            }
                        }

                        break;

                    case ImportEntity.Filaments:
                        var filamentRecords = csv.GetRecords<FilamentDto>().ToList();
                        foreach (var f in filamentRecords)
                        {
                            appLogger.LogDebug("Processing Filament DTO: {@DtoId} ({@DtoProductId})", f.Id, f.ProductId);

                            // If related lookups are provided as names (Id == 0), ensure they exist by creating or resolving them
                            if (f.Manufacturer != null && f.Manufacturer.Id == 0 && !string.IsNullOrWhiteSpace(f.Manufacturer.Name))
                            {
                                await vm.AddManufacturerAsync(new ManufacturerDto(0, f.Manufacturer.Name)).ConfigureAwait(false);
                                var all     = await vm.GetAllManufacturersAsync().ConfigureAwait(false);
                                var matched = all.FirstOrDefault(m => string.Equals(m.Name, f.Manufacturer.Name, StringComparison.OrdinalIgnoreCase));
                                if (matched != null)
                                {
                                    f.Manufacturer = new ManufacturerDto(matched.Id, matched.Name);
                                }
                            }

                            if (f.FilamentType != null && f.FilamentType.Id == 0 && !string.IsNullOrWhiteSpace(f.FilamentType.Description))
                            {
                                await vm.AddFilamentTypeAsync(new FilamentTypeDto(0, f.FilamentType.Description)).ConfigureAwait(false);
                                var all = await vm.GetAllFilamentTypesAsync().ConfigureAwait(false);
                                var matched =
                                    all.FirstOrDefault(t => string.Equals(t.Description, f.FilamentType.Description, StringComparison.OrdinalIgnoreCase));
                                if (matched != null)
                                {
                                    f.FilamentType = new FilamentTypeDto(matched.Id, matched.Description);
                                }
                            }

                            if (f.FilamentColour != null && f.FilamentColour.Id == 0 && !string.IsNullOrWhiteSpace(f.FilamentColour.Description))
                            {
                                await vm.AddFilamentColourAsync(new FilamentColourDto(0, f.FilamentColour.Description)).ConfigureAwait(false);
                                var all = await vm.GetAllFilamentColoursAsync().ConfigureAwait(false);
                                var matched =
                                    all.FirstOrDefault(c => string.Equals(c.Description, f.FilamentColour.Description, StringComparison.OrdinalIgnoreCase));
                                if (matched != null)
                                {
                                    f.FilamentColour = new FilamentColourDto(matched.Id, matched.Description);
                                }
                            }

                            var r = await vm.AddFilamentAsync(f).ConfigureAwait(false);
                            switch (r)
                            {
                                case (var v, AddRecordResult.Added) when v == ValidationResult.Success:
                                    // added
                                    result.Created++;

                                    break;

                                case (var v, AddRecordResult.Exists) when v == ValidationResult.Success:
                                    // Exists - check that properties match
                                    var filamentDto = await vm.GetFilamentAsync(f.Id).ConfigureAwait(false);
                                    if (filamentDto is null)
                                    {
                                        result.Errors.Add($"Filament with ID {f.Id} not found");
                                        result.Failed++;

                                        break;
                                    }

                                    var comparisonResult = filamentDto.CompareTo(f);
                                    if (comparisonResult.IsMatch)
                                    {
                                        result.Skipped++;

                                        break;
                                    }

                                    // Modify existing filament
                                    appLogger.LogDebug("Modifying existing filament {@Filament}", filamentDto);
                                    appLogger.LogDebug("Properties to be updated: {@Properties}", string.Join(',', comparisonResult.MismatchedProperties));
                                    filamentDto.CostPerWeight = f.CostPerWeight;
                                    filamentDto.ProductId     = f.ProductId;
                                    filamentDto.ReorderLink   = f.ReorderLink;

                                    // Get DTOs for related entities
                                    var filamentColour = await vm.GetFilamentColourAsync(f.FilamentColour.Id).ConfigureAwait(false);
                                    if (filamentColour is null)
                                    {
                                        result.Errors.Add($"FilamentColour with ID {f.FilamentColour.Id} not found");
                                        result.Failed++;

                                        break;
                                    }

                                    filamentDto.FilamentColour = filamentColour;
                                    var filamentType = await vm.GetFilamentTypeAsync(f.FilamentType.Id).ConfigureAwait(false);
                                    if (filamentType is null)
                                    {
                                        result.Errors.Add($"FilamentType with ID {f.FilamentType.Id} not found");
                                        result.Failed++;

                                        break;
                                    }

                                    filamentDto.FilamentType = filamentType;
                                    var manufacturer = await vm.GetManufacturerAsync(f.Manufacturer.Id).ConfigureAwait(false);
                                    if (manufacturer is null)
                                    {
                                        result.Errors.Add($"Manufacturer with ID {f.Manufacturer.Id} not found");
                                        result.Failed++;

                                        break;
                                    }

                                    filamentDto.Manufacturer = manufacturer;
                                    await vm.EditFilamentAsync(filamentDto).ConfigureAwait(false);
                                    result.Updated++;

                                    break;

                                case var (v, _) when v != ValidationResult.Success:
                                    // validation error
                                    result.Errors.Add(v.ErrorMessage ?? $"{nameof(vm.AddFilamentTypeAsync)} failed");

                                    break;
                            }
                        }

                        break;

                    case ImportEntity.ModelDesigns:
                        var models = csv.GetRecords<ModelDesignDto>().ToList();
                        foreach (var m in models)
                        {
                            appLogger.LogDebug("Processing ModelDesign DTO: {@Dto}", m);
                            var sourceId = m.Id != 0 ? m.Id.ToString() : null;
                            var r        = await vm.AddModelDesignAsync(m).ConfigureAwait(false);
                            switch (r)
                            {
                                case (var v, AddRecordResult.Added) when v == ValidationResult.Success:
                                    // added
                                    result.Created++;

                                    break;

                                case (var v, AddRecordResult.Exists) when v == ValidationResult.Success:
                                    // Exists - check that properties match
                                    var modelDto = await vm.GetModelDesignAsync(m.Id).ConfigureAwait(false);
                                    if (modelDto is null)
                                    {
                                        result.Errors.Add($"Model with ID {m.Id} not found");
                                        result.Failed++;

                                        break;
                                    }

                                    var comparisonResult = modelDto.CompareTo(m);
                                    if (comparisonResult.IsMatch)
                                    {
                                        result.Skipped++;

                                        break;
                                    }

                                    // Modify existing model
                                    appLogger.LogDebug("Modifying existing model {@Model}",       modelDto);
                                    appLogger.LogDebug("Properties to be updated: {@Properties}", string.Join(',', comparisonResult.MismatchedProperties));
                                    modelDto.Description = m.Description;
                                    modelDto.Length      = m.Length;
                                    modelDto.Summary     = m.Summary;
                                    modelDto.Url         = m.Url;
                                    await vm.EditModelDesignAsync(modelDto).ConfigureAwait(false);
                                    result.Updated++;

                                    break;

                                case var (v, _) when v != ValidationResult.Success:
                                    // validation error
                                    result.Errors.Add(v.ErrorMessage ?? $"{nameof(vm.AddModelDesignAsync)} failed");

                                    break;
                            }
                        }

                        break;

                    case ImportEntity.Printers:
                        var printers = csv.GetRecords<PrinterDto>().ToList();
                        foreach (var p in printers)
                        {
                            appLogger.LogDebug("Processing Printer DTO: {@Dto}", p);
                            var sourceId = p.Id != 0 ? p.Id.ToString() : null;

                            var r = await vm.AddPrinterAsync(p).ConfigureAwait(false);
                            switch (r)
                            {
                                case (var v, AddRecordResult.Added) when v == ValidationResult.Success:
                                    // added
                                    result.Created++;

                                    break;

                                case (var v, AddRecordResult.Exists) when v == ValidationResult.Success:
                                    // Exists - check that properties match
                                    var printerDto = await vm.GetPrinterAsync(p.Id).ConfigureAwait(false);
                                    if (printerDto is null)
                                    {
                                        result.Errors.Add($"Model with ID {p.Id} not found");
                                        result.Failed++;

                                        break;
                                    }

                                    var comparisonResult = printerDto.CompareTo(p);
                                    if (comparisonResult.IsMatch)
                                    {
                                        result.Skipped++;

                                        break;
                                    }

                                    // Modify existing printer
                                    appLogger.LogDebug("Modifying existing printer {@Printer}",   printerDto);
                                    appLogger.LogDebug("Properties to be updated: {@Properties}", string.Join(',', comparisonResult.MismatchedProperties));
                                    printerDto.Model        = p.Model;
                                    printerDto.Manufacturer = p.Manufacturer;
                                    printerDto.CostPerHour  = p.CostPerHour;
                                    await vm.EditPrinterAsync(printerDto).ConfigureAwait(false);
                                    result.Updated++;

                                    break;

                                case var (v, _) when v != ValidationResult.Success:
                                    // validation error
                                    result.Errors.Add(v.ErrorMessage ?? $"{nameof(vm.AddPrinterAsync)} failed");

                                    break;
                            }
                        }

                        break;

                    case ImportEntity.PrintingProjects:
                        var projects = csv.GetRecords<PrintingProjectDto>().ToList();
                        foreach (var p in projects)
                        {
                            appLogger.LogDebug("Processing PrintingProject DTO: {@Dto}", p);
                            var sourceId = p.Id != 0 ? p.Id.ToString() : null;

                            var r = await vm.AddPrintingProjectAsync(p).ConfigureAwait(false);
                            switch (r)
                            {
                                case (var v, AddRecordResult.Added) when v == ValidationResult.Success:
                                    // added
                                    result.Created++;

                                    break;

                                case (var v, AddRecordResult.Exists) when v == ValidationResult.Success:
                                    // Exists - check that properties match
                                    var printingProjectDto = await vm.GetPrintingProjectAsync(p.Id).ConfigureAwait(false);
                                    if (printingProjectDto is null)
                                    {
                                        result.Errors.Add($"Printing project with ID {p.Id} not found");
                                        result.Failed++;

                                        break;
                                    }

                                    var comparisonResult = printingProjectDto.CompareTo(p);
                                    if (comparisonResult.IsMatch)
                                    {
                                        result.Skipped++;

                                        break;
                                    }

                                    // Modify existing project
                                    appLogger.LogDebug("Modifying existing project {@Project}",   printingProjectDto);
                                    appLogger.LogDebug("Properties to be updated: {@Properties}", string.Join(',', comparisonResult.MismatchedProperties));
                                    printingProjectDto.Submitted = p.Submitted;
                                    printingProjectDto.Completed = p.Completed;
                                    printingProjectDto.Cost      = p.Cost; // To be calculated based on length/mass of filament(s) used

                                    // Validate required related entities
                                    if (p.Customer?.Id is null)
                                    {
                                        result.Errors.Add("No customer ID specified for project");
                                        result.Failed++;

                                        break;
                                    }

                                    if (p.ModelDesign?.Id is null)
                                    {
                                        result.Errors.Add("No model design ID specified for project");
                                        result.Failed++;

                                        break;
                                    }

                                    if (p.Filaments.Count == 0)
                                    {
                                        result.Errors.Add("No filaments specified for project");
                                        result.Failed++;

                                        break;
                                    }

                                    // Get customer DTO
                                    var customer = await vm.GetCustomerAsync(p.Customer.Id).ConfigureAwait(false);
                                    if (customer is null)
                                    {
                                        result.Errors.Add($"Customer with ID {p.Customer.Id} not found");
                                        result.Failed++;

                                        break;
                                    }

                                    printingProjectDto.Customer = customer;

                                    // Get Model Design DTO
                                    ModelDesignDto? modelDto = null;

                                    // If provided as id
                                    if (p.ModelDesign?.Id is not null && p.ModelDesign.Id != 0)
                                    {
                                        modelDto = await vm.GetModelDesignAsync(p.ModelDesign.Id).ConfigureAwait(false);
                                        if (modelDto is null)
                                        {
                                            result.Errors.Add($"Model with ID {p.ModelDesign.Id} not found");
                                            result.Failed++;

                                            break;
                                        }

                                        printingProjectDto.ModelDesign = modelDto;
                                    }
                                    else if (p.ModelDesign != null && !string.IsNullOrWhiteSpace(p.ModelDesign.Summary))
                                    {
                                        // Try to match by Summary
                                        var all = await vm.GetAllModelDesignsAsync().ConfigureAwait(false);
                                        modelDto = all.FirstOrDefault(m => string.Equals(m.Summary, p.ModelDesign.Summary, StringComparison.OrdinalIgnoreCase));
                                        if (modelDto is null)
                                        {
                                            // log a warning and continue (per requirements)
                                            appLogger.LogWarning("Model design not found for summary: {Summary}", p.ModelDesign.Summary);
                                        }
                                        else
                                        {
                                            printingProjectDto.ModelDesign = modelDto;
                                        }
                                    }

                                    // Get Filaments from list (either IDs or sets Manufacturer|Type|Colour)
                                    var filaments    = new List<FilamentDto>();
                                    var allFilaments = await vm.GetAllFilamentsAsync().ConfigureAwait(false);
                                    foreach (var pFilament in p.Filaments)
                                    {
                                        FilamentDto? filament = null;
                                        if (pFilament.Id != 0)
                                        {
                                            filament = await vm.GetFilamentAsync(pFilament.Id).ConfigureAwait(false);
                                            if (filament is null)
                                            {
                                                appLogger.LogWarning("Filament with ID {Id} not found", pFilament.Id);

                                                continue;
                                            }

                                            filaments.Add(filament);

                                            continue;
                                        }

                                        // Match by Manufacturer, Type and Colour if provided as names
                                        var manName  = pFilament.Manufacturer?.Name          ?? string.Empty;
                                        var typeDesc = pFilament.FilamentType?.Description   ?? string.Empty;
                                        var colDesc  = pFilament.FilamentColour?.Description ?? string.Empty;

                                        if (!string.IsNullOrWhiteSpace(manName) && !string.IsNullOrWhiteSpace(typeDesc) && !string.IsNullOrWhiteSpace(colDesc))
                                        {
                                            filament =
                                                allFilaments.FirstOrDefault(f =>
                                                                                string.Equals(f.Manufacturer.Name, manName, StringComparison.OrdinalIgnoreCase)
                                                                                && string.Equals(
                                                                                    f.FilamentType.Description,
                                                                                    typeDesc,
                                                                                    StringComparison.OrdinalIgnoreCase)
                                                                                && string.Equals(
                                                                                    f.FilamentColour.Description,
                                                                                    colDesc,
                                                                                    StringComparison.OrdinalIgnoreCase));

                                            if (filament is null)
                                            {
                                                appLogger.LogWarning(
                                                    "Filament not found for Manufacturer={Manufacturer}, Type={Type}, Colour={Colour}",
                                                    manName,
                                                    typeDesc,
                                                    colDesc);

                                                continue;
                                            }

                                            filaments.Add(filament);

                                            continue;
                                        }

                                        // could not resolve filament
                                        appLogger.LogWarning("Unable to resolve filament entry: {@Entry}", pFilament);
                                    }

                                    p.Filaments = filaments;
                                    if (!p.Filaments.Any())
                                    {
                                        result.Errors.Add("No filaments found for project");
                                        result.Failed++;

                                        break;
                                    }

                                    await vm.EditPrintingProjectAsync(printingProjectDto).ConfigureAwait(false);
                                    result.Updated++;

                                    break;

                                case var (v, _) when v != ValidationResult.Success:
                                    // validation error
                                    result.Errors.Add(v.ErrorMessage ?? $"{nameof(vm.AddFilamentTypeAsync)} failed");

                                    break;
                            }
                        }

                        break;
                    case ImportEntity.Unknown:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(entity));
                }
            }

            await tx.CommitAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct).ConfigureAwait(false);
            result.Errors.Add(ex.Message);
            result.Failed++;
        }

        return new ImportFileResult(fileName ?? string.Empty, result);
    }

    static ImportEntity DetectEntityFromFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return ImportEntity.Unknown;
        }

        var name = fileName.ToLowerInvariant();
        if (name.Contains("customers", StringComparison.OrdinalIgnoreCase))
        {
            return ImportEntity.Customers;
        }

        if (name.Contains("manufacturers", StringComparison.OrdinalIgnoreCase))
        {
            return ImportEntity.Manufacturers;
        }

        if (name.Contains("filament_colours", StringComparison.OrdinalIgnoreCase))
        {
            return ImportEntity.FilamentColours;
        }

        if (name.Contains("filament_types", StringComparison.OrdinalIgnoreCase))
        {
            return ImportEntity.FilamentTypes;
        }

        if (name.Contains("filaments"))
        {
            return ImportEntity.Filaments;
        }

        if (name.Contains("printers", StringComparison.OrdinalIgnoreCase))
        {
            return ImportEntity.Printers;
        }

        if (name.Contains("model_designs", StringComparison.OrdinalIgnoreCase))
        {
            return ImportEntity.ModelDesigns;
        }

        return name.Contains("printing_projects", StringComparison.OrdinalIgnoreCase) ? ImportEntity.PrintingProjects : ImportEntity.Unknown;
    }

    static string EntityFileName(ImportEntity entity) => entity switch
    {
        ImportEntity.Customers        => "customers.csv",
        ImportEntity.Manufacturers    => "manufacturers.csv",
        ImportEntity.FilamentColours  => "filament_colours.csv",
        ImportEntity.FilamentTypes    => "filament_types.csv",
        ImportEntity.Filaments        => "filaments.csv",
        ImportEntity.Printers         => "printers.csv",
        ImportEntity.ModelDesigns     => "model_designs.csv",
        ImportEntity.PrintingProjects => "printing_projects.csv",
        _                             => string.Empty
    };

    static ImportEntity ParseEntityName(string name) =>
        name.ToLowerInvariant() switch
        {
            "customers" or "customer"                               => ImportEntity.Customers,
            "manufacturers" or "manufacturer"                       => ImportEntity.Manufacturers,
            "filament_colours" or "filamentcolours"                 => ImportEntity.FilamentColours,
            "filament_types" or "filamenttypes"                     => ImportEntity.FilamentTypes,
            "filaments" or "filament"                               => ImportEntity.Filaments,
            "printers" or "printer"                                 => ImportEntity.Printers,
            "model_designs" or "modeldesigns" or "models"           => ImportEntity.ModelDesigns,
            "printing_projects" or "printingprojects" or "projects" => ImportEntity.PrintingProjects,
            _                                                       => ImportEntity.Unknown
        };

    async Task<ImportFileResult> ImportFileAsync(
        string                                       filePath,
        ILogger                                      appLogger,
        ImportEntity                                 entity,
        bool                                         updateExisting,
        char                                         delimiter,
        CancellationToken                            ct,
        Dictionary<string, Dictionary<string, int>>? existingMap = null)
    {
        await using var fs = File.OpenRead(filePath);

        return await ImportStreamAsync(fs, appLogger, Path.GetFileName(filePath), updateExisting, delimiter, ct, existingMap).ConfigureAwait(false);
    }

    async Task ProcessRowAsync(
        ImportEntity                                 entity,
        IDictionary<string, object>                  dict,
        ImportResult                                 result,
        bool                                         updateExisting,
        CancellationToken                            ct,
        Dictionary<string, Dictionary<string, int>>? existingMappings = null,
        ILogger?                                     appLogger        = null)
    {
        try
        {
            appLogger?.LogDebug("Processing row for entity {Entity}: {@Record}", entity, dict);
            switch (entity)
            {
                case ImportEntity.Customers:
                {
                    var id   = ParseIntOrZero(GetString(dict, "Id"));
                    var name = GetString(dict, "Name");
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        result.Errors.Add("Customer: Name is required");
                        result.Failed++;

                        return;
                    }

                    var dto = id != 0 ? new CustomerDto(id, name) : new CustomerDto(name);
                    if (id != 0 && updateExisting)
                    {
                        var r = await vm.EditCustomerAsync(dto).ConfigureAwait(false);
                        if (r == ValidationResult.Success)
                        {
                            result.Updated++;
                        }
                        else
                        {
                            result.Errors.Add(r.ErrorMessage ?? "EditCustomer failed");
                            result.Failed++;
                        }
                    }
                    else
                    {
                        var r = await vm.AddCustomerAsync(dto).ConfigureAwait(false);
                        if (r.ValidationResult == ValidationResult.Success)
                        {
                            result.Created++;
                            if (id != 0)
                            {
                                var dbEntity =
                                    await db.Customers.FirstOrDefaultAsync(x => EF.Functions.Collate(x.Name, "NOCASE") == name, ct).ConfigureAwait(false);
                                if (dbEntity != null)
                                {
                                    result.RecordMapping("customers", id.ToString(), dbEntity.Id);
                                }
                            }
                        }
                        else
                        {
                            result.Errors.Add(r.ValidationResult.ErrorMessage ?? "AddCustomer failed");
                            result.Failed++;
                        }
                    }

                    break;
                }

                case ImportEntity.Manufacturers:
                {
                    var id   = ParseIntOrZero(GetString(dict, "Id"));
                    var name = GetString(dict, "Name");
                    var isFilament =
                        ParseBoolOrDefault(
                            GetString(dict, "IsFilamentManufacturer") ?? GetString(dict, "IsFilament") ?? GetString(dict, "IsFilamentManufacturer"));
                    var isPrinter =
                        ParseBoolOrDefault(
                            GetString(dict, "IsPrinterManufacturer") ?? GetString(dict, "IsPrinter") ?? GetString(dict, "IsPrinterManufacturer"));
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        result.Errors.Add("Manufacturer: Name is required");
                        result.Failed++;

                        return;
                    }

                    // Create DTO with flags. Use explicit id (0 when not supplied) so flags are preserved for new records.
                    var dto = new ManufacturerDto(id, name, isFilament, isPrinter);
                    if (id != 0 && updateExisting)
                    {
                        var r = await vm.EditManufacturerAsync(dto).ConfigureAwait(false);
                        if (r == ValidationResult.Success)
                        {
                            result.Updated++;
                        }
                        else
                        {
                            result.Errors.Add(r.ErrorMessage ?? "EditManufacturer failed");
                            result.Failed++;
                        }
                    }
                    else
                    {
                        var r = await vm.AddManufacturerAsync(dto).ConfigureAwait(false);
                        if (r.ValidationResult == ValidationResult.Success)
                        {
                            result.Created++;
                            if (id != 0)
                            {
                                var dbEntity =
                                    await db.Manufacturers.FirstOrDefaultAsync(x => EF.Functions.Collate(x.Name, "NOCASE") == name, ct).ConfigureAwait(false);
                                if (dbEntity != null)
                                {
                                    result.RecordMapping("manufacturers", id.ToString(), dbEntity.Id);
                                }
                            }
                        }
                        else
                        {
                            result.Errors.Add(r.ValidationResult.ErrorMessage ?? "AddManufacturer failed");
                            result.Failed++;
                        }
                    }

                    break;
                }

                case ImportEntity.FilamentColours:
                {
                    var id   = ParseIntOrZero(GetString(dict, "Id"));
                    var desc = GetString(dict, "Description") ?? GetString(dict, "Name");
                    if (string.IsNullOrWhiteSpace(desc))
                    {
                        result.Errors.Add("FilamentColour: Description is required");
                        result.Failed++;

                        return;
                    }

                    // Prefer to match existing records by description (case-insensitive). If a matching
                    // description exists, map the source id (if provided) to the existing id and skip
                    // creating a duplicate. If no description match exists, allow updating by ID when
                    // updateExisting is true, otherwise create a new record.
                    var existingByDesc =
                        await db.FilamentColours.FirstOrDefaultAsync(x => EF.Functions.Collate(x.Description, "NOCASE") == desc, ct).ConfigureAwait(false);

                    if (existingByDesc is not null)
                    {
                        // Map source id to existing id if provided
                        if (id != 0)
                        {
                            result.RecordMapping("filament_colours", id.ToString(), existingByDesc.Id);
                        }

                        // Nothing to update because description matches; consider as skipped
                        result.Skipped++;
                    }
                    else
                    {
                        var dto = id != 0 ? new FilamentColourDto(id, desc) : new FilamentColourDto(desc);
                        if (id != 0 && updateExisting)
                        {
                            var r = await vm.EditFilamentColourAsync(dto).ConfigureAwait(false);
                            if (r == ValidationResult.Success)
                            {
                                result.Updated++;
                            }
                            else
                            {
                                result.Errors.Add(r.ErrorMessage ?? "EditFilamentColour failed");
                                result.Failed++;
                            }
                        }
                        else
                        {
                            var r = await vm.AddFilamentColourAsync(dto).ConfigureAwait(false);
                            if (r.ValidationResult == ValidationResult.Success)
                            {
                                result.Created++;
                                if (id != 0)
                                {
                                    var dbEntity =
                                        await db
                                              .FilamentColours.FirstOrDefaultAsync(x => EF.Functions.Collate(x.Description, "NOCASE") == desc, ct)
                                              .ConfigureAwait(false);
                                    if (dbEntity != null)
                                    {
                                        result.RecordMapping("filament_colours", id.ToString(), dbEntity.Id);
                                    }
                                }
                            }
                            else
                            {
                                result.Errors.Add(r.ValidationResult.ErrorMessage ?? "AddFilamentColour failed");
                                result.Failed++;
                            }
                        }
                    }

                    break;
                }

                case ImportEntity.FilamentTypes:
                {
                    var id   = ParseIntOrZero(GetString(dict, "Id"));
                    var desc = GetString(dict, "Description") ?? GetString(dict, "Name");
                    if (string.IsNullOrWhiteSpace(desc))
                    {
                        result.Errors.Add("FilamentType: Description is required");
                        result.Failed++;

                        return;
                    }

                    // Prefer matching existing records by description (case-insensitive). If found,
                    // map source id to existing id and skip creation. Otherwise allow update by ID
                    // when updateExisting is true, or add a new record.
                    var existingByDesc =
                        await db.FilamentTypes.FirstOrDefaultAsync(x => EF.Functions.Collate(x.Description, "NOCASE") == desc, ct).ConfigureAwait(false);

                    if (existingByDesc is not null)
                    {
                        if (id != 0)
                        {
                            result.RecordMapping("filament_types", id.ToString(), existingByDesc.Id);
                        }

                        result.Skipped++;
                    }
                    else
                    {
                        var dto = id != 0 ? new FilamentTypeDto(id, desc) : new FilamentTypeDto(desc);
                        if (id != 0 && updateExisting)
                        {
                            var r = await vm.EditFilamentTypeAsync(dto).ConfigureAwait(false);
                            if (r == ValidationResult.Success)
                            {
                                result.Updated++;
                            }
                            else
                            {
                                result.Errors.Add(r.ErrorMessage ?? "EditFilamentType failed");
                                result.Failed++;
                            }
                        }
                        else
                        {
                            var r = await vm.AddFilamentTypeAsync(dto).ConfigureAwait(false);
                            if (r.ValidationResult == ValidationResult.Success)
                            {
                                result.Created++;
                                if (id != 0)
                                {
                                    var dbEntity =
                                        await db
                                              .FilamentTypes.FirstOrDefaultAsync(x => EF.Functions.Collate(x.Description, "NOCASE") == desc, ct)
                                              .ConfigureAwait(false);
                                    if (dbEntity != null)
                                    {
                                        result.RecordMapping("filament_types", id.ToString(), dbEntity.Id);
                                    }
                                }
                            }
                            else
                            {
                                result.Errors.Add(r.ValidationResult.ErrorMessage ?? "AddFilamentType failed");
                                result.Failed++;
                            }
                        }
                    }

                    break;
                }

                case ImportEntity.Filaments:
                {
                    var id        = ParseIntOrZero(GetString(dict, "Id"));
                    var cost      = ParseDecimalOrZero(GetString(dict, "CostPerWeight") ?? GetString(dict, "Cost"));
                    var productId = GetString(dict, "ProductId");
                    var reorder   = GetString(dict, "ReorderLink") ?? GetString(dict, "ReorderUrl") ?? GetString(dict, "Url");

                    // Read raw fields (may be numeric IDs or string names)
                    var colourField = GetString(dict, "FilamentColourId") ?? GetString(dict, "FilamentColour") ?? GetString(dict, "ColourId");
                    var typeField   = GetString(dict, "FilamentTypeId")   ?? GetString(dict, "FilamentType")   ?? GetString(dict, "TypeId");
                    var manField    = GetString(dict, "ManufacturerId")   ?? GetString(dict, "Manufacturer")   ?? GetString(dict, "MakerId");

                    var colourId = ParseIntOrZero(colourField);
                    var typeId   = ParseIntOrZero(typeField);
                    var manId    = ParseIntOrZero(manField);

                    // If name values were supplied instead of numeric IDs, try to create/resolve them via the ViewModel
                    if (colourId == 0 && !string.IsNullOrWhiteSpace(colourField))
                    {
                        // attempt to create or resolve filament colour
                        await vm.AddFilamentColourAsync(new FilamentColourDto(0, colourField)).ConfigureAwait(false);
                        var allCols = await vm.GetAllFilamentColoursAsync().ConfigureAwait(false);
                        var matched = allCols.FirstOrDefault(c => string.Equals(c.Description, colourField, StringComparison.OrdinalIgnoreCase));
                        if (matched != null)
                        {
                            colourId = matched.Id;
                        }
                    }

                    if (typeId == 0 && !string.IsNullOrWhiteSpace(typeField))
                    {
                        await vm.AddFilamentTypeAsync(new FilamentTypeDto(0, typeField)).ConfigureAwait(false);
                        var allTypes = await vm.GetAllFilamentTypesAsync().ConfigureAwait(false);
                        var matched  = allTypes.FirstOrDefault(t => string.Equals(t.Description, typeField, StringComparison.OrdinalIgnoreCase));
                        if (matched != null)
                        {
                            typeId = matched.Id;
                        }
                    }

                    if (manId == 0 && !string.IsNullOrWhiteSpace(manField))
                    {
                        await vm.AddManufacturerAsync(new ManufacturerDto(0, manField)).ConfigureAwait(false);
                        var allMans = await vm.GetAllManufacturersAsync().ConfigureAwait(false);
                        var matched = allMans.FirstOrDefault(m => string.Equals(m.Name, manField, StringComparison.OrdinalIgnoreCase));
                        if (matched != null)
                        {
                            manId = matched.Id;
                        }
                    }

                    var colourDto = new FilamentColourDto(colourId, string.Empty);
                    var typeDto   = new FilamentTypeDto(typeId, string.Empty);
                    var manDto    = new ManufacturerDto(manId, string.Empty);

                    var dto =
                        id != 0
                            ? new FilamentDto(id,   cost,      productId, reorder,   colourDto, typeDto, manDto)
                            : new FilamentDto(cost, productId, reorder,   colourDto, typeDto,   manDto);

                    if (id != 0 && updateExisting)
                    {
                        var r = await vm.EditFilamentAsync(dto).ConfigureAwait(false);
                        if (r == ValidationResult.Success)
                        {
                            result.Updated++;
                        }
                        else
                        {
                            result.Errors.Add(r.ErrorMessage ?? "EditFilament failed");
                            result.Failed++;
                        }
                    }
                    else
                    {
                        // Check whether a filament with the same
                        // ManufacturerId, FilamentTypeId and FilamentColourId already exists to avoid duplicates.
                        if (manId != 0 && typeId != 0 && colourId != 0)
                        {
                            var existingFilament =
                                await db
                                      .Filaments.FirstOrDefaultAsync(
                                          x => x.ManufacturerId == manId && x.FilamentTypeId == typeId && x.FilamentColourId == colourId,
                                          ct)
                                      .ConfigureAwait(false);
                            if (existingFilament is not null)
                            {
                                // Considered already existing; skip creation.
                                result.Skipped++;

                                break;
                            }
                        }

                        var r = await vm.AddFilamentAsync(dto).ConfigureAwait(false);
                        if (r.ValidationResult == ValidationResult.Success)
                        {
                            result.Created++;
                            if (id != 0)
                            {
                                Filament? dbEntity = null;
                                if (!string.IsNullOrWhiteSpace(productId))
                                {
                                    dbEntity = await db.Filaments.FirstOrDefaultAsync(x => x.ProductId == productId, ct).ConfigureAwait(false);
                                }

                                dbEntity ??=
                                    await db
                                          .Filaments.FirstOrDefaultAsync(
                                              x => x.ManufacturerId == manId && x.FilamentTypeId == typeId && x.FilamentColourId == colourId,
                                              ct)
                                          .ConfigureAwait(false);

                                if (dbEntity != null)
                                {
                                    result.RecordMapping("filaments", id.ToString(), dbEntity.Id);
                                }
                            }
                        }
                        else
                        {
                            result.Errors.Add(r.ValidationResult.ErrorMessage ?? "AddFilament failed");
                            result.Failed++;
                        }
                    }

                    break;
                }

                case ImportEntity.ModelDesigns:
                {
                    var id      = ParseIntOrZero(GetString(dict, "Id"));
                    var desc    = GetString(dict, "Description") ?? GetString(dict, "Name");
                    var length  = ParseDecimalOrZero(GetString(dict, "Length"));
                    var summary = GetString(dict, "Summary");
                    var url     = GetString(dict, "Url");

                    if (string.IsNullOrWhiteSpace(desc))
                    {
                        result.Errors.Add("ModelDesign: Description is required");
                        result.Failed++;

                        return;
                    }

                    var dto =
                        id != 0
                            ? new ModelDesignDto(id,   desc,   length, summary ?? string.Empty, url)
                            : new ModelDesignDto(desc, length, summary         ?? string.Empty, url);
                    if (id != 0 && updateExisting)
                    {
                        var r = await vm.EditModelDesignAsync(dto).ConfigureAwait(false);
                        if (r == ValidationResult.Success)
                        {
                            result.Updated++;
                        }
                        else
                        {
                            result.Errors.Add(r.ErrorMessage ?? "EditModelDesign failed");
                            result.Failed++;
                        }
                    }
                    else
                    {
                        var r = await vm.AddModelDesignAsync(dto).ConfigureAwait(false);
                        if (r.ValidationResult == ValidationResult.Success)
                        {
                            result.Created++;
                            if (id != 0)
                            {
                                var dbEntity =
                                    await db
                                          .ModelDesigns.FirstOrDefaultAsync(x => EF.Functions.Collate(x.Description, "NOCASE") == desc, ct)
                                          .ConfigureAwait(false);
                                if (dbEntity != null)
                                {
                                    result.RecordMapping("model_designs", id.ToString(), dbEntity.Id);
                                }
                            }
                        }
                        else
                        {
                            result.Errors.Add(r.ValidationResult.ErrorMessage ?? "AddModelDesign failed");
                            result.Failed++;
                        }
                    }

                    break;
                }

                case ImportEntity.PrintingProjects:
                {
                    var id             = ParseIntOrZero(GetString(dict,         "Id"));
                    var cost           = ParseDecimalOrZero(GetString(dict,     "Cost"));
                    var submitted      = ParseDateOnlyOrDefault(GetString(dict, "Submitted"));
                    var completed      = ParseDateOnlyOrDefault(GetString(dict, "Completed"));
                    var costPerHour    = ParseDecimalOrZero(GetString(dict,     "CostPerHour"));
                    var customerId     = ParseIntOrZero(GetString(dict, "CustomerId")    ?? GetString(dict, "Customer"));
                    var modelId        = ParseIntOrZero(GetString(dict, "ModelDesignId") ?? GetString(dict, "ModelDesign"));
                    var filamentIdsRaw = GetString(dict, "FilamentIds") ?? GetString(dict, "FilamentId") ?? GetString(dict, "Filaments");

                    CustomerDto? customerDto = null;
                    if (customerId != 0)
                    {
                        customerDto = new CustomerDto(customerId, string.Empty);
                    }

                    ModelDesignDto? modelDto = null;
                    if (modelId != 0)
                    {
                        modelDto = new ModelDesignDto(modelId, string.Empty, 0m, string.Empty, null);
                    }

                    // Parse printer information: prefer explicit id, then separate Manufacturer/Model columns, then single Printer column
                    var         printerId  = ParseIntOrZero(GetString(dict, "PrinterId") ?? GetString(dict, "Printer"));
                    PrinterDto? printerDto = null;
                    if (printerId != 0)
                    {
                        printerDto = new PrinterDto(printerId, string.Empty);
                    }
                    else
                    {
                        var printerManufacturer = GetString(dict, "PrinterManufacturer") ?? GetString(dict, "PrinterMaker") ?? GetString(dict, "PrinterBrand");
                        var printerModel        = GetString(dict, "PrinterModel");

                        if (!string.IsNullOrWhiteSpace(printerManufacturer) || !string.IsNullOrWhiteSpace(printerModel))
                        {
                            if (!string.IsNullOrWhiteSpace(printerManufacturer))
                            {
                                var manu = new ManufacturerDto(0, printerManufacturer!.Trim());
                                printerDto = new PrinterDto(manu, printerModel?.Trim() ?? string.Empty, costPerHour);
                            }
                            else
                            {
                                // Manufacturer missing - use model only
                                printerDto = new PrinterDto(0, null, printerModel?.Trim() ?? string.Empty, costPerHour);
                            }
                        }
                    }

                    var filamentDtos = new List<FilamentDto>();
                    if (!string.IsNullOrWhiteSpace(filamentIdsRaw))
                    {
                        var parts = filamentIdsRaw.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries);
                        foreach (var p in parts)
                        {
                            var fid = ParseIntOrZero(p.Trim());
                            if (fid == 0)
                            {
                                continue;
                            }

                            // create minimal filament DTO with id only; AddPrintingProjectAsync will resolve existing entity
                            var placeholder =
                                new FilamentDto(
                                    fid,
                                    0m,
                                    null,
                                    null,
                                    new FilamentColourDto(0, string.Empty),
                                    new FilamentTypeDto(0, string.Empty),
                                    new ManufacturerDto(0, string.Empty));
                            filamentDtos.Add(placeholder);
                        }
                    }

                    var projDto =
                        id != 0
                            ? new PrintingProjectDto(
                                id,
                                cost,
                                submitted == DateOnly.MinValue ? DateOnly.FromDateTime(DateTime.Now) : submitted,
                                completed == DateOnly.MinValue ? null : completed,
                                customerDto,
                                modelDto,
                                printerDto,
                                filamentDtos)
                            : new PrintingProjectDto(
                                cost,
                                submitted == DateOnly.MinValue ? DateOnly.FromDateTime(DateTime.Now) : submitted,
                                completed == DateOnly.MinValue ? null : completed,
                                customerDto,
                                modelDto,
                                printerDto,
                                filamentDtos);

                    if (id != 0 && updateExisting)
                    {
                        var r = await vm.EditPrintingProjectAsync(projDto).ConfigureAwait(false);
                        if (r == ValidationResult.Success)
                        {
                            result.Updated++;
                        }
                        else
                        {
                            result.Errors.Add(r.ErrorMessage ?? "EditPrintingProject failed");
                            result.Failed++;
                        }
                    }
                    else
                    {
                        var r = await vm.AddPrintingProjectAsync(projDto).ConfigureAwait(false);
                        if (r.ValidationResult == ValidationResult.Success)
                        {
                            result.Created++;
                            if (id != 0)
                            {
                                var projectCustomerId = projDto.Customer?.Id    ?? 0;
                                var mdlId             = projDto.ModelDesign?.Id ?? 0;
                                var dbEntity =
                                    await db
                                          .PrintingProjects.FirstOrDefaultAsync(
                                              x => x.Cost == projDto.Cost && x.CustomerId == projectCustomerId && x.ModelDesignId == mdlId,
                                              ct)
                                          .ConfigureAwait(false);
                                if (dbEntity != null)
                                {
                                    result.RecordMapping("printing_projects", id.ToString(), dbEntity.Id);
                                }
                            }
                        }
                        else
                        {
                            result.Errors.Add(r.ValidationResult.ErrorMessage ?? "AddPrintingProject failed");
                            result.Failed++;
                        }
                    }

                    break;
                }
                case ImportEntity.Unknown:
                default:
                    result.Errors.Add($"Unknown entity '{entity}'");
                    result.Failed++;

                    break;
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add(ex.Message);
            result.Failed++;
        }

        return;

        // helper locals
        static string? GetString(IDictionary<string, object> d, string key) =>
            (from k in d.Keys where string.Equals(k, key, StringComparison.OrdinalIgnoreCase) select d[k] into v select v?.ToString()?.Trim()).FirstOrDefault();

        static int ParseIntOrZero(string? s) => int.TryParse(s, out var i) ? i : 0;

        static decimal ParseDecimalOrZero(string? s) => decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? d : 0m;

        static DateOnly ParseDateOnlyOrDefault(string? s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return DateOnly.MinValue;
            }

            if (DateOnly.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) || DateOnly.TryParse(s, out d))
            {
                return d;
            }

            return DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ? DateOnly.FromDateTime(dt) : DateOnly.MinValue;
        }

        static bool ParseBoolOrDefault(string? s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }

            if (bool.TryParse(s, out var b))
            {
                return b;
            }

            // Accept common truthy values
            return s.Equals("1",       StringComparison.OrdinalIgnoreCase)
                   || s.Equals("yes",  StringComparison.OrdinalIgnoreCase)
                   || s.Equals("y",    StringComparison.OrdinalIgnoreCase)
                   || s.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
    }
}