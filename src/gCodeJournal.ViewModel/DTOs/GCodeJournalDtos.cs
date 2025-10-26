namespace gCodeJournal.ViewModel.DTOs
{
 public record CustomerDto(int Id, string Name);

 public record ManufacturerDto(int Id, string Name);

 public record FilamentColourDto(int Id, string Description);

 public record FilamentTypeDto(int Id, string Description);

 public record FilamentDto(int Id, decimal CostPerWeight, string? ProductId, string? ReorderLink,
 FilamentColourDto FilamentColour, FilamentTypeDto FilamentType, ManufacturerDto Manufacturer);

 public record ModelDesignDto(int Id, string Description, decimal Length, string Summary, string? Url);

 public record PrintingProjectDto(int Id, decimal Cost, System.DateTime Submitted, System.DateTime? Completed,
 CustomerDto? Customer, ModelDesignDto? ModelDesign, List<FilamentDto> Filaments);
}
