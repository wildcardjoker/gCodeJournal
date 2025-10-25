using System.Collections.Generic;
using System.Threading.Tasks;
using gCodeJournal.Model;

namespace gCodeJournal.ViewModel
{
 public interface IGCodeJournalViewModel
 {
 Task AddFilamentAsync(Filament filament);
 Task<List<Filament>> GetAllFilamentsAsync();
 Task<List<Manufacturer>> GetAllManufacturersAsync();
 Task<List<Customer>> GetAllCustomersAsync();
 Task<List<FilamentColour>> GetAllFilamentColoursAsync();
 Task<List<FilamentType>> GetAllFilamentTypesAsync();
 Task<List<ModelDesign>> GetAllModelDesignsAsync();
 Task<List<PrintingProject>> GetAllPrintingProjectsAsync();
 }
}
