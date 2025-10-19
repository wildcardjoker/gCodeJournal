// gCodeJournal.Model

namespace gCodeJournal.Model;

#region Using Directives
using System.ComponentModel.DataAnnotations;
#endregion

/// <summary>
///     Represents a design entry within the gCodeJournal model.
///     Contains identifying information and descriptive metadata for a design.
/// </summary>
/// <remarks>
///     Instances of this type are used to convey basic searchable and display
///     information about a design, including identifiers, textual descriptions,
///     and an optional external link.
/// </remarks>
public class ModelDesign
{
    #region Properties
    /// <summary>
    ///     Gets or sets a detailed description of the design.
    /// </summary>
    /// <value>A <see cref="string" /> containing extended descriptive text or notes.</value>
    [StringLength(5000)]
    public string Description {get; set;} = null!;

    /// <summary>
    ///     Gets or sets the unique identifier for the design.
    /// </summary>
    /// <value>An <see cref="int" /> that uniquely identifies the design.</value>
    public int Id {get; set;}

    /// <summary>
    ///     Gets or sets the length associated with the design.
    /// </summary>
    /// <value>A <see cref="decimal" /> representing the length value (units are context-dependent).</value>
    public decimal Length {get; set;}

    /// <summary>
    ///     Gets or sets a short summary of the design.
    /// </summary>
    /// <value>A concise <see cref="string" /> summarizing the design.</value>
    [StringLength(100)]
    public string Summary {get; set;} = null!;

    /// <summary>
    ///     Gets or sets a URL linking to additional information or resources for the design.
    /// </summary>
    /// <value>A <see cref="string" /> containing an absolute or relative URL.</value>
    [StringLength(2038)]
    public string? Url {get; set;}
    #endregion
}