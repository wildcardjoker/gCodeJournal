namespace gCodeJournal.ViewModel.DTOs;

/// <summary>
///     Represents a filament type with an ID and a description.
/// </summary>
public class FilamentTypeDto(int id, string description)
{
    #region Constructors
    /// <inheritdoc />
    public FilamentTypeDto(string description) : this(0, description) {}
    #endregion

    #region Properties
    /// <summary>
    ///     The description of the filament type.
    /// </summary>
    public string Description {get; set;} = description;

    /// <summary>
    ///     The unique identifier of the filament type.
    /// </summary>
    public int Id {get; init;} = id;
    #endregion

    /// <summary>
    ///     Returns a string representation of the filament type.
    /// </summary>
    /// <returns>The description of the filament type.</returns>
    public override string ToString() => Description;
}