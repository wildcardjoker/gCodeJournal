namespace gCodeJournal.Model;

/// <summary>
/// Indicates the outcome of an attempt to add or update a record in the application's
/// data store.
/// </summary>
public enum AddRecordResult
{
    /// <summary>
    /// A new record was created and persisted to the database.
    /// </summary>
    Added,
    /// <summary>
    /// Indicates that the record already exists in the application's data store.
    /// </summary>
    Exists,
    /// <summary>
    /// An existing record was modified and the changes were saved.
    /// </summary>
    Modified,

    /// <summary>
    /// The operation was skipped because the record already existed or no change was needed.
    /// </summary>
    Skipped,

    /// <summary>
    /// The operation failed and the record was not added or updated.
    /// </summary>
    Failed
}
