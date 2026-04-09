// gCodeJournal.ViewModel

namespace gCodeJournal.ViewModel.Import.Maps;

#region Using Directives
using CsvHelper.Configuration;
using DTOs;
#endregion

public sealed class CustomerMap : ClassMap<CustomerDto>
{
    #region Constructors
    public CustomerMap()
    {
        Map(m => m.Id).Optional();
        Map(m => m.Name);
    }
    #endregion
}