namespace gCodeJournal.ViewModel.DTOs;

/// <summary>
///     Represents a filament with various properties such as cost, product ID, reorder link, color, type, and
///     manufacturer.
/// </summary>
public class FilamentDto(
    int               id,
    decimal           costPerWeight,
    string?           productId,
    string?           reorderLink,
    FilamentColourDto filamentColour,
    FilamentTypeDto   filamentType,
    ManufacturerDto   manufacturer)
{
    #region Constructors
    /// <inheritdoc />
    public FilamentDto(
        decimal           costPerWeight,
        string?           productId,
        string?           reorderLink,
        FilamentColourDto filamentColour,
        FilamentTypeDto   filamentType,
        ManufacturerDto   manufacturer) : this(0, costPerWeight, productId, reorderLink, filamentColour, filamentType, manufacturer) {}
    #endregion

    #region Properties
    /// <summary>
    ///     The cost of the filament per unit weight.
    /// </summary>
    public decimal CostPerWeight {get; set;} = costPerWeight;

    /// <summary>
    ///     The color of the filament.
    /// </summary>
    public FilamentColourDto FilamentColour {get; set;} = filamentColour;

    /// <summary>
    ///     The type of the filament.
    /// </summary>
    public FilamentTypeDto FilamentType {get; set;} = filamentType;

    /// <summary>
    ///     The unique identifier of the filament.
    /// </summary>
    public int Id {get; init;} = id;

    /// <summary>
    ///     The manufacturer of the filament.
    /// </summary>
    public ManufacturerDto Manufacturer {get; set;} = manufacturer;

    /// <summary>
    ///     The product ID of the filament (optional).
    /// </summary>
    public string? ProductId {get; set;} = productId;

    /// <summary>
    ///     The link to reorder the filament (optional).
    /// </summary>
    public string? ReorderLink {get; set;} = reorderLink;
    #endregion

    /// <summary>
    ///     Returns a string representation of the filament, including its manufacturer, type, and color.
    /// </summary>
    /// <returns>A string in the format: "{Manufacturer} {FilamentType} ({FilamentColour})".</returns>
    public override string ToString() => $"{Manufacturer} {FilamentType} ({FilamentColour})";
}