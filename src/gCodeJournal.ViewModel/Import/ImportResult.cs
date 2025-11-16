namespace gCodeJournal.ViewModel.Import;

using System.Collections.Generic;

public class ImportResult
{
    public int Created { get; set; }
    public int Updated { get; set; }
    public int Skipped { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();

    public bool Success => Failed == 0;

    public override string ToString() => $"Created={Created}, Updated={Updated}, Skipped={Skipped}, Failed={Failed}, Errors={Errors.Count}";
}
