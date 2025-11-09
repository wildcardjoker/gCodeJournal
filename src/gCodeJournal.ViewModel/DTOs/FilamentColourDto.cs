namespace gCodeJournal.ViewModel.DTOs;

/// <summary>
///     Represents a filament color with an ID and a description.
/// </summary>
public class FilamentColourDto(int id, string description)
{
    #region Constructors
    /// <inheritdoc />
    public FilamentColourDto(string description) : this(0, description) {}
    #endregion

    #region Properties
    /// <summary>
    ///     The description of the filament color.
    /// </summary>
    public string Description {get; init;} = description;

    /// <summary>
    ///     The unique identifier of the filament color.
    /// </summary>
    public int Id {get; init;} = id;
    #endregion

    /// <summary>
    ///     Returns a string representation of the filament color.
    /// </summary>
    /// <returns>The description of the filament color.</returns>
    public override string ToString() => Description;
}