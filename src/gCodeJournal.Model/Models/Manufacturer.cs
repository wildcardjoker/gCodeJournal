// gCodeJournal.Model

namespace gCodeJournal.Model;

#region Using Directives
using System.ComponentModel.DataAnnotations;
#endregion

/// <summary>
///     Represents a manufacturer of 3D printing filament.
/// </summary>
/// <remarks>
///     Instances of this class are used as a lookup/reference entity for
///     <see cref="Filament" /> records. Typical values include vendor or
///     brand names such as "Prusament" or "Hatchbox".
/// </remarks>
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
    ///     A human-readable name for the manufacturer (for example, "Prusa" or "Hatchbox").
    /// </summary>
    [StringLength(30)]
    public string Name {get; set;} = null!;
    #endregion

    #region Overrides of Object
    /// <inheritdoc />
    public override string ToString() => Name;
    #endregion
}