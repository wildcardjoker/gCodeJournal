// gCodeJournal.Model

namespace gCodeJournal.Model;

#region Using Directives
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

/// <summary>
///     Represents a 3D printer, including its manufacturer and model information.
/// </summary>
/// <remarks>
///     This class is used to store details about a specific 3D printer, such as its
///     manufacturer and model. It can be associated with other entities in the system
///     to track printer-specific data.
/// </remarks>
public class Printer
{
    #region Properties
    /// <summary>
    ///     Gets or sets the unique identifier for the printer.
    /// </summary>
    /// <value>
    ///     An integer representing the unique identifier of the printer.
    /// </value>
    /// <remarks>
    ///     This property is used as the primary key for the <see cref="Printer" /> entity.
    /// </remarks>
    public int Id {get; set;}

    /// <summary>
    ///     Gets or sets the manufacturer of the 3D printer.
    /// </summary>
    /// <remarks>
    ///     This property represents the manufacturer associated with the 3D printer.
    ///     It provides a reference to the <see cref="Manufacturer" /> entity, which contains
    ///     details about the vendor or brand of the printer.
    /// </remarks>
    public virtual Manufacturer Manufacturer {get; set;} = null!;

    /// <summary>
    ///     Gets or sets the identifier of the manufacturer associated with the printer.
    /// </summary>
    /// <remarks>
    ///     This property serves as a foreign key linking the printer to its corresponding
    ///     <see cref="Manufacturer" /> entity. It is used to establish the relationship
    ///     between the printer and its manufacturer in the database.
    /// </remarks>
    public int ManufacturerId {get; set;}

    /// <summary>
    ///     Gets or sets the labour cost per hour for using this printer.
    /// </summary>
    /// <remarks>
    ///     This value is stored as a currency/money value in the database.
    /// </remarks>
    [Column(TypeName = "money")]
    public decimal CostPerHour { get; set; } = 0m;

    /// <summary>
    ///     Gets or sets the model name of the 3D printer.
    /// </summary>
    /// <remarks>
    ///     This property represents the specific model of the 3D printer, such as "Prusa i3 MK3S" or "Ender 3".
    ///     It is limited to a maximum length of 50 characters.
    /// </remarks>
    [StringLength(50)]
    public string Model {get; set;} = null!;
    #endregion
}