namespace gCodeJournal.ViewModel.DTOs;

/// <summary>
///     Represents a printing project with various properties such as cost, submission and completion dates, customer,
///     model design, and filaments used.
/// </summary>
public class PrintingProjectDto(
    int               id,
    decimal           cost,
    DateOnly          submitted,
    DateOnly?         completed,
    CustomerDto?      customer,
    ModelDesignDto?   modelDesign,
    List<FilamentDto> filaments)
{
    #region Constructors
    public PrintingProjectDto(decimal cost, DateOnly submitted, DateOnly? completed, CustomerDto? customer, ModelDesignDto? modelDesign, List<FilamentDto> filaments) :
        this(0, cost, submitted, completed, customer, modelDesign, filaments) {}
    #endregion

    #region Properties
    /// <summary>
    ///     The date when the project was completed (optional).
    /// </summary>
    public DateOnly? Completed {get; set;} = completed; // TODO: Revert to init properties with code for updating values in ViewModel

    /// <summary>
    ///     The total cost of the printing project.
    /// </summary>
    public decimal Cost {get; set;} = cost;

    /// <summary>
    ///     The customer associated with the project (optional).
    /// </summary>
    public CustomerDto? Customer {get; set;} = customer;

    /// <summary>
    ///     The list of filaments used in the project.
    /// </summary>
    public List<FilamentDto> Filaments {get; set;} = filaments;

    /// <summary>
    ///     The unique identifier of the printing project.
    /// </summary>
    public int Id {get; init;} = id;

    /// <summary>
    ///     The model design associated with the project (optional).
    /// </summary>
    public ModelDesignDto? ModelDesign {get; set;} = modelDesign;

    /// <summary>
    ///     The date when the project was submitted.
    /// </summary>
    public DateOnly Submitted {get; set;} = submitted;
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