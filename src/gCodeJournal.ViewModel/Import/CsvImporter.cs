namespace gCodeJournal.ViewModel.Import;

#region Using Directives
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using DTOs;
using Microsoft.EntityFrameworkCore;
using Model;
using CsvHelper;
using CsvHelper.Configuration;
using Mapping;
#endregion

public class CsvImporter
{
    private readonly GCodeJournalDbContext _db;
    private readonly GCodeJournalViewModel _vm;

    public CsvImporter(GCodeJournalDbContext db, GCodeJournalViewModel vm)
    {
        _db = db;
        _vm = vm;
    }

    public async Task<ImportResult> ImportFromPathAsync(string path, bool updateExisting, char delimiter, CancellationToken ct)
    {
        var result = new ImportResult();
        if (File.Exists(path))
        {
            // single file - delegate to stream overload
            await using var fs = File.OpenRead(path);
            var res = await ImportStreamAsync(fs, Path.GetFileName(path), updateExisting, delimiter, ct).ConfigureAwait(false);
            return res;
        }

        if (Directory.Exists(path))
        {
            // process known file names in directory
            var map = new Dictionary<string, Func<string, Task<ImportResult>>>(StringComparer.OrdinalIgnoreCase)
            {
                ["customers.csv"] = async f => await ImportFileAsync(f, ImportEntity.Customers, updateExisting, delimiter, ct).ConfigureAwait(false),
                ["manufacturers.csv"] = async f => await ImportFileAsync(f, ImportEntity.Manufacturers, updateExisting, delimiter, ct).ConfigureAwait(false),
                ["filament_colours.csv"] = async f => await ImportFileAsync(f, ImportEntity.FilamentColours, updateExisting, delimiter, ct).ConfigureAwait(false),
                ["filament_types.csv"] = async f => await ImportFileAsync(f, ImportEntity.FilamentTypes, updateExisting, delimiter, ct).ConfigureAwait(false),
                ["filaments.csv"] = async f => await ImportFileAsync(f, ImportEntity.Filaments, updateExisting, delimiter, ct).ConfigureAwait(false),
                ["model_designs.csv"] = async f => await ImportFileAsync(f, ImportEntity.ModelDesigns, updateExisting, delimiter, ct).ConfigureAwait(false),
                ["printing_projects.csv"] = async f => await ImportFileAsync(f, ImportEntity.PrintingProjects, updateExisting, delimiter, ct).ConfigureAwait(false)
            };

            foreach (var kv in map)
            {
                var file = Path.Combine(path, kv.Key);
                if (!File.Exists(file))
                {
                    continue;
                }

                var r = await kv.Value(file).ConfigureAwait(false);
                result.Created += r.Created;
                result.Updated += r.Updated;
                result.Skipped += r.Skipped;
                result.Failed += r.Failed;
                result.Errors.AddRange(r.Errors);
            }

            return result;
        }

        result.Errors.Add($"Path '{path}' does not exist");
        result.Failed++;
        return result;
    }

    private async Task<ImportResult> ImportFileAsync(string filePath, ImportEntity entity, bool updateExisting, char delimiter, CancellationToken ct)
    {
        await using var fs = File.OpenRead(filePath);
        return await ImportStreamAsync(fs, Path.GetFileName(filePath), updateExisting, delimiter, ct).ConfigureAwait(false);
    }

    public async Task<ImportResult> ImportStreamAsync(Stream stream, string? fileName, bool updateExisting, char delimiter, CancellationToken ct)
    {
        var result = new ImportResult();
        using var sr = new StreamReader(stream, Encoding.UTF8, true, 8192, true);
        using var csv = new CsvReader(sr, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = delimiter.ToString() });

        // Try to detect entity from filename
        var entity = DetectEntityFromFileName(fileName);

        await using var tx = await _db.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
        try
        {
            if (entity == ImportEntity.Unknown)
            {
                // assume one-row-per-file contains Entity column
                var records = csv.GetRecords<dynamic>();
                foreach (var rec in records)
                {
                    var dict = (IDictionary<string, object>)rec;
                    if (!dict.TryGetValue("Entity", out var en))
                    {
                        result.Errors.Add("No Entity column found in CSV and filename did not match a known entity");
                        result.Failed++;
                        continue;
                    }

                    var entityName = en?.ToString() ?? string.Empty;
                    var ent = ParseEntityName(entityName);
                    await ProcessRowAsync(ent, dict, result, updateExisting, ct).ConfigureAwait(false);
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
                            var r = await _vm.AddCustomerAsync(c).ConfigureAwait(false);
                            if (r == ValidationResult.Success) result.Created++; else { result.Errors.Add(r.ErrorMessage ?? "AddCustomer failed"); result.Failed++; }
                        }

                        break;

                    case ImportEntity.Manufacturers:
                        var mans = csv.GetRecords<ManufacturerDto>().ToList();
                        foreach (var m in mans)
                        {
                            var r = await _vm.AddManufacturerAsync(m).ConfigureAwait(false);
                            if (r == ValidationResult.Success) result.Created++; else { result.Errors.Add(r.ErrorMessage ?? "AddManufacturer failed"); result.Failed++; }
                        }

                        break;

                    case ImportEntity.FilamentColours:
                        var cols = csv.GetRecords<FilamentColourDto>().ToList();
                        foreach (var c in cols)
                        {
                            var r = await _vm.AddFilamentColourAsync(c).ConfigureAwait(false);
                            if (r == ValidationResult.Success) result.Created++; else { result.Errors.Add(r.ErrorMessage ?? "AddFilamentColour failed"); result.Failed++; }
                        }

                        break;

                    case ImportEntity.FilamentTypes:
                        var types = csv.GetRecords<FilamentTypeDto>().ToList();
                        foreach (var t in types)
                        {
                            var r = await _vm.AddFilamentTypeAsync(t).ConfigureAwait(false);
                            if (r == ValidationResult.Success) result.Created++; else { result.Errors.Add(r.ErrorMessage ?? "AddFilamentType failed"); result.Failed++; }
                        }

                        break;

                    case ImportEntity.Filaments:
                        var filaments = csv.GetRecords<FilamentDto>().ToList();
                        foreach (var f in filaments)
                        {
                            var r = await _vm.AddFilamentAsync(f).ConfigureAwait(false);
                            if (r == ValidationResult.Success) result.Created++; else { result.Errors.Add(r.ErrorMessage ?? "AddFilament failed"); result.Failed++; }
                        }

                        break;

                    case ImportEntity.ModelDesigns:
                        var models = csv.GetRecords<ModelDesignDto>().ToList();
                        foreach (var m in models)
                        {
                            var r = await _vm.AddModelDesignAsync(m).ConfigureAwait(false);
                            if (r == ValidationResult.Success) result.Created++; else { result.Errors.Add(r.ErrorMessage ?? "AddModelDesign failed"); result.Failed++; }
                        }

                        break;

                    case ImportEntity.PrintingProjects:
                        var projects = csv.GetRecords<PrintingProjectDto>().ToList();
                        foreach (var p in projects)
                        {
                            var r = await _vm.AddPrintingProjectAsync(p).ConfigureAwait(false);
                            if (r == ValidationResult.Success) result.Created++; else { result.Errors.Add(r.ErrorMessage ?? "AddPrintingProject failed"); result.Failed++; }
                        }

                        break;
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

        return result;
    }

    private ImportEntity DetectEntityFromFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return ImportEntity.Unknown;
        }

        var name = fileName.ToLowerInvariant();
        if (name.Contains("customers")) return ImportEntity.Customers;
        if (name.Contains("manufacturers")) return ImportEntity.Manufacturers;
        if (name.Contains("filament_colours") || name.Contains("filamentcolors")) return ImportEntity.FilamentColours;
        if (name.Contains("filament_types") || name.Contains("filamenttypes")) return ImportEntity.FilamentTypes;
        if (name.Contains("filaments")) return ImportEntity.Filaments;
        if (name.Contains("model_designs") || name.Contains("models")) return ImportEntity.ModelDesigns;
        if (name.Contains("printing_projects") || name.Contains("projects")) return ImportEntity.PrintingProjects;
        return ImportEntity.Unknown;
    }

    private ImportEntity ParseEntityName(string name)
    {
        return name.ToLowerInvariant() switch
        {
            "customers" or "customer" => ImportEntity.Customers,
            "manufacturers" or "manufacturer" => ImportEntity.Manufacturers,
            "filament_colours" or "filament_colours" or "filamentcolours" => ImportEntity.FilamentColours,
            "filament_types" or "filamenttypes" => ImportEntity.FilamentTypes,
            "filaments" or "filament" => ImportEntity.Filaments,
            "model_designs" or "modeldesigns" or "models" => ImportEntity.ModelDesigns,
            "printing_projects" or "printingprojects" or "projects" => ImportEntity.PrintingProjects,
            _ => ImportEntity.Unknown
        };
    }

    private async Task ProcessRowAsync(ImportEntity entity, IDictionary<string, object> dict, ImportResult result, bool updateExisting, CancellationToken ct)
    {
        // helper locals
        static string? GetString(IDictionary<string, object> d, string key)
        {
            foreach (var k in d.Keys)
            {
                if (string.Equals(k, key, StringComparison.OrdinalIgnoreCase))
                {
                    var v = d[k];
                    return v?.ToString()?.Trim();
                }
            }

            return null;
        }

        static int ParseIntOrZero(string? s)
        {
            return int.TryParse(s, out var i) ? i : 0;
        }

        static decimal ParseDecimalOrZero(string? s)
        {
            return decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? d : 0m;
        }

        static DateOnly ParseDateOnlyOrDefault(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return DateOnly.MinValue;
            if (DateOnly.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) return d;
            if (DateOnly.TryParse(s, out d)) return d;
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)) return DateOnly.FromDateTime(dt);
            return DateOnly.MinValue;
        }

        try
        {
            switch (entity)
            {
                case ImportEntity.Customers:
                {
                    var id = ParseIntOrZero(GetString(dict, "Id"));
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
                        var r = await _vm.EditCustomerAsync(dto).ConfigureAwait(false);
                        if (r == ValidationResult.Success) result.Updated++; else { result.Errors.Add(r.ErrorMessage ?? "EditCustomer failed"); result.Failed++; }
                    }
                    else
                    {
                        var r = await _vm.AddCustomerAsync(dto). ConfigureAwait(false);
                        if (r == ValidationResult.Success) result.Created++; else { result.Errors.Add(r.ErrorMessage ?? "AddCustomer failed"); result.Failed++; }
                    }

                    break;
                }

                case ImportEntity.Manufacturers:
                {
                    var id = ParseIntOrZero(GetString(dict, "Id"));
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
                        var r = await _vm.EditManufacturerAsync(dto).ConfigureAwait(false);
                        if (r == ValidationResult.Success) result.Updated++; else { result.Errors.Add(r.ErrorMessage ?? "EditManufacturer failed"); result.Failed++; }
                    }
                    else
                    {
                        var r = await _vm.AddManufacturerAsync(dto).ConfigureAwait(false);
                        if (r == ValidationResult.Success) result.Created++; else { result.Errors.Add(r.ErrorMessage ?? "AddManufacturer failed"); result.Failed++; }
                    }

                    break;
                }

                case ImportEntity.FilamentColours:
                {
                    var id = ParseIntOrZero(GetString(dict, "Id"));
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
                        var r = await _vm.EditFilamentColourAsync(dto).ConfigureAwait(false);
                        if (r == ValidationResult.Success) result.Updated++; else { result.Errors.Add(r.ErrorMessage ?? "EditFilamentColour failed"); result.Failed++; }
                    }
                    else
                    {
                        var r = await _vm.AddFilamentColourAsync(dto).ConfigureAwait(false);
                        if (r == ValidationResult.Success) result.Created++; else { result.Errors.Add(r.ErrorMessage ?? "AddFilamentColour failed"); result.Failed++; }
                    }

                    break;
                }

                case ImportEntity.FilamentTypes:
                {
                    var id = ParseIntOrZero(GetString(dict, "Id"));
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
                        var r = await _vm.EditFilamentTypeAsync(dto).ConfigureAwait(false);
                        if (r == ValidationResult.Success) result.Updated++; else { result.Errors.Add(r.ErrorMessage ?? "EditFilamentType failed"); result.Failed++; }
                    }
                    else
                    {
                        var r = await _vm.AddFilamentTypeAsync(dto).ConfigureAwait(false);
                        if (r == ValidationResult.Success) result.Created++; else { result.Errors.Add(r.ErrorMessage ?? "AddFilamentType failed"); result.Failed++; }
                    }

                    break;
                }

                case ImportEntity.Filaments:
                {
                    var id = ParseIntOrZero(GetString(dict, "Id"));
                    var cost = ParseDecimalOrZero(GetString(dict, "CostPerWeight") ?? GetString(dict, "Cost"));
                    var productId = GetString(dict, "ProductId");
                    var reorder = GetString(dict, "ReorderLink") ?? GetString(dict, "ReorderUrl") ?? GetString(dict, "Url");
                    var colourId = ParseIntOrZero(GetString(dict, "FilamentColourId") ?? GetString(dict, "FilamentColour") ?? GetString(dict, "ColourId"));
                    var typeId = ParseIntOrZero(GetString(dict, "FilamentTypeId") ?? GetString(dict, "FilamentType") ?? GetString(dict, "TypeId"));
                    var manId = ParseIntOrZero(GetString(dict, "ManufacturerId") ?? GetString(dict, "Manufacturer") ?? GetString(dict, "MakerId"));

                    var colourDto = new FilamentColourDto(colourId, string.Empty);
                    var typeDto = new FilamentTypeDto(typeId, string.Empty);
                    var manDto = new ManufacturerDto(manId, string.Empty);

                    var dto = id != 0
                        ? new FilamentDto(id, cost, productId, reorder, colourDto, typeDto, manDto)
                        : new FilamentDto(cost, productId, reorder, colourDto, typeDto, manDto);

                    if (id != 0 && updateExisting)
                    {
                        var r = await _vm.EditFilamentAsync(dto).ConfigureAwait(false);
                        if (r == ValidationResult.Success) result.Updated++; else { result.Errors.Add(r.ErrorMessage ?? "EditFilament failed"); result.Failed++; }
                    }
                    else
                    {
                        var r = await _vm.AddFilamentAsync(dto).ConfigureAwait(false);
                        if (r == ValidationResult.Success) result.Created++; else { result.Errors.Add(r.ErrorMessage ?? "AddFilament failed"); result.Failed++; }
                    }

                    break;
                }

                case ImportEntity.ModelDesigns:
                {
                    var id = ParseIntOrZero(GetString(dict, "Id"));
                    var desc = GetString(dict, "Description") ?? GetString(dict, "Name");
                    var length = ParseDecimalOrZero(GetString(dict, "Length"));
                    var summary = GetString(dict, "Summary");
                    var url = GetString(dict, "Url");

                    if (string.IsNullOrWhiteSpace(desc))
                    {
                        result.Errors.Add("ModelDesign: Description is required");
                        result.Failed++;
                        return;
                    }

                    var dto = id != 0 ? new ModelDesignDto(id, desc, length, summary ?? string.Empty, url) : new ModelDesignDto(desc, length, summary ?? string.Empty, url);
                    if (id != 0 && updateExisting)
                    {
                        var r = await _vm.EditModelDesignAsync(dto).ConfigureAwait(false);
                        if (r == ValidationResult.Success) result.Updated++; else { result.Errors.Add(r.ErrorMessage ?? "EditModelDesign failed"); result.Failed++; }
                    }
                    else
                    {
                        var r = await _vm.AddModelDesignAsync(dto).ConfigureAwait(false);
                        if (r == ValidationResult.Success) result.Created++; else { result.Errors.Add(r.ErrorMessage ?? "AddModelDesign failed"); result.Failed++; }
                    }

                    break;
                }

                case ImportEntity.PrintingProjects:
                {
                    var id = ParseIntOrZero(GetString(dict, "Id"));
                    var cost = ParseDecimalOrZero(GetString(dict, "Cost"));
                    var submitted = ParseDateOnlyOrDefault(GetString(dict, "Submitted"));
                    var completed = ParseDateOnlyOrDefault(GetString(dict, "Completed"));
                    var customerId = ParseIntOrZero(GetString(dict, "CustomerId") ?? GetString(dict, "Customer"));
                    var modelId = ParseIntOrZero(GetString(dict, "ModelDesignId") ?? GetString(dict, "ModelDesign"));
                    var filamentIdsRaw = GetString(dict, "FilamentIds") ?? GetString(dict, "FilamentId") ?? GetString(dict, "Filaments");

                    CustomerDto? custDto = null;
                    if (customerId != 0) custDto = new CustomerDto(customerId, string.Empty);

                    ModelDesignDto? modelDto = null;
                    if (modelId != 0) modelDto = new ModelDesignDto(modelId, string.Empty, 0m, string.Empty, null);

                    var filamentDtos = new List<FilamentDto>();
                    if (!string.IsNullOrWhiteSpace(filamentIdsRaw))
                    {
                        var parts = filamentIdsRaw.Split(new[] {',', ';'}, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var p in parts)
                        {
                            var fid = ParseIntOrZero(p.Trim());
                            if (fid == 0) continue;
                            // create minimal filament DTO with id only; AddPrintingProjectAsync will resolve existing entity
                            var placeholder = new FilamentDto(fid, 0m, null, null, new FilamentColourDto(0, string.Empty), new FilamentTypeDto(0, string.Empty), new ManufacturerDto(0, string.Empty));
                            filamentDtos.Add(placeholder);
                        }
                    }

                    var projDto = id != 0
                        ? new PrintingProjectDto(id, cost, submitted == DateOnly.MinValue ? DateOnly.FromDateTime(DateTime.Now) : submitted, completed == DateOnly.MinValue ? null : completed, custDto, modelDto, filamentDtos)
                        : new PrintingProjectDto(cost, submitted == DateOnly.MinValue ? DateOnly.FromDateTime(DateTime.Now) : submitted, completed == DateOnly.MinValue ? null : completed, custDto, modelDto, filamentDtos);

                    if (id != 0 && updateExisting)
                    {
                        var r = await _vm.EditPrintingProjectAsync(projDto).ConfigureAwait(false);
                        if (r == ValidationResult.Success) result.Updated++; else { result.Errors.Add(r.ErrorMessage ?? "EditPrintingProject failed"); result.Failed++; }
                    }
                    else
                    {
                        var r = await _vm.AddPrintingProjectAsync(projDto).ConfigureAwait(false);
                        if (r == ValidationResult.Success) result.Created++; else { result.Errors.Add(r.ErrorMessage ?? "AddPrintingProject failed"); result.Failed++; }
                    }

                    break;
                }

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
    }

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
}
