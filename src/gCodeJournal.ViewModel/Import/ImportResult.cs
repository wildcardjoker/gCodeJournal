namespace gCodeJournal.ViewModel.Import;

public class ImportResult
{
    #region Properties
    public int          Created {get; set;}
    public List<string> Errors  {get;} = [];
    public int          Failed  {get; set;}

    /// <summary>
    ///     Map of entity key -> (source id string -> db id)
    ///     Populated during import so dependent rows can be resolved.
    /// </summary>
    public Dictionary<string, Dictionary<string, int>> IdMap {get;} = new (StringComparer.OrdinalIgnoreCase);

    public int Skipped {get; set;}

    public bool Success => Failed == 0;
    public int  Updated {get; set;}
    #endregion

    public void RecordMapping(string entityKey, string sourceId, int dbId)
    {
        if (!IdMap.TryGetValue(entityKey, out var inner))
        {
            inner            = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            IdMap[entityKey] = inner;
        }

        inner[sourceId] = dbId;
    }

    public override string ToString() => $"Created={Created}, Updated={Updated}, Skipped={Skipped}, Failed={Failed}, Errors={Errors.Count}";
}