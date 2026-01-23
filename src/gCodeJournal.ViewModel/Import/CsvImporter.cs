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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Model;
#endregion

public class CsvImporter(GCodeJournalDbContext db, GCodeJournalViewModel vm)
{
    #region ImportEntity Enum
    private enum ImportEntity
    {
        Unknown,
        Customers,
        Manufacturers,
        FilamentColours,
        FilamentTypes,
        Filaments,
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
            var ordered = new[]
            {
                ImportEntity.Customers,
                ImportEntity.Manufacturers,
                ImportEntity.FilamentColours,
                ImportEntity.FilamentTypes,
                ImportEntity.Filaments,
                ImportEntity.ModelDesigns,
                ImportEntity.PrintingProjects
            };

            // mergedMap carries mappings between files so later files can resolve references
            var mergedMap = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in ordered.Select(EntityFileName).Select(fileName => Path.Combine(path, fileName)).Where(File.Exists))
            {
                await using var fs = File.OpenRead(file);
                var             r  = await ImportStreamAsync(fs, appLogger, Path.GetFileName(file), updateExisting, delimiter, ct, mergedMap).ConfigureAwait(false);

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

    // accept optional existing mappings so single-file imports in a batch can resolve earlier-created ids
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
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter.ToString(), PrepareHeaderForMatch = args => args.Header?.Trim().ToLowerInvariant()
        };

        using var csv = new CsvReader(sr, csvConfig);

        // Try to detect entity from filename
        var entity = DetectEntityFromFileName(fileName);

        await using var tx = await db.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
        try
        {
                if (entity == ImportEntity.Unknown)
            {
                // assume one-row-per-file contains Entity column
                var records = csv.GetRecords<dynamic>();
                foreach (var dict in records.Select(rec => (IDictionary<string, object>) rec))
                {
                        appLogger.LogDebug("Processing dynamic record for unknown-entity CSV: {@Record}", dict);
                    if (!dict.TryGetValue("Entity", out var en))
                    {
                        result.Errors.Add("No Entity column found in CSV and filename did not match a known entity");
                        result.Failed++;
                        continue;
                    }

                    var entityName = en?.ToString() ?? string.Empty;
                    var ent        = ParseEntityName(entityName);
                    await ProcessRowAsync(ent, dict, result, updateExisting, ct, existingMappings, appLogger).ConfigureAwait(false);
                }
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

                            if (c.Id != 0 && updateExisting)
                            {
                                var r = await vm.EditCustomerAsync(c).ConfigureAwait(false);
                                if (r == ValidationResult.Success)
                                {
                                    result.Updated++;

                                    if (sourceId != null)
                                    {
                                        var dbEntity = await db.Customers.FindAsync(new object[] { c.Id }, ct).ConfigureAwait(false)
                                                       ?? await db.Customers.FirstOrDefaultAsync(x => EF.Functions.Collate(x.Name, "NOCASE") == c.Name, ct).ConfigureAwait(false);
                                        if (dbEntity != null)
                                        {
                                            result.RecordMapping("customers", sourceId, dbEntity.Id);
                                        }
                                    }
                                }
                                else
                                {
                                    result.Errors.Add(r.ErrorMessage ?? "EditCustomer failed");
                                    result.Failed++;
                                }
                            }
                            else
                            {
                                var r = await vm.AddCustomerAsync(c).ConfigureAwait(false);
                                if (r == ValidationResult.Success)
                                {
                                    result.Created++;
                                    if (sourceId == null)
                                    {
                                        continue;
                                    }

                                    var dbEntity = await db.Customers.FirstOrDefaultAsync(x => EF.Functions.Collate(x.Name, "NOCASE") == c.Name, ct).ConfigureAwait(false);
                                    if (dbEntity != null)
                                    {
                                        result.RecordMapping("customers", sourceId, dbEntity.Id);
                                    }
                                }
                                else
                                {
                                    result.Errors.Add(r.ErrorMessage ?? "AddCustomer failed");
                                    result.Failed++;
                                }
                            }
                        }

                        break;

                    case ImportEntity.Manufacturers:
                        var mans = csv.GetRecords<ManufacturerDto>().ToList();
                        foreach (var m in mans)
                        {
                            appLogger.LogDebug("Processing Manufacturer DTO: {@Dto}", m);
                            var sourceId = m.Id != 0 ? m.Id.ToString() : null;

                            if (m.Id != 0 && updateExisting)
                            {
                                var r = await vm.EditManufacturerAsync(m).ConfigureAwait(false);
                                if (r == ValidationResult.Success)
                                {
                                    result.Updated++;
                                    if (sourceId != null)
                                    {
                                        var dbEntity = await db.Manufacturers.FindAsync(new object[] { m.Id }, ct).ConfigureAwait(false)
                                                       ?? await db.Manufacturers.FirstOrDefaultAsync(x => EF.Functions.Collate(x.Name, "NOCASE") == m.Name, ct).ConfigureAwait(false);
                                        if (dbEntity != null)
                                        {
                                            result.RecordMapping("manufacturers", sourceId, dbEntity.Id);
                                        }
                                    }
                                }
                                else
                                {
                                    result.Errors.Add(r.ErrorMessage ?? "EditManufacturer failed");
                                    result.Failed++;
                                }
                            }
                            else
                            {
                                var r = await vm.AddManufacturerAsync(m).ConfigureAwait(false);
                                if (r == ValidationResult.Success)
                                {
                                    result.Created++;
                                    if (sourceId == null)
                                    {
                                        continue;
                                    }

                                    var dbEntity = await db.Manufacturers.FirstOrDefaultAsync(x => EF.Functions.Collate(x.Name, "NOCASE") == m.Name, ct).ConfigureAwait(false);
                                    if (dbEntity != null)
                                    {
                                        result.RecordMapping("manufacturers", sourceId, dbEntity.Id);
                                    }
                                }
                                else
                                {
                                    result.Errors.Add(r.ErrorMessage ?? "AddManufacturer failed");
                                    result.Failed++;
                                }
                            }
                        }

                        break;

                    case ImportEntity.FilamentColours:
                        var cols = csv.GetRecords<FilamentColourDto>().ToList();
                        foreach (var c in cols)
                        {
                            appLogger.LogDebug("Processing FilamentColour DTO: {@Dto}", c);
                            var sourceId = c.Id != 0 ? c.Id.ToString() : null;

                            if (c.Id != 0 && updateExisting)
                            {
                                var r = await vm.EditFilamentColourAsync(c).ConfigureAwait(false);
                                if (r == ValidationResult.Success)
                                {
                                    result.Updated++;
                                    if (sourceId != null)
                                    {
                                        var dbEntity = await db.FilamentColours.FindAsync(new object[] { c.Id }, ct).ConfigureAwait(false)
                                                       ?? await db.FilamentColours.FirstOrDefaultAsync(x => EF.Functions.Collate(x.Description, "NOCASE") == c.Description, ct).ConfigureAwait(false);
                                        if (dbEntity != null)
                                        {
                                            result.RecordMapping("filament_colours", sourceId, dbEntity.Id);
                                        }
                                    }
                                }
                                else
                                {
                                    result.Errors.Add(r.ErrorMessage ?? "EditFilamentColour failed");
                                    result.Failed++;
                                }
                            }
                            else
                            {
                                var r = await vm.AddFilamentColourAsync(c).ConfigureAwait(false);
                                if (r == ValidationResult.Success)
                                {
                                    result.Created++;
                                    if (sourceId == null)
                                    {
                                        continue;
                                    }

                                    var dbEntity = await db.FilamentColours.FirstOrDefaultAsync(x => EF.Functions.Collate(x.Description, "NOCASE") == c.Description, ct)
                                                           .ConfigureAwait(false);
                                    if (dbEntity != null)
                                    {
                                        result.RecordMapping("filament_colours", sourceId, dbEntity.Id);
                                    }
                                }
                                else
                                {
                                    result.Errors.Add(r.ErrorMessage ?? "AddFilamentColour failed");
                                    result.Failed++;
                                }
                            }
                        }

                        break;

                    case ImportEntity.FilamentTypes:
                        var types = csv.GetRecords<FilamentTypeDto>().ToList();
                        foreach (var t in types)
                        {
                            appLogger.LogDebug("Processing FilamentType DTO: {@Dto}", t);
                            var sourceId = t.Id != 0 ? t.Id.ToString() : null;

                            if (t.Id != 0 && updateExisting)
                            {
                                var r = await vm.EditFilamentTypeAsync(t).ConfigureAwait(false);
                                if (r == ValidationResult.Success)
                                {
                                    result.Updated++;
                                    if (sourceId != null)
                                    {
                                        var dbEntity = await db.FilamentTypes.FindAsync(new object[] { t.Id }, ct).ConfigureAwait(false)
                                                       ?? await db.FilamentTypes.FirstOrDefaultAsync(x => EF.Functions.Collate(x.Description, "NOCASE") == t.Description, ct).ConfigureAwait(false);
                                        if (dbEntity != null)
                                        {
                                            result.RecordMapping("filament_types", sourceId, dbEntity.Id);
                                        }
                                    }
                                }
                                else
                                {
                                    result.Errors.Add(r.ErrorMessage ?? "EditFilamentType failed");
                                    result.Failed++;
                                }
                            }
                            else
                            {
                                var r = await vm.AddFilamentTypeAsync(t).ConfigureAwait(false);

                                // TODO: Add ValidationResult.Skipped if duplicate and updateExisting is false
                                if (r == ValidationResult.Success)
                                {
                                    result.Created++;
                                    if (sourceId == null)
                                    {
                                        continue;
                                    }

                                    var dbEntity = await db.FilamentTypes.FirstOrDefaultAsync(x => EF.Functions.Collate(x.Description, "NOCASE") == t.Description, ct)
                                                           .ConfigureAwait(false);
                                    if (dbEntity != null)
                                    {
                                        result.RecordMapping("filament_types", sourceId, dbEntity.Id);
                                    }
                                }
                                else
                                {
                                    result.Errors.Add(r.ErrorMessage ?? "AddFilamentType failed");
                                    result.Failed++;
                                }
                            }
                        }

                        break;

                    case ImportEntity.Filaments:
                        // Use the dynamic reader and reuse ProcessRowAsync which already contains robust parsing logic for filaments (handles multiple header name variants).
                        var filamentRecords = csv.GetRecords<dynamic>();
                        foreach (var rec in filamentRecords.Select(r => (IDictionary<string, object>) r))
                        {
                            appLogger.LogDebug("Processing dynamic Filaments record: {@Record}", rec);
                            await ProcessRowAsync(ImportEntity.Filaments, rec, result, updateExisting, ct, existingMappings, appLogger).ConfigureAwait(false);
                        }

                        break;

                    case ImportEntity.ModelDesigns:
                        var models = csv.GetRecords<ModelDesignDto>().ToList();
                        foreach (var m in models)
                        {
                            appLogger.LogDebug("Processing ModelDesign DTO: {@Dto}", m);
                            var sourceId = m.Id != 0 ? m.Id.ToString() : null;

                            if (m.Id != 0 && updateExisting)
                            {
                                var r = await vm.EditModelDesignAsync(m).ConfigureAwait(false);
                                if (r == ValidationResult.Success)
                                {
                                    result.Updated++;
                                    if (sourceId != null)
                                    {
                                        var dbEntity = await db.ModelDesigns.FindAsync(new object[] { m.Id }, ct).ConfigureAwait(false)
                                                       ?? await db.ModelDesigns.FirstOrDefaultAsync(x => EF.Functions.Collate(x.Description, "NOCASE") == m.Description, ct).ConfigureAwait(false);
                                        if (dbEntity != null)
                                        {
                                            result.RecordMapping("model_designs", sourceId, dbEntity.Id);
                                        }
                                    }
                                }
                                else
                                {
                                    result.Errors.Add(r.ErrorMessage ?? "EditModelDesign failed");
                                    result.Failed++;
                                }
                            }
                            else
                            {
                                var r = await vm.AddModelDesignAsync(m).ConfigureAwait(false);
                                if (r == ValidationResult.Success)
                                {
                                    result.Created++;
                                    if (sourceId == null)
                                    {
                                        continue;
                                    }

                                    var dbEntity = await db.ModelDesigns.FirstOrDefaultAsync(x => EF.Functions.Collate(x.Description, "NOCASE") == m.Description, ct)
                                                           .ConfigureAwait(false);
                                    if (dbEntity != null)
                                    {
                                        result.RecordMapping("model_designs", sourceId, dbEntity.Id);
                                    }
                                }
                                else
                                {
                                    result.Errors.Add(r.ErrorMessage ?? "AddModelDesign failed");
                                    result.Failed++;
                                }
                            }
                        }

                        break;

                    case ImportEntity.PrintingProjects:
                        var projectRecords = csv.GetRecords<dynamic>();
                        foreach (var rec in projectRecords.Select(r => (IDictionary<string, object>) r))
                        {
                            appLogger.LogDebug("Processing dynamic PrintingProjects record: {@Record}", rec);
                            await ProcessRowAsync(ImportEntity.PrintingProjects, rec, result, updateExisting, ct, existingMappings, appLogger).ConfigureAwait(false);
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

    private static ImportEntity DetectEntityFromFileName(string? fileName)
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

        if (name.Contains("model_designs", StringComparison.OrdinalIgnoreCase))
        {
            return ImportEntity.ModelDesigns;
        }

        return name.Contains("printing_projects", StringComparison.OrdinalIgnoreCase) ? ImportEntity.PrintingProjects : ImportEntity.Unknown;
    }

    private static string EntityFileName(ImportEntity entity) => entity switch
    {
        ImportEntity.Customers        => "customers.csv",
        ImportEntity.Manufacturers    => "manufacturers.csv",
        ImportEntity.FilamentColours  => "filament_colours.csv",
        ImportEntity.FilamentTypes    => "filament_types.csv",
        ImportEntity.Filaments        => "filaments.csv",
        ImportEntity.ModelDesigns     => "model_designs.csv",
        ImportEntity.PrintingProjects => "printing_projects.csv",
        _                             => string.Empty
    };

    private static ImportEntity ParseEntityName(string name)
    {
        return name.ToLowerInvariant() switch
        {
            "customers" or "customer"                               => ImportEntity.Customers,
            "manufacturers" or "manufacturer"                       => ImportEntity.Manufacturers,
            "filament_colours" or "filamentcolours"                 => ImportEntity.FilamentColours,
            "filament_types" or "filamenttypes"                     => ImportEntity.FilamentTypes,
            "filaments" or "filament"                               => ImportEntity.Filaments,
            "model_designs" or "modeldesigns" or "models"           => ImportEntity.ModelDesigns,
            "printing_projects" or "printingprojects" or "projects" => ImportEntity.PrintingProjects,
            _                                                       => ImportEntity.Unknown
        };
    }

    private async Task<ImportFileResult> ImportFileAsync(
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

    private async Task ProcessRowAsync(
        ImportEntity                                 entity,
        IDictionary<string, object>                  dict,
        ImportResult                                 result,
        bool                                         updateExisting,
        CancellationToken                            ct,
        Dictionary<string, Dictionary<string, int>>? existingMappings = null,
        ILogger?                                     appLogger = null)
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
                        if (r == ValidationResult.Success)
                        {
                            result.Created++;
                            if (id != 0)
                            {
                                var dbEntity = await db.Customers.FirstOrDefaultAsync(x => EF.Functions.Collate(x.Name, "NOCASE") == name, ct).ConfigureAwait(false);
                                if (dbEntity != null)
                                {
                                    result.RecordMapping("customers", id.ToString(), dbEntity.Id);
                                }
                            }
                        }
                        else
                        {
                            result.Errors.Add(r.ErrorMessage ?? "AddCustomer failed");
                            result.Failed++;
                        }
                    }

                    break;
                }

                case ImportEntity.Manufacturers:
                {
                    var id   = ParseIntOrZero(GetString(dict, "Id"));
                    var name = GetString(dict, "Name");
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        result.Errors.Add("Manufacturer: Name is required");
                        result.Failed++;
                        return;
                    }

                    var dto = id != 0 ? new ManufacturerDto(id, name) : new ManufacturerDto(name);
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
                        if (r == ValidationResult.Success)
                        {
                            result.Created++;
                            if (id != 0)
                            {
                                var dbEntity = await db.Manufacturers.FirstOrDefaultAsync(x => EF.Functions.Collate(x.Name, "NOCASE") == name, ct).ConfigureAwait(false);
                                if (dbEntity != null)
                                {
                                    result.RecordMapping("manufacturers", id.ToString(), dbEntity.Id);
                                }
                            }
                        }
                        else
                        {
                            result.Errors.Add(r.ErrorMessage ?? "AddManufacturer failed");
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
                        if (r == ValidationResult.Success)
                        {
                            result.Created++;
                            if (id != 0)
                            {
                                var dbEntity = await db.FilamentColours.FirstOrDefaultAsync(x => EF.Functions.Collate(x.Description, "NOCASE") == desc, ct)
                                                       .ConfigureAwait(false);
                                if (dbEntity != null)
                                {
                                    result.RecordMapping("filament_colours", id.ToString(), dbEntity.Id);
                                }
                            }
                        }
                        else
                        {
                            result.Errors.Add(r.ErrorMessage ?? "AddFilamentColour failed");
                            result.Failed++;
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
                        if (r == ValidationResult.Success)
                        {
                            result.Created++;
                            if (id != 0)
                            {
                                var dbEntity = await db.FilamentTypes.FirstOrDefaultAsync(x => EF.Functions.Collate(x.Description, "NOCASE") == desc, ct)
                                                       .ConfigureAwait(false);
                                if (dbEntity != null)
                                {
                                    result.RecordMapping("filament_types", id.ToString(), dbEntity.Id);
                                }
                            }
                        }
                        else
                        {
                            result.Errors.Add(r.ErrorMessage ?? "AddFilamentType failed");
                            result.Failed++;
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
                    var colourId  = ParseIntOrZero(GetString(dict, "FilamentColourId") ?? GetString(dict, "FilamentColour") ?? GetString(dict, "ColourId"));
                    var typeId    = ParseIntOrZero(GetString(dict, "FilamentTypeId")   ?? GetString(dict, "FilamentType")   ?? GetString(dict, "TypeId"));
                    var manId     = ParseIntOrZero(GetString(dict, "ManufacturerId")   ?? GetString(dict, "Manufacturer")   ?? GetString(dict, "MakerId"));

                    var colourDto = new FilamentColourDto(colourId, string.Empty);
                    var typeDto   = new FilamentTypeDto(typeId, string.Empty);
                    var manDto    = new ManufacturerDto(manId, string.Empty);

                    var dto = id != 0
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
                            var existingFilament = await db.Filaments.FirstOrDefaultAsync(
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
                        if (r == ValidationResult.Success)
                        {
                            result.Created++;
                            if (id != 0)
                            {
                                Filament? dbEntity = null;
                                if (!string.IsNullOrWhiteSpace(productId))
                                {
                                    dbEntity = await db.Filaments.FirstOrDefaultAsync(x => x.ProductId == productId, ct).ConfigureAwait(false);
                                }

                                dbEntity ??= await db.Filaments.FirstOrDefaultAsync(
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
                            result.Errors.Add(r.ErrorMessage ?? "AddFilament failed");
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

                    var dto = id != 0 ? new ModelDesignDto(id, desc, length, summary ?? string.Empty, url) : new ModelDesignDto(desc, length, summary ?? string.Empty, url);
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
                        if (r == ValidationResult.Success)
                        {
                            result.Created++;
                            if (id != 0)
                            {
                                var dbEntity = await db.ModelDesigns.FirstOrDefaultAsync(x => EF.Functions.Collate(x.Description, "NOCASE") == desc, ct)
                                                       .ConfigureAwait(false);
                                if (dbEntity != null)
                                {
                                    result.RecordMapping("model_designs", id.ToString(), dbEntity.Id);
                                }
                            }
                        }
                        else
                        {
                            result.Errors.Add(r.ErrorMessage ?? "AddModelDesign failed");
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
                            var placeholder = new FilamentDto(
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

                    var projDto = id != 0
                                      ? new PrintingProjectDto(
                                          id,
                                          cost,
                                          submitted == DateOnly.MinValue ? DateOnly.FromDateTime(DateTime.Now) : submitted,
                                          completed == DateOnly.MinValue ? null : completed,
                                          customerDto,
                                          modelDto,
                                          filamentDtos)
                                      : new PrintingProjectDto(
                                          cost,
                                          submitted == DateOnly.MinValue ? DateOnly.FromDateTime(DateTime.Now) : submitted,
                                          completed == DateOnly.MinValue ? null : completed,
                                          customerDto,
                                          modelDto,
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
                        if (r == ValidationResult.Success)
                        {
                            result.Created++;
                            if (id != 0)
                            {
                                var projectCustomerId = projDto.Customer?.Id    ?? 0;
                                var mdlId             = projDto.ModelDesign?.Id ?? 0;
                                var dbEntity = await db.PrintingProjects.FirstOrDefaultAsync(
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
                            result.Errors.Add(r.ErrorMessage ?? "AddPrintingProject failed");
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
    }
}