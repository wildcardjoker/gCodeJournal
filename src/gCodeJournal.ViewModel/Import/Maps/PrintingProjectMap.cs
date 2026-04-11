// gCodeJournal.ViewModel

namespace gCodeJournal.ViewModel.Import.Maps;

#region Using Directives
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using DTOs;
#endregion

/// <summary>
///     CSV class map for mapping printing project CSV rows to <see cref="PrintingProjectDto" /> instances.
///     Expects columns: Id, Cost, Submitted, Completed, CustomerId, ModelDesignId, FilamentIds
///     FilamentIds is expected to be a comma/semicolon separated list of filament ids.
/// </summary>
public sealed class PrintingProjectMap : ClassMap<PrintingProjectDto>
{
    #region Constructors
    public PrintingProjectMap()
    {
        Map(m => m.Id).Name("Id").Optional();

        // Ignore Cost in CSV — cost will be calculated or provided elsewhere, do not map from CSV
        Map(m => m.Cost).Ignore();

        // Accept multiple specific date formats, preferring ISO then common locale formats.
        // CsvHelper will try formats in the order provided when converting string -> DateOnly.
        Map(m => m.Submitted).Name("Submitted").TypeConverterOption.Format("yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy");
        Map(m => m.Completed).Name("Completed").TypeConverterOption.Format("yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy");

        // Convert customer/model ids to minimal DTOs
        Map(m => m.Customer)
            .Convert(args =>
                     {
                         var row   = args.Row;
                         var field = TryGetField(row, "CustomerId") ?? TryGetField(row, "Customer");
                         if (string.IsNullOrWhiteSpace(field))
                         {
                             return null;
                         }

                         var id = ParseIntOrZero(field);
                         if (id != 0)
                         {
                             return new CustomerDto(id, string.Empty);
                         }

                         // treat non-numeric value as customer name
                         return new CustomerDto(field.Trim());
                     });

        Map(m => m.ModelDesign)
            .Convert(args =>
                     {
                         var row   = args.Row;
                         var field = TryGetField(row, "ModelDesignId") ?? TryGetField(row, "Model Design") ?? TryGetField(row, "ModelDesign");
                         if (string.IsNullOrWhiteSpace(field))
                         {
                             return null;
                         }

                         var id = ParseIntOrZero(field);
                         if (id != 0)
                         {
                             return new ModelDesignDto(id, string.Empty, 0m, string.Empty, null);
                         }

                         // treat non-numeric value as the model summary
                         return new ModelDesignDto(0, string.Empty, 0m, field.Trim(), null);
                     });

        Map(m => m.Printer)
            .Convert(args =>
                     {
                         var row = args.Row;

                         // Prefer explicit id if present
                         var idField = TryGetField(row, "PrinterId");
                         if (!string.IsNullOrWhiteSpace(idField))
                         {
                             var id = ParseIntOrZero(idField);
                             if (id != 0)
                             {
                                 return new PrinterDto(id, string.Empty);
                             }
                         }

                         // If provided as separate Manufacturer + Model columns, prefer those
                         var manuField  = TryGetField(row, "PrinterManufacturer") ?? TryGetField(row, "PrinterMaker") ?? TryGetField(row, "PrinterBrand");
                         var modelField = TryGetField(row, "PrinterModel");
                         _ = decimal.TryParse(TryGetField(row, "CostPerHour"), out var costPerHour);
                         if (!string.IsNullOrWhiteSpace(manuField) || !string.IsNullOrWhiteSpace(modelField))
                         {
                             var manu  = new ManufacturerDto(0, (manuField ?? string.Empty).Trim());
                             var model = (modelField ?? string.Empty).Trim();

                             return new PrinterDto(manu, model, costPerHour);
                         }

                         // Fallback to single 'Printer' column (may be id or model)
                         var printerIdField = TryGetField(row, "Printer");
                         if (string.IsNullOrWhiteSpace(printerIdField))
                         {
                             return null;
                         }

                         var printerId = ParseIntOrZero(printerIdField);
                         if (printerId != 0)
                         {
                             return new PrinterDto(printerId, null, string.Empty, costPerHour);
                         }

                         return new PrinterDto(0, null, printerIdField.Trim(), costPerHour);
                     });

        Map(m => m.Filaments)
            .Convert(args =>
                     {
                         var row  = args.Row;
                         var raw  = TryGetField(row, "FilamentIds") ?? TryGetField(row, "FilamentId") ?? TryGetField(row, "Filaments");
                         var list = new List<FilamentDto>();
                         if (string.IsNullOrWhiteSpace(raw))
                         {
                             return list;
                         }

                         // support two formats: numeric ids or sets Manufacturer|Type|Colour separated by ';'
                         var parts = raw.Split(new[] {',', ';'}, StringSplitOptions.RemoveEmptyEntries);
                         foreach (var p in parts)
                         {
                             var token = p.Trim();
                             if (string.IsNullOrEmpty(token))
                             {
                                 continue;
                             }

                             // Manufacturer|Type|Colour format
                             if (token.Contains('|'))
                             {
                                 var set = token.Split('|', StringSplitOptions.TrimEntries);
                                 if (set.Length >= 3)
                                 {
                                     var man  = new ManufacturerDto(0, set[0].Trim());
                                     var type = new FilamentTypeDto(0, set[1].Trim());
                                     var col  = new FilamentColourDto(0, set[2].Trim());
                                     list.Add(new FilamentDto(0, 0m, null, null, col, type, man));
                                 }

                                 // ignore malformed set
                                 continue;
                             }

                             // numeric id
                             var fid = ParseIntOrZero(token);
                             if (fid == 0)
                             {
                                 continue;
                             }

                             list.Add(
                                 new FilamentDto(
                                     fid,
                                     0m,
                                     null,
                                     null,
                                     new FilamentColourDto(0, string.Empty),
                                     new FilamentTypeDto(0, string.Empty),
                                     new ManufacturerDto(0, string.Empty)));
                         }

                         return list;
                     });
    }
    #endregion

    static decimal ParseDecimalOrZero(string? s) => decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? d : 0m;

    static int ParseIntOrZero(string? s) => int.TryParse(s, out var i) ? i : 0;

    static string? TryGetField(IReaderRow row, string name)
    {
        try
        {
            if (row == null)
            {
                return null;
            }

            return row.TryGetField(name, out string? value) ? value : null;
        }
        catch
        {
            return null;
        }
    }
}