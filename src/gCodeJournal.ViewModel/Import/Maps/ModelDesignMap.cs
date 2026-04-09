// gCodeJournal.ViewModel

namespace gCodeJournal.ViewModel.Import.Maps;

#region Using Directives
using CsvHelper.Configuration;
using DTOs;
#endregion

/// <summary>
///     CSV class map for mapping model design CSV rows to <see cref="ModelDesignDto" /> instances.
///     Expects columns: Id (optional), Description, Length, Summary, Url
/// </summary>
public sealed class ModelDesignMap : ClassMap<ModelDesignDto>
{
    #region Constructors
    public ModelDesignMap()
    {
        // Make Id optional so CSVs that omit Id will still map correctly
        Map(m => m.Id).Name("Id").Optional();

        // Primary descriptive fields. Support 'Name' as an alternative header for Description.
        Map(m => m.Description).Name("Description");
        Map(m => m.Length).Name("Length");
        Map(m => m.Summary).Name("Summary");
        Map(m => m.Url).Name("Url");
    }
    #endregion
}