namespace gCodeJournal.ViewModel.Import.Maps;

#region Using Directives
using CsvHelper.Configuration;
using CsvHelper;
using DTOs;
using System;
using System.Globalization;
#endregion

/// <summary>
/// CSV class map for mapping filament CSV rows to <see cref="FilamentDto"/> instances.
/// Expects columns: Id, CostPerWeight, ProductId, ReorderLink, ManufacturerId, FilamentTypeId, FilamentColourId
/// </summary>
public sealed class FilamentMap : ClassMap<FilamentDto>
{
    public FilamentMap()
    {
        // No explicit ConstructUsing - rely on parameterless ctor on FilamentDto and property mappings

        Map(m => m.Id).Name("Id");
        Map(m => m.CostPerWeight).Name("CostPerWeight");
        Map(m => m.ProductId).Name("ProductId");
        Map(m => m.ReorderLink).Name("ReorderLink");

        // Map nested DTOs by converting id fields into minimal DTO instances (id only).
        Map(m => m.Manufacturer).Convert(args =>
        {
            var field = TryGetField(args.Row, "ManufacturerId") ?? TryGetField(args.Row, "Manufacturer");
            return new ManufacturerDto(ParseIntOrZero(field), string.Empty);
        });

        Map(m => m.FilamentType).Convert(args =>
        {
            var field = TryGetField(args.Row, "FilamentTypeId") ?? TryGetField(args.Row, "FilamentType");
            return new FilamentTypeDto(ParseIntOrZero(field), string.Empty);
        });

        Map(m => m.FilamentColour).Convert(args =>
        {
            var field = TryGetField(args.Row, "FilamentColourId") ?? TryGetField(args.Row, "FilamentColour") ?? TryGetField(args.Row, "ColourId");
            return new FilamentColourDto(ParseIntOrZero(field), string.Empty);
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
