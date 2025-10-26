namespace gCodeJournal.ViewModel
{
    #region Using Directives
    using Model;
    using DTOs;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    #endregion

    /// <summary>
    ///     Represents the view model interface for managing GCode Journal operations.
    ///     Provides methods to add and retrieve data related to customers, filaments, filament colors, filament types, model
    ///     designs, and printing projects.
    /// </summary>
    public interface IGCodeJournalViewModel
    {
        // Add operations (entity-based)
        /// <summary>
        ///     Adds a new customer asynchronously.
        /// </summary>
        /// <param name="customer">The customer to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddCustomerAsync(Customer customer);

        /// <summary>
        ///     Adds a new filament asynchronously.
        /// </summary>
        /// <param name="filament">The filament to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddFilamentAsync(Filament filament);

        /// <summary>
        ///     Adds a new filament color asynchronously.
        /// </summary>
        /// <param name="filamentColour">The filament color to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddFilamentColourAsync(FilamentColour filamentColour);

        /// <summary>
        ///     Adds a new filament type asynchronously.
        /// </summary>
        /// <param name="filamentType">The filament type to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddFilamentTypeAsync(FilamentType filamentType);

        /// <summary>
        ///     Adds a new model design asynchronously.
        /// </summary>
        /// <param name="modelDesign">The model design to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddModelDesignAsync(ModelDesign modelDesign);

        /// <summary>
        ///     Adds a new printing project asynchronously.
        /// </summary>
        /// <param name="project">The printing project to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddPrintingProjectAsync(PrintingProject project);

        // Retrieval operations (DTO-based)
        /// <summary>
        ///     Retrieves all customers asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with a list of customers as the result.</returns>
        Task<List<CustomerDto>> GetAllCustomersAsync();

        /// <summary>
        ///     Retrieves all filament colors asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with a list of filament colors as the result.</returns>
        Task<List<FilamentColourDto>> GetAllFilamentColoursAsync();

        /// <summary>
        ///     Retrieves all filaments asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with a list of filaments as the result.</returns>
        Task<List<FilamentDto>> GetAllFilamentsAsync();

        /// <summary>
        ///     Retrieves all filament types asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with a list of filament types as the result.</returns>
        Task<List<FilamentTypeDto>> GetAllFilamentTypesAsync();

        /// <summary>
        ///     Retrieves all manufacturers asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with a list of manufacturers as the result.</returns>
        Task<List<ManufacturerDto>> GetAllManufacturersAsync();

        /// <summary>
        ///     Retrieves all model designs asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with a list of model designs as the result.</returns>
        Task<List<ModelDesignDto>> GetAllModelDesignsAsync();

        /// <summary>
        ///     Retrieves all printing projects asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with a list of printing projects as the result.</returns>
        Task<List<PrintingProjectDto>> GetAllPrintingProjectsAsync();
    }
}