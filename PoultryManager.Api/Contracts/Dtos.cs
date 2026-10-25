namespace PoultryManager.Api.Contracts;

// ===================== Authentification =====================

public sealed record RegisterRequest(string? FullName, string? FarmName, string? Phone, string? Email, string? Password);

public sealed record LoginRequest(string? Identifier, string? Password);

public sealed record UpdateProfileRequest(string? FullName, string? FarmName, string? Phone, string? Email);

public sealed record ChangePasswordRequest(string? CurrentPassword, string? NewPassword);

public sealed record ResetPasswordRequest(string? Email, string? NewPassword);

public sealed record UserDto(int Id, string FullName, string FarmName, string Email, string UserName, string Phone, DateTime CreatedAt);

public sealed record AuthResponse(string Token, UserDto User);

// ===================== Bandes =====================

/// <summary>Création ou modification d'une bande (les champs Id / UserId éventuellement envoyés sont ignorés).</summary>
public sealed record BandRequest(
    string? Name,
    string? Strain,
    DateTime StartDate,
    int InitialCount,
    int? AgeAtArrivalWeeks,
    string? Notes,
    bool IsClosed,
    DateTime? ClosedDate);

public sealed record BandDto(
    int Id,
    int UserId,
    string Name,
    string Strain,
    DateTime StartDate,
    int InitialCount,
    int? AgeAtArrivalWeeks,
    string Notes,
    bool IsClosed,
    DateTime? ClosedDate);

// ===================== Saisies quotidiennes =====================

public sealed record EggRecordRequest(int Id, DateTime Date, int EggsCollected, int? BrokenEggs, string? Category, string? Notes);

public sealed record EggRecordDto(int Id, int BandId, DateTime Date, int EggsCollected, int? BrokenEggs, string Category, string Notes);

public sealed record FeedRecordRequest(int Id, DateTime Date, string? FeedType, double QuantityKg, double? RemainingKg, string? Notes);

public sealed record FeedRecordDto(int Id, int BandId, DateTime Date, string FeedType, double QuantityKg, double? RemainingKg, string Notes);

public sealed record MortalityRecordRequest(int Id, DateTime Date, int Deaths, string? Cause, string? Notes);

public sealed record MortalityRecordDto(int Id, int BandId, DateTime Date, int Deaths, string Cause, string Notes);

/// <summary>Toutes les saisies d'une bande, en une seule requête.</summary>
public sealed record BandRecordsResponse(
    IReadOnlyList<EggRecordDto> Eggs,
    IReadOnlyList<FeedRecordDto> Feeds,
    IReadOnlyList<MortalityRecordDto> Mortality);

// ===================== Sauvegarde =====================

public sealed record BandBackupDto(BandDto Band, BandRecordsResponse Records);

public sealed record BackupDto(DateTime ExportedAt, UserDto User, IReadOnlyList<BandBackupDto> Bands);
