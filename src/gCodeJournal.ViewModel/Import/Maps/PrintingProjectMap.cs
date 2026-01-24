namespace gCodeJournal.ViewModel.Import.Maps;

#region Using Directives
using CsvHelper.Configuration;
using CsvHelper;
using DTOs;
using System;
using System.Globalization;
using System.Collections.Generic;
#endregion

/// <summary>
/// CSV class map for mapping printing project CSV rows to <see cref="PrintingProjectDto"/> instances.
/// Expects columns: Id, Cost, Submitted, Completed, CustomerId, ModelDesignId, FilamentIds
/// FilamentIds is expected to be a comma/semicolon separated list of filament ids.
/// </summary>
public sealed class PrintingProjectMap : ClassMap<PrintingProjectDto>
{
    public PrintingProjectMap()
    {
        Map(m => m.Id).Name("Id");
        // Ignore Cost in CSV — cost will be calculated or provided elsewhere, do not map from CSV
        Map(m => m.Cost).Ignore();
        // Accept multiple specific date formats, preferring ISO then common locale formats.
        // CsvHelper will try formats in the order provided when converting string -> DateOnly.
        Map(m => m.Submitted).Name("Submitted").TypeConverterOption.Format("yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy");
        Map(m => m.Completed).Name("Completed").TypeConverterOption.Format("yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy");

        // Convert customer/model ids to minimal DTOs
        Map(m => m.Customer).Convert(args =>
        {
            var row = args.Row;
            var field = TryGetField(row, "CustomerId") ?? TryGetField(row, "Customer");
            var id = ParseIntOrZero(field);
            return id != 0 ? new CustomerDto(id, string.Empty) : null;
        });

        Map(m => m.ModelDesign).Convert(args =>
        {
            var row = args.Row;
            var field = TryGetField(row, "ModelDesignId") ?? TryGetField(row, "ModelDesign");
            var id = ParseIntOrZero(field);
            return id != 0 ? new ModelDesignDto(id, string.Empty, 0m, string.Empty, null) : null;
        });

        Map(m => m.Filaments).Convert(args =>
        {
            var row = args.Row;
            var raw = TryGetField(row, "FilamentIds") ?? TryGetField(row, "FilamentId") ?? TryGetField(row, "Filaments");
            var list = new List<FilamentDto>();
            if (string.IsNullOrWhiteSpace(raw)) return list;
            var parts = raw.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
            {
                var fid = ParseIntOrZero(p.Trim());
                if (fid == 0) continue;
                list.Add(new FilamentDto(fid, 0m, null, null, new FilamentColourDto(0, string.Empty), new FilamentTypeDto(0, string.Empty), new ManufacturerDto(0, string.Empty)));
            }

            return list;
        });
    }

    private static string? TryGetField(IReaderRow row, string name)
    {
        try
        {
            if (row == null) return null;
            return row.TryGetField(name, out string? value) ? value : null;
        }
        catch
        {
            return null;
        }
    }

    private static int ParseIntOrZero(string? s) => int.TryParse(s, out var i) ? i : 0;

    private static decimal ParseDecimalOrZero(string? s) => decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? d : 0m;
}
