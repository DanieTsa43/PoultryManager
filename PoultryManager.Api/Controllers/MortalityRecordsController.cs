using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoultryManager.Api.Contracts;
using PoultryManager.Api.Data;
using PoultryManager.Api.Domain;
using PoultryManager.Api.Services;

namespace PoultryManager.Api.Controllers;

/// <summary>Mortalité (écran 07). Une seule saisie par jour et par bande.</summary>
[Route("api/bands/{bandId:int}/mortality")]
public sealed class MortalityRecordsController : DailyRecordsControllerBase
{
    public MortalityRecordsController(AppDbContext db) : base(db)
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
        var query = Db.MortalityRecords.AsNoTracking().Where(r => r.BandId == bandId);
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
        var record = await Db.MortalityRecords.AsNoTracking().FirstOrDefaultAsync(r => r.BandId == bandId && r.Date == day);
        return record is null ? NotFound() : Ok(record.ToDto());
    }

    [HttpPut]
    public async Task<IActionResult> Save(int bandId, MortalityRecordRequest request)
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

        if (request.Deaths < 0)
        {
            return Fail(400, "Indiquez le nombre de morts (0 ou plus).");
        }

        var startCount = await CountAtStartOfDayAsync(band, date);
        if (request.Deaths > startCount)
        {
            return Fail(400, $"Le nombre de morts ne peut pas dépasser l'effectif du jour ({FarmRules.Int(startCount)}).");
        }

        await using var transaction = await Db.Database.BeginTransactionAsync();

        var sameDay = await Db.MortalityRecords.FirstOrDefaultAsync(r => r.BandId == bandId && r.Date == date);
        var target = request.Id != 0
            ? await Db.MortalityRecords.FirstOrDefaultAsync(r => r.Id == request.Id && r.BandId == bandId)
            : null;

        if (target is null)
        {
            target = sameDay;
        }
        else if (sameDay is not null && sameDay.Id != target.Id)
        {
            Db.MortalityRecords.Remove(sameDay);
            await Db.SaveChangesAsync();
        }

        if (target is null)
        {
            target = new MortalityRecord { BandId = bandId };
            Db.MortalityRecords.Add(target);
        }

        target.Date = date;
        target.Deaths = request.Deaths;
        target.Cause = FarmRules.Clean(request.Cause, 100);
        target.Notes = FarmRules.Clean(request.Notes, 2000);

        await Db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok(target.ToDto());
    }

    [HttpDelete("~/api/mortality/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await Db.MortalityRecords.Where(r => r.Id == id && r.Band!.UserId == CurrentUserId).ExecuteDeleteAsync();
        return deleted == 0 ? NotFound() : NoContent();
    }
}
