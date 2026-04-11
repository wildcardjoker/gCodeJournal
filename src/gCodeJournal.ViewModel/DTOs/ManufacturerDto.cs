// gCodeJournal.ViewModel

namespace gCodeJournal.ViewModel.DTOs;

#region Using Directives
using Model;
#endregion

/// <summary>
///     Represents a manufacturer with an ID and a name.
/// </summary>
public class ManufacturerDto(int id, string name, bool isFilamentManufacturer = false, bool isPrinterManufacturer = false)
{
    #region Constructors
    public ManufacturerDto() : this(0, "") {}

    // Explicit two-argument constructor avoids optional-argument expansion in expression trees
    public ManufacturerDto(int id, string name) : this(id, name, false) {}

    /// <inheritdoc />
    public ManufacturerDto(string name) : this(0, name) {}

    public ManufacturerDto(Manufacturer manufacturer) : this(
        manufacturer.Id,
        manufacturer.Name,
        manufacturer.IsFilamentManufacturer,
        manufacturer.IsPrinterManufacturer) {}
    #endregion

    #region Properties
    /// <summary>
    ///     The unique identifier of the manufacturer.
    /// </summary>
    public int Id {get; init;} = id;

    /// <summary>
    ///     Gets a value indicating whether the manufacturer produces filament.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the manufacturer is a filament producer; otherwise, <c>false</c>.
    /// </value>
    public bool IsFilamentManufacturer {get; init;} = isFilamentManufacturer;

    /// <summary>
    ///     Gets a value indicating whether the manufacturer produces 3D printers.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the manufacturer produces 3D printers; otherwise, <c>false</c>.
    /// </value>
    public bool IsPrinterManufacturer {get; init;} = isPrinterManufacturer;

    /// <summary>
    ///     The name of the manufacturer.
    /// </summary>
    public string Name {get; set;} = name;
    #endregion

    /// <summary>
    ///     Returns a string representation of the manufacturer.
    /// </summary>
    /// <returns>The name of the manufacturer.</returns>
    public override string ToString() => Name;
}