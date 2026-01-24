namespace gCodeJournal.ViewModel;

/// <summary>
/// Represents the result of comparing two DTOs (Data Transfer Objects).
/// </summary>
/// <param name="IsMatch">
/// Indicates whether the compared DTOs are identical.
/// </param>
/// <param name="MismatchedProperties">
/// A list of property names that differ between the compared DTOs.
/// </param>
public record DtoCompareResult(bool IsMatch, List<string> MismatchedProperties);