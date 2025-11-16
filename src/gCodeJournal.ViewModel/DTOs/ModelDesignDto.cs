namespace gCodeJournal.ViewModel.DTOs;

/// <summary>
///     Represents a model design with properties such as description, length, summary, and URL.
/// </summary>
public class ModelDesignDto(int id, string description, decimal length, string summary, string? url)
{
    #region Constructors
    /// <inheritdoc />
    public ModelDesignDto(string description, decimal length, string summary, string? url) : this(0, description, length, summary, url) {}
    #endregion

    #region Properties
    /// <summary>
    ///     The description of the model design.
    /// </summary>
    public string Description {get; set;} = description;

    /// <summary>
    ///     The unique identifier of the model design.
    /// </summary>
    public int Id {get; init;} = id;

    /// <summary>
    ///     The length of the model design.
    /// </summary>
    public decimal Length {get; set;} = length;

    /// <summary>
    ///     A summary of the model design.
    /// </summary>
    public string Summary {get; set;} = summary;

    /// <summary>
    ///     The URL associated with the model design (optional).
    /// </summary>
    public string? Url {get; set;} = url;
    #endregion

    /// <summary>
    ///     Returns a string representation of the model design.
    /// </summary>
    /// <returns>The description of the model design.</returns>
    public override string ToString() => Summary;
}