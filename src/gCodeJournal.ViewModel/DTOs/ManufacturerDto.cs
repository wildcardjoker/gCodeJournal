namespace gCodeJournal.ViewModel.DTOs
{
    /// <summary>
    ///     Represents a manufacturer with an ID and a name.
    /// </summary>
    public class ManufacturerDto(int id, string name)
    {
        #region Constructors
        /// <inheritdoc />
        public ManufacturerDto(string name) : this(0, name) {}
        #endregion

        #region Properties
        /// <summary>
        ///     The unique identifier of the manufacturer.
        /// </summary>
        public int Id {get; init;} = id;

        /// <summary>
        ///     The name of the manufacturer.
        /// </summary>
        public string Name {get; set;} = name;
        #endregion

        /// <summary>
        ///     Returns a string representation of the manufacturer.
        /// </summary>
        /// <returns>The name of the manufacturer.</returns>
        public override string ToString() => Name;
    }
}