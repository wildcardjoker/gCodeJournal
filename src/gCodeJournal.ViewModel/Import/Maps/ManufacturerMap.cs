// gCodeJournal.ViewModel

namespace gCodeJournal.ViewModel.Import.Maps;

#region Using Directives
using CsvHelper.Configuration;
using DTOs;
#endregion

public sealed class ManufacturerMap : ClassMap<ManufacturerDto>
{
    #region Constructors
    public ManufacturerMap()
    {
        Map(m => m.Id).Optional();
        Map(m => m.Name);
    }
    #endregion
}