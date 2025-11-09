namespace gCodeJournal.ViewModel
{
    #region Using Directives
    using System.ComponentModel.DataAnnotations;
    using DTOs;
    using Model;
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

        // Add operations (DTO-based overloads) - return ValidationResult instead of throwing
        /// <summary>
        ///     Adds a new customer asynchronously using DTO.
        /// </summary>
        /// <param name="customerDto">The customer DTO to add.</param>
        /// <returns>
        ///     A task representing the asynchronous operation; returns a ValidationResult indicating success or validation
        ///     errors.
        /// </returns>
        Task<ValidationResult> AddCustomerAsync(CustomerDto customerDto);

        /// <summary>
        ///     Adds a new filament asynchronously.
        /// </summary>
        /// <param name="filament">The filament to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddFilamentAsync(Filament filament);

        /// <summary>
        ///     Adds a new filament asynchronously using DTO.
        /// </summary>
        /// <param name="filamentDto">The filament DTO to add.</param>
        /// <returns>
        ///     A task representing the asynchronous operation; returns a ValidationResult indicating success or validation
        ///     errors.
        /// </returns>
        Task<ValidationResult> AddFilamentAsync(FilamentDto filamentDto);

        /// <summary>
        ///     Adds a new filament color asynchronously.
        /// </summary>
        /// <param name="filamentColour">The filament color to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddFilamentColourAsync(FilamentColour filamentColour);

        /// <summary>
        ///     Adds a new filament color asynchronously using DTO.
        /// </summary>
        /// <param name="filamentColourDto">The filament color DTO to add.</param>
        /// <returns>
        ///     A task representing the asynchronous operation; returns a ValidationResult indicating success or validation
        ///     errors.
        /// </returns>
        Task<ValidationResult> AddFilamentColourAsync(FilamentColourDto filamentColourDto);

        /// <summary>
        ///     Adds a new filament type asynchronously.
        /// </summary>
        /// <param name="filamentType">The filament type to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddFilamentTypeAsync(FilamentType filamentType);

        /// <summary>
        ///     Adds a new filament type asynchronously using DTO.
        /// </summary>
        /// <param name="filamentTypeDto">The filament type DTO to add.</param>
        /// <returns>
        ///     A task representing the asynchronous operation; returns a ValidationResult indicating success or validation
        ///     errors.
        /// </returns>
        Task<ValidationResult> AddFilamentTypeAsync(FilamentTypeDto filamentTypeDto);

        /// <summary>
        ///     Adds a new manufacturer asynchronously.
        /// </summary>
        /// <param name="manufacturer">The manufacturer entity to add.</param>
        /// <remarks>
        ///     The <paramref name="manufacturer" /> represents a vendor or brand of 3D printing filament.
        ///     Typical examples include "Prusament" or "Hatchbox".
        /// </remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="manufacturer" /> is <c>null</c>.</exception>
        Task<ValidationResult> AddManufacturerAsync(Manufacturer manufacturer);

        /// <summary>
        ///     Adds a new manufacturer asynchronously using a DTO.
        /// </summary>
        /// <param name="manufacturer">The manufacturer DTO to add.</param>
        /// <returns>
        ///     A task representing the asynchronous operation; returns a <see cref="ValidationResult" /> indicating success or
        ///     validation errors.
        /// </returns>
        Task<ValidationResult> AddManufacturerAsync(ManufacturerDto manufacturer);

        /// <summary>
        ///     Adds a new model design asynchronously.
        /// </summary>
        /// <param name="modelDesign">The model design to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddModelDesignAsync(ModelDesign modelDesign);

        /// <summary>
        ///     Adds a new model design asynchronously using DTO.
        /// </summary>
        /// <param name="modelDesignDto">The model design DTO to add.</param>
        /// <returns>
        ///     A task representing the asynchronous operation; returns a ValidationResult indicating success or validation
        ///     errors.
        /// </returns>
        Task<ValidationResult> AddModelDesignAsync(ModelDesignDto modelDesignDto);

        /// <summary>
        ///     Adds a new printing project asynchronously.
        /// </summary>
        /// <param name="project">The printing project to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddPrintingProjectAsync(PrintingProject project);

        /// <summary>
        ///     Adds a new printing project asynchronously using DTO.
        /// </summary>
        /// <param name="projectDto">The printing project DTO to add.</param>
        /// <returns>
        ///     A task representing the asynchronous operation; returns a ValidationResult indicating success or validation
        ///     errors.
        /// </returns>
        Task<ValidationResult> AddPrintingProjectAsync(PrintingProjectDto projectDto);

        // Edit operations (DTO-based)
        /// <summary>
        ///     Edits an existing customer asynchronously using DTO.
        /// </summary>
        /// <param name="customerDto">The customer DTO with updated values.</param>
        /// <returns>
        ///     A task representing the asynchronous operation; returns a ValidationResult indicating success or validation
        ///     errors.
        /// </returns>
        Task<ValidationResult> EditCustomerAsync(CustomerDto customerDto);

        /// <summary>
        ///     Edits an existing filament asynchronously using DTO.
        /// </summary>
        /// <param name="filamentDto">The filament DTO with updated values.</param>
        /// <returns>
        ///     A task representing the asynchronous operation; returns a ValidationResult indicating success or validation
        ///     errors.
        /// </returns>
        Task<ValidationResult> EditFilamentAsync(FilamentDto filamentDto);

        /// <summary>
        ///     Edits an existing filament color asynchronously using DTO.
        /// </summary>
        /// <param name="filamentColourDto">The filament color DTO with updated values.</param>
        /// <returns>
        ///     A task representing the asynchronous operation; returns a ValidationResult indicating success or validation
        ///     errors.
        /// </returns>
        Task<ValidationResult> EditFilamentColourAsync(FilamentColourDto filamentColourDto);

        /// <summary>
        ///     Edits an existing filament type asynchronously using DTO.
        /// </summary>
        /// <param name="filamentTypeDto">The filament type DTO with updated values.</param>
        /// <returns>
        ///     A task representing the asynchronous operation; returns a ValidationResult indicating success or validation
        ///     errors.
        /// </returns>
        Task<ValidationResult> EditFilamentTypeAsync(FilamentTypeDto filamentTypeDto);

        /// <summary>
        ///     Edits an existing manufacturer asynchronously using DTO.
        /// </summary>
        /// <param name="manufacturerDto">The manufacturer DTO with updated values.</param>
        /// <returns>
        ///     A task representing the asynchronous operation; returns a ValidationResult indicating success or validation
        ///     errors.
        /// </returns>
        Task<ValidationResult> EditManufacturerAsync(ManufacturerDto manufacturerDto);

        /// <summary>
        ///     Edits an existing model design asynchronously using DTO.
        /// </summary>
        /// <param name="modelDesignDto">The model design DTO with updated values.</param>
        /// <returns>
        ///     A task representing the asynchronous operation; returns a ValidationResult indicating success or validation
        ///     errors.
        /// </returns>
        Task<ValidationResult> EditModelDesignAsync(ModelDesignDto modelDesignDto);

        /// <summary>
        ///     Edits an existing printing project asynchronously using DTO.
        /// </summary>
        /// <param name="printingProjectDto">The printing project DTO with updated values.</param>
        /// <returns>
        ///     A task representing the asynchronous operation; returns a ValidationResult indicating success or validation
        ///     errors.
        /// </returns>
        Task<ValidationResult> EditPrintingProjectAsync(PrintingProjectDto printingProjectDto);

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