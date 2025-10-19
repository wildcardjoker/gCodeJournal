namespace gCodeJournal.Model;

#region Using Directives
using System.ComponentModel.DataAnnotations;
#endregion

/// <summary>
///     Represents a customer who requests printing projects.
/// </summary>
/// <remarks>
///     A <see cref="Customer" /> contains an identifier, a display name and a collection of associated
///     <see cref="PrintingProject" /> instances. The properties are intended to be non-null when the
///     object is in a valid state within the application.
/// </remarks>
public class Customer
{
    #region Properties
    /// <summary>
    ///     Gets or sets the unique identifier for the customer.
    /// </summary>
    /// <value>An integer representing the primary key for the customer.</value>
    public int Id {get; set;}

    /// <summary>
    ///     Gets or sets the customer's display name.
    /// </summary>
    /// <value>A non-null string containing the customer's name.</value>
    [StringLength(100)]
    public string Name {get; set;} = null!;

    /// <summary>
    ///     Gets or sets the collection of printing projects associated with the customer.
    /// </summary>
    /// <value>
    ///     A collection of <see cref="PrintingProject" /> instances related to this customer.
    ///     The property is expected to be non-null when the entity is in a valid state.
    /// </value>
    public ICollection<PrintingProject> Projects {get; set;} = null!;
    #endregion
}