// gCodeJournal.ViewModel

namespace gCodeJournal.ViewModel.Import.Maps;

#region Using Directives
using CsvHelper.Configuration;
using DTOs;
#endregion

public sealed class FilamentTypeMap : ClassMap<FilamentTypeDto>
{
    #region Constructors
    public FilamentTypeMap()
    {
        Map(m => m.Id).Optional();
        Map(m => m.Description);
    }
    #endregion
}