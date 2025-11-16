// gCodeJournal.Model

namespace gCodeJournal.Model;

#region Using Directives
using System.Collections.Generic;
using JetBrains.Annotations;
#endregion

/// <summary>
///     Represents a single printing job requested by a customer.
/// </summary>
/// <remarks>
///     A <see cref="PrintingProject" /> aggregates metadata about an individual print request,
///     including the requesting <see cref="Customer" />, the design to print (<see cref="Model" />),
///     the filament(s) used, cost information and timestamps for submission and completion.
///     Instances of this type are typically used as an entity within the application's persistence model.
/// </remarks>
[UsedImplicitly]
public class PrintingProject
{
    #region Properties
    /// <summary>
    ///     Gets or sets the date and time when the printing project was completed.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTime" /> representing when the print finished, or <c>null</c> if the project
    ///     has not yet been completed.
    /// </value>
    public DateTime? Completed {get; set;}

    /// <summary>
    ///     Gets or sets the total cost charged for this printing project.
    /// </summary>
    /// <value>
    ///     A <see cref="decimal" /> value representing the monetary cost. Uses <see cref="decimal" />
    ///     to preserve precision for currency amounts.
    /// </value>
    public decimal Cost {get; set;}

    /// <summary>
    ///     Navigation property: the customer who requested the printing project.
    /// </summary>
    /// <value>
    ///     A non-null <see cref="Customer" /> instance when the entity is in a valid state.
    ///     The corresponding foreign key is <see cref="CustomerId" />.
    /// </value>
    public virtual Customer Customer {get; set;} = null!;

    /// <summary>
    ///     Foreign key identifier for the <see cref="Customer" /> who requested the project.
    /// </summary>
    /// <value>An integer representing the customer's primary key.</value>
    public int CustomerId {get; set;}

    /// <summary>
    ///     Navigation property: collection of filament records associated with this project.
    /// </summary>
    /// <value>
    ///     A collection of <see cref="Filament" /> instances; expected to be non-null when the entity
    ///     is in a valid state (can be empty if no specific filaments are recorded).
    /// </value>
    public virtual ICollection<Filament> Filaments {get; set;} = new List<Filament>();

    /// <summary>
    ///     Primary key identifier for the printing project.
    /// </summary>
    /// <value>An integer representing the project's primary key.</value>
    public int Id {get; set;}

    /// <summary>
    ///     Navigation property: the design or model that is being printed.
    /// </summary>
    /// <value>
    ///     A non-null <see cref="ModelDesign" /> instance describing the design metadata.
    ///     The corresponding foreign key is <see cref="ModelDesignId" />.
    /// </value>
    public virtual ModelDesign Model {get; set;} = null!;

    /// <summary>
    ///     Foreign key identifier for the <see cref="Model" />.
    /// </summary>
    /// <value>An integer representing the model design's primary key.</value>
    public int ModelDesignId {get; set;}

    /// <summary>
    ///     Gets or sets the date and time when the project was submitted.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTime" /> indicating the submission time. Defaults to the current date/time
    ///     when a new <see cref="PrintingProject" /> instance is created.
    /// </value>
    public DateTime Submitted {get; set;} = DateTime.Now;
    #endregion
}