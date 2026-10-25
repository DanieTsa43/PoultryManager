using PoultryManager.Api.Domain;

namespace PoultryManager.Api.Contracts;

/// <summary>Conversions entités → objets envoyés à l'application.</summary>
public static class Mapping
{
    public static UserDto ToDto(this User u)
        => new(u.Id, u.FullName, u.FarmName, u.Email, u.UserName, u.Phone, u.CreatedAt);

    public static BandDto ToDto(this Band b)
        => new(b.Id, b.UserId, b.Name, b.Strain, b.StartDate, b.InitialCount, b.AgeAtArrivalWeeks, b.Notes, b.IsClosed, b.ClosedDate);

    public static EggRecordDto ToDto(this EggRecord r)
        => new(r.Id, r.BandId, r.Date, r.EggsCollected, r.BrokenEggs, r.Category, r.Notes);

    public static FeedRecordDto ToDto(this FeedRecord r)
        => new(r.Id, r.BandId, r.Date, r.FeedType, r.QuantityKg, r.RemainingKg, r.Notes);

    public static MortalityRecordDto ToDto(this MortalityRecord r)
        => new(r.Id, r.BandId, r.Date, r.Deaths, r.Cause, r.Notes);
}
