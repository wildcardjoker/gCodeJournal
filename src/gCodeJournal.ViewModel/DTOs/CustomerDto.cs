// gCodeJournal.ViewModel

namespace gCodeJournal.ViewModel.DTOs;

/// <summary>
///     Represents a customer with an ID and a name.
/// </summary>
/// <param name="id">The unique identifier of the customer.</param>
/// <param name="name">The name of the customer.</param>
public class CustomerDto(int id, string name)
{
    #region Constructors
    /// <inheritdoc />
    public CustomerDto(string name) : this(0, name) {}
    #endregion

    #region Properties
    /// <summary>The unique identifier of the customer.</summary>
    public int Id {get; init;} = id;

    /// <summary>The name of the customer.</summary>
    public string Name {get; set;} = name;
    #endregion

    public void Deconstruct(out int Id, out string Name)
    {
        Id   = this.Id;
        Name = this.Name;
    }

    /// <summary>
    ///     Returns the string representation of the customer.
    /// </summary>
    /// <returns>The name of the customer.</returns>
    public override string ToString() => Name;
}