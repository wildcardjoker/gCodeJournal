// gCodeJournal.Model

namespace gCodeJournal.Model;

#region Using Directives
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
#endregion

/// <summary>
///     Represents a manufacturer of 3D printing filament.
/// </summary>
/// <remarks>
///     Instances of this class are used as a lookup/reference entity for
///     <see cref="Filament" /> records. Typical values include vendor or
///     brand names such as "Prusament" or "Hatchbox".
/// </remarks>
[UsedImplicitly]
public class Manufacturer
{
    #region Properties
    /// <summary>
    ///     Navigation property: the filaments produced by this manufacturer.
    ///     This collection represents zero or more <see cref="Filament" />
    ///     instances associated with the manufacturer.
    /// </summary>
    public virtual ICollection<Filament> Filaments {get; set;} = null!;

    /// <summary>
    ///     Primary key identifier for this manufacturer record.
    /// </summary>
    public int Id {get; set;}

    /// <summary>
    ///     Gets or sets a value indicating whether the manufacturer produces 3D printing filament.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the manufacturer produces filament; otherwise, <c>false</c>.
    /// </value>
    public bool IsFilamentManufacturer {get; set;}

    /// <summary>
    ///     Gets or sets a value indicating whether the manufacturer produces 3D printers.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the manufacturer produces 3D printers; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     This property helps distinguish manufacturers that produce 3D printers
    ///     from those that exclusively produce other products, such as 3D printing filament.
    /// </remarks>
    public bool IsPrinterManufacturer {get; set;}

    /// <summary>
    ///     A human-readable name for the manufacturer (for example, "Prusa" or "Hatchbox").
    /// </summary>
    [StringLength(30)]
    public string Name {get; set;} = null!;

    /// <summary>
    ///     Gets or sets the collection of 3D printers associated with this manufacturer.
    /// </summary>
    /// <remarks>
    ///     This property represents the printers produced by the manufacturer. Each printer
    ///     includes details such as its model and manufacturer information.
    /// </remarks>
    public virtual ICollection<Printer> Printers {get; set;} = null!;
    #endregion

    #region Overrides of Object
    /// <inheritdoc />
    public override string ToString() => Name;
    #endregion
}