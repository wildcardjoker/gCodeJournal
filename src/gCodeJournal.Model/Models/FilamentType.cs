// gCodeJournal.Model

namespace gCodeJournal.Model;

#region Using Directives
#region Using Directives
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
#endregion
#endregion

/// <summary>
///     Represents a material/type of 3D printing filament (for example, PLA, ABS).
/// </summary>
/// <remarks>
///     Instances of this class are used as lookup/reference entities for individual
///     <see cref="Filament" /> records and to group available <see cref="FilamentColour" />
///     options for that material type.
/// </remarks>
[UsedImplicitly]
public class FilamentType
{
    #region Properties
    /// <summary>
    ///     A human-readable description or name for this filament type (for example, "PLA" or "PETG").
    /// </summary>
    [StringLength(15)]
    public string Description {get; set;} = null!;

    /// <summary>
    ///     Navigation property: the filaments that use this filament type.
    /// </summary>
    /// <remarks>
    ///     Represents the set of <see cref="Filament" /> instances which reference this
    ///     filament type (for example, individual spools or purchases).
    /// </remarks>
    public virtual ICollection<Filament> Filaments {get; set;} = null!;

    /// <summary>
    ///     Primary key identifier for this filament type record.
    /// </summary>
    public int Id {get; set;}
    #endregion

    #region Overrides of Object
    /// <inheritdoc />
    public override string ToString() => Description;
    #endregion
}