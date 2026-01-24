using gCodeJournal.ViewModel.DTOs;

namespace gCodeJournal.ViewModel;

public static class ExtensionMethods
{
    public static DtoCompareResult CompareTo(this FilamentDto filamentDto, FilamentDto other)
    {
        var errors = new List<string>();

        if (filamentDto.Id != other.Id)
            errors.Add(nameof(filamentDto.Id));
        if (filamentDto.ProductId != other.ProductId)
            errors.Add(nameof(filamentDto.ProductId));
        if (filamentDto.Manufacturer.Id != other.Manufacturer.Id)
            errors.Add("ManufacturerId");
        if (filamentDto.CostPerWeight != other.CostPerWeight)
            errors.Add(nameof(filamentDto.CostPerWeight));
        if (filamentDto.FilamentColour.Id != other.FilamentColour.Id)
            errors.Add("FilamentColourId");
        if (filamentDto.FilamentType.Id != other.FilamentType.Id)
            errors.Add("FilamentTypeId");
        if (filamentDto.ReorderLink != other.ReorderLink)
            errors.Add(nameof(filamentDto.ReorderLink));
        return new DtoCompareResult(errors.Count == 0, errors);
    }
}