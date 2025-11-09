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
        public string Name {get; init;} = name;
        #endregion

        /// <summary>
        ///     Returns a string representation of the manufacturer.
        /// </summary>
        /// <returns>The name of the manufacturer.</returns>
        public override string ToString() => Name;
    }

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
        public string Description {get; init;} = description;

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

    /// <summary>
    ///     Represents a filament with various properties such as cost, product ID, reorder link, color, type, and
    ///     manufacturer.
    /// </summary>
    public class FilamentDto(
        int               id,
        decimal           costPerWeight,
        string?           productId,
        string?           reorderLink,
        FilamentColourDto filamentColour,
        FilamentTypeDto   filamentType,
        ManufacturerDto   manufacturer)
    {
        #region Constructors
        /// <inheritdoc />
        public FilamentDto(
            decimal           costPerWeight,
            string?           productId,
            string?           reorderLink,
            FilamentColourDto filamentColour,
            FilamentTypeDto   filamentType,
            ManufacturerDto   manufacturer) : this(0, costPerWeight, productId, reorderLink, filamentColour, filamentType, manufacturer) {}
        #endregion

        #region Properties
        /// <summary>
        ///     The cost of the filament per unit weight.
        /// </summary>
        public decimal CostPerWeight {get; init;} = costPerWeight;

        /// <summary>
        ///     The color of the filament.
        /// </summary>
        public FilamentColourDto FilamentColour {get; init;} = filamentColour;

        /// <summary>
        ///     The type of the filament.
        /// </summary>
        public FilamentTypeDto FilamentType {get; init;} = filamentType;

        /// <summary>
        ///     The unique identifier of the filament.
        /// </summary>
        public int Id {get; init;} = id;

        /// <summary>
        ///     The manufacturer of the filament.
        /// </summary>
        public ManufacturerDto Manufacturer {get; init;} = manufacturer;

        /// <summary>
        ///     The product ID of the filament (optional).
        /// </summary>
        public string? ProductId {get; init;} = productId;

        /// <summary>
        ///     The link to reorder the filament (optional).
        /// </summary>
        public string? ReorderLink {get; init;} = reorderLink;
        #endregion

        /// <summary>
        ///     Returns a string representation of the filament, including its manufacturer, type, and color.
        /// </summary>
        /// <returns>A string in the format: "{Manufacturer} {FilamentType} ({FilamentColour})".</returns>
        public override string ToString() => $"{Manufacturer} {FilamentType} ({FilamentColour})";
    }

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
        public string Description {get; init;} = description;

        /// <summary>
        ///     The unique identifier of the model design.
        /// </summary>
        public int Id {get; init;} = id;

        /// <summary>
        ///     The length of the model design.
        /// </summary>
        public decimal Length {get; init;} = length;

        /// <summary>
        ///     A summary of the model design.
        /// </summary>
        public string Summary {get; init;} = summary;

        /// <summary>
        ///     The URL associated with the model design (optional).
        /// </summary>
        public string? Url {get; init;} = url;
        #endregion

        /// <summary>
        ///     Returns a string representation of the model design.
        /// </summary>
        /// <returns>The description of the model design.</returns>
        public override string ToString() => Summary;
    }

    /// <summary>
    ///     Represents a printing project with various properties such as cost, submission and completion dates, customer,
    ///     model design, and filaments used.
    /// </summary>
    public class PrintingProjectDto(
        int               id,
        decimal           cost,
        DateTime          submitted,
        DateTime?         completed,
        CustomerDto?      customer,
        ModelDesignDto?   modelDesign,
        List<FilamentDto> filaments)
    {
        #region Constructors
        public PrintingProjectDto(decimal cost, DateTime submitted, DateTime? completed, CustomerDto? customer, ModelDesignDto? modelDesign, List<FilamentDto> filaments) :
            this(0, cost, submitted, completed, customer, modelDesign, filaments) {}
        #endregion

        #region Properties
        /// <summary>
        ///     The date when the project was completed (optional).
        /// </summary>
        public DateTime? Completed {get; init;} = completed;

        /// <summary>
        ///     The total cost of the printing project.
        /// </summary>
        public decimal Cost {get; init;} = cost;

        /// <summary>
        ///     The customer associated with the project (optional).
        /// </summary>
        public CustomerDto? Customer {get; init;} = customer;

        /// <summary>
        ///     The list of filaments used in the project.
        /// </summary>
        public List<FilamentDto> Filaments {get; init;} = filaments;

        /// <summary>
        ///     The unique identifier of the printing project.
        /// </summary>
        public int Id {get; init;} = id;

        /// <summary>
        ///     The model design associated with the project (optional).
        /// </summary>
        public ModelDesignDto? ModelDesign {get; init;} = modelDesign;

        /// <summary>
        ///     The date when the project was submitted.
        /// </summary>
        public DateTime Submitted {get; init;} = submitted;
        #endregion

        /// <summary>
        ///     Returns a string representation of the printing project.
        /// </summary>
        /// <returns>
        ///     A string that includes the model description, customer name, and a list of filaments used in the project.
        ///     If the model or customer is not specified, placeholders ("no model" or "no customer") are used.
        /// </returns>
        public override string ToString()
        {
            var model     = ModelDesign?.Summary ?? "<no model>";
            var customer  = Customer?.Name       ?? "<no customer>";
            var filaments = Filaments.OrderBy(f => f.Manufacturer.Name).ThenBy(f1 => f1.FilamentType.Description).ThenBy(f2 => f2.FilamentColour.Description).ToList();
            return $"{model} for {customer} {string.Join("/", filaments)}";
        }
    }
}