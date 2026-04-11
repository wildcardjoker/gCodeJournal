// gCodeJournal.ViewModel

namespace gCodeJournal.ViewModel;

#region Using Directives
using DTOs;
#endregion

public static class ExtensionMethods
{
    public static DtoCompareResult CompareTo(this FilamentDto filamentDto, FilamentDto other)
    {
        var errors = new List<string>();

        if (filamentDto.Id != other.Id)
        {
            errors.Add(nameof(filamentDto.Id));
        }

        if (filamentDto.ProductId != other.ProductId)
        {
            errors.Add(nameof(filamentDto.ProductId));
        }

        if (filamentDto.Manufacturer.Id != other.Manufacturer.Id)
        {
            errors.Add("ManufacturerId");
        }

        if (filamentDto.CostPerWeight != other.CostPerWeight)
        {
            errors.Add(nameof(filamentDto.CostPerWeight));
        }

        if (filamentDto.FilamentColour.Id != other.FilamentColour.Id)
        {
            errors.Add("FilamentColourId");
        }

        if (filamentDto.FilamentType.Id != other.FilamentType.Id)
        {
            errors.Add("FilamentTypeId");
        }

        if (filamentDto.ReorderLink != other.ReorderLink)
        {
            errors.Add(nameof(filamentDto.ReorderLink));
        }

        return new DtoCompareResult(errors.Count == 0, errors);
    }

    public static DtoCompareResult CompareTo(this ManufacturerDto manufacturerDto, ManufacturerDto other)
    {
        var errors = new List<string>();
        if (manufacturerDto.Id != other.Id)
        {
            errors.Add(nameof(manufacturerDto.Id));
        }

        if (manufacturerDto.Name != other.Name)
        {
            errors.Add(nameof(manufacturerDto.Name));
        }

        return new DtoCompareResult(errors.Count == 0, errors);
    }

    public static DtoCompareResult CompareTo(this ModelDesignDto modelDesignDto, ModelDesignDto other)
    {
        var errors = new List<string>();
        if (modelDesignDto.Id != other.Id)
        {
            errors.Add(nameof(modelDesignDto.Id));
        }

        if (modelDesignDto.Description != other.Description)
        {
            errors.Add(nameof(modelDesignDto.Description));
        }

        if (modelDesignDto.Length != other.Length)
        {
            errors.Add(nameof(modelDesignDto.Length));
        }

        if (modelDesignDto.Summary != other.Summary)
        {
            errors.Add(nameof(modelDesignDto.Summary));
        }

        if (modelDesignDto.Url != other.Url)
        {
            errors.Add(nameof(modelDesignDto.Url));
        }

        return new DtoCompareResult(errors.Count == 0, errors);
    }

    public static DtoCompareResult CompareTo(this PrinterDto printerDto, PrinterDto other)
    {
        ArgumentNullException.ThrowIfNull(printerDto.Manufacturer, nameof(printerDto.Manufacturer));
        ArgumentNullException.ThrowIfNull(other.Manufacturer,      nameof(other.Manufacturer));
        var errors = new List<string>();
        if (printerDto.Id != other.Id)
        {
            errors.Add(nameof(printerDto.Id));
        }

        if (printerDto.Manufacturer.Id != other.Manufacturer.Id)
        {
            errors.Add("ManufacturerId");
        }

        if (printerDto.Model != other.Model)
        {
            errors.Add(nameof(printerDto.Model));
        }

        return new DtoCompareResult(errors.Count == 0, errors);
    }

    public static DtoCompareResult CompareTo(this PrintingProjectDto printingProjectDto, PrintingProjectDto other)
    {
        var errors = new List<string>();
        if (printingProjectDto.Id != other.Id)
        {
            errors.Add(nameof(printingProjectDto.Id));
        }

        if (printingProjectDto.Completed != other.Completed)
        {
            errors.Add(nameof(printingProjectDto.Completed));
        }

        if (printingProjectDto.Customer?.Id != other.Customer?.Id)
        {
            errors.Add("CustomerId");
        }

        if (printingProjectDto.ModelDesign?.Id != other.ModelDesign?.Id)
        {
            errors.Add("ModelDesignId");
        }

        if (printingProjectDto.Submitted != other.Submitted)
        {
            errors.Add(nameof(printingProjectDto.Submitted));
        }

        if (!IdSequencesEqual(printingProjectDto.Filaments, other.Filaments, x => x.Id))
        {
            errors.Add(nameof(printingProjectDto.Filaments));
        }

        return new DtoCompareResult(errors.Count == 0, errors);
    }

    public static DtoCompareResult CompareTo(this FilamentColourDto filamentColourDto, FilamentColourDto other)
    {
        var errors = new List<string>();
        if (filamentColourDto.Id != other.Id)
        {
            errors.Add(nameof(filamentColourDto.Id));
        }

        if (filamentColourDto.Description != other.Description)
        {
            errors.Add(nameof(filamentColourDto.Description));
        }

        return new DtoCompareResult(errors.Count == 0, errors);
    }

    public static DtoCompareResult CompareTo(this FilamentTypeDto filamentTypeDto, FilamentTypeDto other)
    {
        var errors = new List<string>();
        if (filamentTypeDto.Id != other.Id)
        {
            errors.Add(nameof(filamentTypeDto.Id));
        }

        if (filamentTypeDto.Description != other.Description)
        {
            errors.Add(nameof(filamentTypeDto.Description));
        }

        return new DtoCompareResult(errors.Count == 0, errors);
    }

    public static DtoCompareResult CompareTo(this CustomerDto customerDto, CustomerDto other)
    {
        var errors = new List<string>();
        if (customerDto.Id != other.Id)
        {
            errors.Add(nameof(customerDto.Id));
        }

        if (customerDto.Name != other.Name)
        {
            errors.Add(nameof(customerDto.Name));
        }

        return new DtoCompareResult(errors.Count == 0, errors);
    }

    static bool IdSequencesEqual<T, TId>(IEnumerable<T>? a, IEnumerable<T>? b, Func<T, TId> idSelector)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Select(idSelector).SequenceEqual(b.Select(idSelector));
    }
}