using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoultryManager.Api.Contracts;
using PoultryManager.Api.Data;
using PoultryManager.Api.Domain;
using PoultryManager.Api.Services;

namespace PoultryManager.Api.Controllers;

/// <summary>Consommation d'aliment (écran 06). Une seule saisie par jour et par bande.</summary>
[Route("api/bands/{bandId:int}/feeds")]
public sealed class FeedRecordsController : DailyRecordsControllerBase
{
    public FeedRecordsController(AppDbContext db) : base(db)
    {
    }

    [HttpGet]
    public async Task<IActionResult> List(int bandId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        if (await FindBandAsync(bandId) is null)
        {
            return NotFound();
        }

        var (lower, upper) = Range(from, to);
        var query = Db.FeedRecords.AsNoTracking().Where(r => r.BandId == bandId);
        if (lower.HasValue)
        {
            query = query.Where(r => r.Date >= lower.Value);
        }

        if (upper.HasValue)
        {
            query = query.Where(r => r.Date <= upper.Value);
        }

        var list = await query.OrderBy(r => r.Date).ToListAsync();
        return Ok(list.Select(r => r.ToDto()).ToList());
    }

    [HttpGet("{date:datetime}")]
    public async Task<IActionResult> Get(int bandId, DateTime date)
    {
        if (await FindBandAsync(bandId) is null)
        {
            return NotFound();
        }

        var day = date.Date;
        var record = await Db.FeedRecords.AsNoTracking().FirstOrDefaultAsync(r => r.BandId == bandId && r.Date == day);
        return record is null ? NotFound() : Ok(record.ToDto());
    }

    [HttpPut]
    public async Task<IActionResult> Save(int bandId, FeedRecordRequest request)
    {
        var band = await FindBandAsync(bandId);
        if (band is null)
        {
            return NotFound();
        }

        var date = request.Date.Date;
        if (ValidateDate(band, date) is { } dateError)
        {
            return dateError;
        }

        if (!double.IsFinite(request.QuantityKg) || request.QuantityKg <= 0)
        {
            return Fail(400, "Indiquez la quantité distribuée en kg (supérieure à 0).");
        }

        if (request.QuantityKg > 100_000)
        {
            return Fail(400, "Quantité trop élevée : vérifiez l'unité (kg).");
        }

        if (request.RemainingKg is double remaining)
        {
            if (!double.IsFinite(remaining) || remaining < 0)
            {
                return Fail(400, "Quantité invalide.");
            }

            if (remaining > request.QuantityKg)
            {
                return Fail(400, "Le reste ne peut pas dépasser la quantité distribuée.");
            }
        }

        await using var transaction = await Db.Database.BeginTransactionAsync();

        var sameDay = await Db.FeedRecords.FirstOrDefaultAsync(r => r.BandId == bandId && r.Date == date);
        var target = request.Id != 0
            ? await Db.FeedRecords.FirstOrDefaultAsync(r => r.Id == request.Id && r.BandId == bandId)
            : null;

        if (target is null)
        {
            target = sameDay;
        }
        else if (sameDay is not null && sameDay.Id != target.Id)
        {
            Db.FeedRecords.Remove(sameDay);
            await Db.SaveChangesAsync();
        }

        if (target is null)
        {
            target = new FeedRecord { BandId = bandId };
            Db.FeedRecords.Add(target);
        }

        target.Date = date;
        target.FeedType = FarmRules.Clean(request.FeedType, 100);
        target.QuantityKg = request.QuantityKg;
        target.RemainingKg = request.RemainingKg;
        target.Notes = FarmRules.Clean(request.Notes, 2000);

        await Db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok(target.ToDto());
    }

    [HttpDelete("~/api/feeds/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await Db.FeedRecords.Where(r => r.Id == id && r.Band!.UserId == CurrentUserId).ExecuteDeleteAsync();
        return deleted == 0 ? NotFound() : NoContent();
    }
}
