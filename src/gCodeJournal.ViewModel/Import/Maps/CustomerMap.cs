// gCodeJournal.ViewModel

#region Using Directives
#endregion

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
        Map(m => m.Id).Optional(); // CsvHelper v?.x supports Optional on maps; otherwise avoid mapping Id
        Map(m => m.Name);
    }
    #endregion
}