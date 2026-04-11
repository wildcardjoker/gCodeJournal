// gCodeJournal.ViewModel

namespace gCodeJournal.ViewModel.DTOs;

#region Using Directives
using Model;
#endregion

/// <summary>
///     Represents a filament type with an ID and a description.
/// </summary>
public class FilamentTypeDto(int id, string description)
{
    #region Constructors
    public FilamentTypeDto() : this(0, "") {}

    /// <inheritdoc />
    public FilamentTypeDto(string description) : this(0, description) {}

    /// <inheritdoc />
    public FilamentTypeDto(FilamentType filamentType) : this(filamentType.Id, filamentType.Description) {}
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