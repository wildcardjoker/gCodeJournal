// gCodeJournal.ViewModel

namespace gCodeJournal.ViewModel.Import.Maps;

#region Using Directives
using CsvHelper;
using CsvHelper.Configuration;
using DTOs;
#endregion

/// <summary>
///     CSV map for printers.csv
///     Expects columns: Id (optional), ManufacturerId/Manufacturer, Model
/// </summary>
public sealed class PrinterMap : ClassMap<PrinterDto>
{
    #region Constructors
    public PrinterMap()
    {
        Map(m => m.Id).Name("Id").Optional();

        Map(m => m.Manufacturer)
            .Convert(args =>
                     {
                         var row   = args.Row;
                         var field = TryGetField(row, "ManufacturerId") ?? TryGetField(row, "Manufacturer");
                         if (string.IsNullOrWhiteSpace(field))
                         {
                             return null;
                         }

                         var id = ParseIntOrZero(field);

                         return id != 0 ? new ManufacturerDto(id, string.Empty) : new ManufacturerDto(field.Trim());
                     });

        Map(m => m.Model).Name("Model", "PrinterModel");
    }
    #endregion

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