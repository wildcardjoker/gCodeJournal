// gCodeJournal.Model

namespace gCodeJournal.Model;

#region Using Directives
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
#endregion

/// <summary>
///     Represents a colour used for 3D printing filament.
/// </summary>
/// <remarks>
///     Instances of this class are used as a lookup/reference entity for filament
///     records (<see cref="Filament" />) and filament type definitions
///     (<see cref="FilamentType" />). Typical values include human-readable names
///     such as "Matte Black" or "Translucent Blue".
/// </remarks>
[UsedImplicitly]
public class FilamentColour
{
    #region Properties
    /// <summary>
    ///     A human-readable description of this filament colour (for example, "Matte Black" or "Translucent Blue").
    /// </summary>
    [StringLength(100)]
    public string Description {get; set;} = null!;

    /// <summary>
    ///     Navigation property: the filaments that use this colour.
    ///     This collection represents zero or more <see cref="Filament" /> instances associated with this colour.
    /// </summary>
    public virtual ICollection<Filament> Filaments {get; set;} = null!;

    /// <summary>
    ///     Primary key identifier for this filament colour record.
    /// </summary>
    public int Id {get; set;}
    #endregion

    #region Overrides of Object
    /// <inheritdoc />
    public override string ToString() => Description;
    #endregion
}