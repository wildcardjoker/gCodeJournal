using System.ComponentModel.DataAnnotations;

namespace gCodeJournal.Model;

public record DbUpdateResult(ValidationResult ValidationResult, AddRecordResult AddRecordResult);