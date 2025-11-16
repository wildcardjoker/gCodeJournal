namespace gCodeJournal.ViewModel.Import;

using System.Collections.Generic;

public class ImportResult
{
    public int Created { get; set; }
    public int Updated { get; set; }
    public int Skipped { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Map of entity key -> (source id string -> db id)
    /// Populated during import so dependent rows can be resolved.
    /// </summary>
    public Dictionary<string, Dictionary<string,int>> IdMap { get; } = new(StringComparer.OrdinalIgnoreCase);

    public bool Success => Failed == 0;

    public override string ToString() => $"Created={Created}, Updated={Updated}, Skipped={Skipped}, Failed={Failed}, Errors={Errors.Count}";

    public void RecordMapping(string entityKey, string sourceId, int dbId)
    {
        if (!IdMap.TryGetValue(entityKey, out var inner))
        {
            inner = new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase);
            IdMap[entityKey] = inner;
        }

        inner[sourceId] = dbId;
    }
}
