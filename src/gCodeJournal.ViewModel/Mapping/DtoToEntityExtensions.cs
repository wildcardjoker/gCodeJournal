namespace gCodeJournal.ViewModel.Mapping;

#region Using Directives
using DTOs;
using Model;
#endregion

public static class DtoToEntityExtensions
{
    public static Customer ToEntity(this CustomerDto dto) => new () {Id = dto.Id, Name = dto.Name};

    public static Manufacturer ToEntity(this ManufacturerDto dto) => new () {Id = dto.Id, Name = dto.Name};

    public static FilamentColour ToEntity(this FilamentColourDto dto) => new () {Id = dto.Id, Description = dto.Description};

    public static FilamentType ToEntity(this FilamentTypeDto dto) => new () {Id = dto.Id, Description = dto.Description};

    public static Filament ToEntity(this FilamentDto dto)
    {
        var filament = new Filament
        {
            Id               = dto.Id,
            CostPerWeight    = dto.CostPerWeight,
            ProductId        = dto.ProductId,
            ReorderLink      = dto.ReorderLink,
            ManufacturerId   = dto.Manufacturer?.Id   ?? 0,
            FilamentColourId = dto.FilamentColour?.Id ?? 0,
            FilamentTypeId   = dto.FilamentType?.Id   ?? 0
        };
        return filament;
    }

    public static ModelDesign ToEntity(this ModelDesignDto dto) => new ()
    {
        Id          = dto.Id,
        Description = dto.Description,
        Length      = dto.Length,
        Summary     = dto.Summary,
        Url         = dto.Url
    };

    public static PrintingProject ToEntity(this PrintingProjectDto dto)
    {
        var project = new PrintingProject
        {
            Id            = dto.Id,
            Cost          = dto.Cost,
            Submitted     = dto.Submitted,
            Completed     = dto.Completed,
            CustomerId    = dto.Customer?.Id    ?? 0,
            ModelDesignId = dto.ModelDesign?.Id ?? 0
        };
        return project;
    }
}