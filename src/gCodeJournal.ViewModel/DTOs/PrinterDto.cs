namespace gCodeJournal.ViewModel.DTOs;

/// <summary>
///     Represents a 3D printer DTO with an optional manufacturer reference.
/// </summary>
public class PrinterDto(int id, ManufacturerDto? manufacturer, string model)
{
    #region Constructors
    public PrinterDto() : this(0, null, string.Empty) {}

    public PrinterDto(int id, string model) : this(id, null, model) {}

    public PrinterDto(ManufacturerDto manufacturer, string model) : this(0, manufacturer, model) {}
    #endregion

    #region Properties
    public int Id {get; init;} = id;

    public ManufacturerDto? Manufacturer {get; set;} = manufacturer;

    public string Model {get; set;} = model;
    #endregion

    public override string ToString() => Model;
}
