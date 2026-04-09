// gCodeJournal.ViewModel

namespace gCodeJournal.ViewModel.Import.Maps;

#region Using Directives
using CsvHelper.Configuration;
using DTOs;
#endregion

public sealed class FilamentColourMap : ClassMap<FilamentColourDto>
{
    #region Constructors
    public FilamentColourMap()
    {
        Map(m => m.Id).Optional();
        Map(m => m.Description);
    }
    #endregion
}