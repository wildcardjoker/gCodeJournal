using System.Collections.Generic;
using System.Threading.Tasks;
using gCodeJournal.Model;

namespace gCodeJournal.ViewModel
{
 public interface IGCodeJournalViewModel
 {
 Task AddFilament(Filament filament);
 Task<List<Filament>> GetFilamentsAsync();
 Task<List<Manufacturer>> GetManufacturersAsync();
 Task<List<Customer>> GetCustomersAsync();
 }
}
