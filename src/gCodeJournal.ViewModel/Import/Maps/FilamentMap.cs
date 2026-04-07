// gCodeJournal.ViewModel

namespace gCodeJournal.ViewModel.Import.Maps;

#region Using Directives
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using DTOs;
#endregion

/// <summary>
///     CSV class map for mapping filament CSV rows to <see cref="FilamentDto" /> instances.
///     Expects columns: Id, CostPerWeight, ProductId, ReorderLink, ManufacturerId, FilamentTypeId, FilamentColourId
/// </summary>
public sealed class FilamentMap : ClassMap<FilamentDto>
{
    #region Constructors
    public FilamentMap()
    {
        // No explicit ConstructUsing - rely on parameterless ctor on FilamentDto and property mappings

        Map(m => m.Id).Name("Id");
        Map(m => m.CostPerWeight).Name("CostPerWeight");
        Map(m => m.ProductId).Name("ProductId");
        Map(m => m.ReorderLink).Name("ReorderLink");

        // Map nested DTOs by converting id fields into minimal DTO instances (id only).
        Map(m => m.Manufacturer)
            .Convert(args =>
                     {
                         var field = TryGetField(args.Row, "ManufacturerId") ?? TryGetField(args.Row, "Manufacturer");
                         if (int.TryParse(field, out var id) && id != 0)
                         {
                             return new ManufacturerDto(id, string.Empty);
                         }

                         // treat non-numeric value as manufacturer name
                         return new ManufacturerDto(0, field?.Trim() ?? string.Empty);
                     });

        Map(m => m.FilamentType)
            .Convert(args =>
                     {
                         var field = TryGetField(args.Row, "FilamentTypeId") ?? TryGetField(args.Row, "FilamentType") ?? TryGetField(args.Row, "Filament Type");
                         if (int.TryParse(field, out var id) && id != 0)
                         {
                             return new FilamentTypeDto(id, string.Empty);
                         }

                         // treat non-numeric value as filament type description
                         return new FilamentTypeDto(0, field?.Trim() ?? string.Empty);
                     });

        Map(m => m.FilamentColour)
            .Convert(args =>
                     {
                         var field =
                             TryGetField(args.Row, "FilamentColourId")
                             ?? TryGetField(args.Row, "FilamentColour") ?? TryGetField(args.Row, "Filament Colour") ?? TryGetField(args.Row, "ColourId");
                         if (int.TryParse(field, out var id) && id != 0)
                         {
                             return new FilamentColourDto(id, string.Empty);
                         }

                         // treat non-numeric value as filament colour description
                         return new FilamentColourDto(0, field?.Trim() ?? string.Empty);
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