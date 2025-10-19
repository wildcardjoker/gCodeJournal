// gCodeJournal.Model

namespace gCodeJournal.Model;

#region Using Directives
using System.ComponentModel.DataAnnotations;
#endregion

/// <summary>
///     Represents a length or spool of 3D printing filament, including its
///     type, colour, manufacturer and cost information.
/// </summary>
public class Filament
{
    #region Properties
    /// <summary>
    ///     The colour information for this filament.
    ///     This is a navigation property that references a <see cref="FilamentColour" />.
    ///     The corresponding foreign key is <see cref="FilamentColourId" />.
    /// </summary>
    public FilamentColour Colour {get; set;} = null!;

    /// <summary>
    ///     The cost of the filament per unit weight.
    ///     Uses <see cref="decimal" /> to preserve precision for currency values.
    ///     The unit of weight (e.g., grams, kilograms) should be determined by the application.
    /// </summary>
    public decimal CostPerWeight {get; set;}

    /// <summary>
    ///     Foreign key identifier for the <see cref="Colour" /> property.
    /// </summary>
    public int FilamentColourId {get; set;}

    /// <summary>
    ///     Foreign key identifier for the <see cref="Type" /> property.
    /// </summary>
    public int FilamentTypeId {get; set;}

    /// <summary>
    ///     Primary key identifier for this filament record.
    /// </summary>
    public int Id {get; set;}

    /// <summary>
    ///     The manufacturer that produced this filament.
    ///     This is a navigation property that references a <see cref="Manufacturer" />.
    ///     The corresponding foreign key is <see cref="ManufacturerId" />.
    /// </summary>
    public Manufacturer Manufacturer {get; set;} = null!;

    /// <summary>
    ///     Foreign key identifier for the <see cref="Manufacturer" /> property.
    /// </summary>
    public int ManufacturerId {get; set;}

    /// <summary>
    ///     Gets or sets the unique identifier for the product associated with this filament.
    /// </summary>
    /// <remarks>
    ///     This property is used to link the filament to a specific product in the inventory
    ///     or catalog. It serves as a reference for tracking and managing product details.
    /// </remarks>
    [StringLength(20)]
    public string? ProductId {get; set;}

    /// <summary>
    ///     Gets or sets the URL link to reorder this filament.
    /// </summary>
    /// <remarks>
    ///     This property contains a hyperlink (up to 2083 characters) that directs users
    ///     to a webpage where they can reorder the specific filament. It is intended to
    ///     simplify the process of restocking filament by providing a direct reference
    ///     to the product.
    /// </remarks>
    /// <value>
    ///     A string representing the URL for reordering the filament.
    /// </value>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
    ///     Thrown if the length of the URL exceeds 2083 characters.
    /// </exception>
    [MaxLength(2083)]
    public string? ReorderLink {get; set;} = null!;

    /// <summary>
    ///     The filament material type (for example, PLA, ABS).
    ///     This is a navigation property that references a <see cref="FilamentType" />.
    ///     The corresponding foreign key is <see cref="FilamentTypeId" />.
    /// </summary>
    public FilamentType Type {get; set;} = null!;
    #endregion

    #region Overrides of Object
    /// <inheritdoc />
    public override string ToString() => $"{Manufacturer} {Type} ({Colour})";
    #endregion
}