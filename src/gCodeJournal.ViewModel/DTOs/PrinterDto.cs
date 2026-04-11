// gCodeJournal.ViewModel

namespace gCodeJournal.ViewModel.DTOs;

#region Using Directives
using Model;
#endregion

/// <summary>
///     Represents a 3D printer DTO with an optional manufacturer reference.
/// </summary>
public class PrinterDto(int id, ManufacturerDto? manufacturer, string model, decimal costPerHour)
{
    #region Constructors
    public PrinterDto() : this(0, null, string.Empty, 0m) {}

    public PrinterDto(int id, string model) : this(id, null, model, 0m) {}

    public PrinterDto(ManufacturerDto manufacturer, string model, decimal costPerHour) : this(0, manufacturer, model, costPerHour) {}
    public PrinterDto(Printer         printer) : this(printer.Id, new ManufacturerDto(printer.Manufacturer), printer.Model, printer.CostPerHour) {}
    #endregion

    #region Properties
    public decimal          CostPerHour  {get; set;}  = costPerHour;
    public int              Id           {get; init;} = id;
    public ManufacturerDto? Manufacturer {get; set;}  = manufacturer;

    public string Model {get; set;} = model;
    #endregion

    public override string ToString() => $"{Manufacturer?.Name} {Model}";
}