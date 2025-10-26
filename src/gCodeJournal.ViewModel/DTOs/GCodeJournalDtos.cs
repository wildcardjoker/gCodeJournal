namespace gCodeJournal.ViewModel.DTOs
{
    /// <summary>
    ///     Represents a customer with an ID and a name.
    /// </summary>
    /// <param name="Id">The unique identifier of the customer.</param>
    /// <param name="Name">The name of the customer.</param>
    public record CustomerDto(int Id, string Name)
    {
        /// <summary>
        ///     Returns the string representation of the customer.
        /// </summary>
        /// <returns>The name of the customer.</returns>
        public override string ToString() => Name;
    }

    /// <summary>
    ///     Represents a manufacturer with an ID and a name.
    /// </summary>
    /// <param name="Id">The unique identifier of the manufacturer.</param>
    /// <param name="Name">The name of the manufacturer.</param>
    public record ManufacturerDto(int Id, string Name)
    {
        /// <summary>
        ///     Returns a string representation of the manufacturer.
        /// </summary>
        /// <returns>The name of the manufacturer.</returns>
        public override string ToString() => Name;
    }

    /// <summary>
    ///     Represents a filament color with an ID and a description.
    /// </summary>
    /// <param name="Id">The unique identifier of the filament color.</param>
    /// <param name="Description">The description of the filament color.</param>
    public record FilamentColourDto(int Id, string Description)
    {
        /// <summary>
        ///     Returns a string representation of the filament color.
        /// </summary>
        /// <returns>
        ///     The description of the filament color.
        /// </returns>
        public override string ToString() => Description;
    }

    /// <summary>
    ///     Represents a filament type with an ID and a description.
    /// </summary>
    /// <param name="Id">The unique identifier of the filament type.</param>
    /// <param name="Description">The description of the filament type.</param>
    public record FilamentTypeDto(int Id, string Description)
    {
        /// <summary>
        ///     Returns a string representation of the filament type.
        /// </summary>
        /// <returns>
        ///     The description of the filament type.
        /// </returns>
        public override string ToString() => Description;
    }

    /// <summary>
    ///     Represents a filament with various properties such as cost, product ID, reorder link, color, type, and
    ///     manufacturer.
    /// </summary>
    /// <param name="Id">The unique identifier of the filament.</param>
    /// <param name="CostPerWeight">The cost of the filament per unit weight.</param>
    /// <param name="ProductId">The product ID of the filament (optional).</param>
    /// <param name="ReorderLink">The link to reorder the filament (optional).</param>
    /// <param name="FilamentColour">The color of the filament.</param>
    /// <param name="FilamentType">The type of the filament.</param>
    /// <param name="Manufacturer">The manufacturer of the filament.</param>
    public record FilamentDto(
        int               Id,
        decimal           CostPerWeight,
        string?           ProductId,
        string?           ReorderLink,
        FilamentColourDto FilamentColour,
        FilamentTypeDto   FilamentType,
        ManufacturerDto   Manufacturer)
    {
        /// <summary>
        ///     Returns a string representation of the filament, including its manufacturer, type, and color.
        /// </summary>
        /// <returns>
        ///     A string in the format: "{Manufacturer} {FilamentType} ({FilamentColour})".
        /// </returns>
        public override string ToString() => $"{Manufacturer} {FilamentType} ({FilamentColour})";
    }

    /// <summary>
    ///     Represents a model design with properties such as description, length, summary, and URL.
    /// </summary>
    /// <param name="Id">The unique identifier of the model design.</param>
    /// <param name="Description">The description of the model design.</param>
    /// <param name="Length">The length of the model design.</param>
    /// <param name="Summary">A summary of the model design.</param>
    /// <param name="Url">The URL associated with the model design (optional).</param>
    public record ModelDesignDto(int Id, string Description, decimal Length, string Summary, string? Url)
    {
        /// <summary>
        ///     Returns a string representation of the model design.
        /// </summary>
        /// <returns>
        ///     The description of the model design.
        /// </returns>
        public override string ToString() => Description;
    }

    /// <summary>
    ///     Represents a printing project with various properties such as cost, submission and completion dates, customer,
    ///     model design, and filaments used.
    /// </summary>
    /// <param name="Id">The unique identifier of the printing project.</param>
    /// <param name="Cost">The total cost of the printing project.</param>
    /// <param name="Submitted">The date when the project was submitted.</param>
    /// <param name="Completed">The date when the project was completed (optional).</param>
    /// <param name="Customer">The customer associated with the project (optional).</param>
    /// <param name="ModelDesign">The model design associated with the project (optional).</param>
    /// <param name="Filaments">The list of filaments used in the project.</param>
    public record PrintingProjectDto(
        int               Id,
        decimal           Cost,
        DateTime          Submitted,
        DateTime?         Completed,
        CustomerDto?      Customer,
        ModelDesignDto?   ModelDesign,
        List<FilamentDto> Filaments)
    {
        /// <summary>
        ///     Returns a string representation of the printing project.
        /// </summary>
        /// <returns>
        ///     A string that includes the model description, customer name, and a list of filaments used in the project.
        ///     If the model or customer is not specified, placeholders ("&lt;no model&gt;" or "&lt;no customer&gt;") are used.
        /// </returns>
        public override string ToString()
        {
            var model     = ModelDesign?.Description ?? "<no model>";
            var customer  = Customer?.Name           ?? "<no customer>";
            var filaments = Filaments.OrderBy(f => f.Manufacturer).ThenBy(f1 => f1.FilamentType).ThenBy(f2 => f2.FilamentColour).ToList();
            return $"{model} for {customer} {string.Join("/", filaments)}";
        }
    }
}