using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoultryManager.Api.Contracts;
using PoultryManager.Api.Data;
using PoultryManager.Api.Domain;
using PoultryManager.Api.Services;

namespace PoultryManager.Api.Controllers;

/// <summary>Production d'œufs (écran 05). Une seule saisie par jour et par bande.</summary>
[Route("api/bands/{bandId:int}/eggs")]
public sealed class EggRecordsController : DailyRecordsControllerBase
{
    public EggRecordsController(AppDbContext db) : base(db)
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
        var query = Db.EggRecords.AsNoTracking().Where(r => r.BandId == bandId);
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
        var record = await Db.EggRecords.AsNoTracking().FirstOrDefaultAsync(r => r.BandId == bandId && r.Date == day);
        return record is null ? NotFound() : Ok(record.ToDto());
    }

    /// <summary>
    /// Enregistre la saisie du jour. Si une saisie existe déjà à cette date, elle est mise à jour ;
    /// si la date d'une saisie modifiée tombe sur un autre jour déjà saisi, cette autre saisie est remplacée.
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> Save(int bandId, EggRecordRequest request)
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

        if (request.EggsCollected < 0)
        {
            return Fail(400, "Indiquez le nombre d'œufs collectés (0 ou plus).");
        }

        var startCount = await CountAtStartOfDayAsync(band, date);
        if (request.EggsCollected > startCount)
        {
            return Fail(400, $"Le nombre d'œufs ne peut pas dépasser l'effectif de la bande ({FarmRules.Int(startCount)}).");
        }

        if (request.BrokenEggs is int broken && (broken < 0 || broken > request.EggsCollected))
        {
            return Fail(400, "Les œufs cassés ne peuvent pas dépasser les œufs collectés.");
        }

        await using var transaction = await Db.Database.BeginTransactionAsync();

        var sameDay = await Db.EggRecords.FirstOrDefaultAsync(r => r.BandId == bandId && r.Date == date);
        var target = request.Id != 0
            ? await Db.EggRecords.FirstOrDefaultAsync(r => r.Id == request.Id && r.BandId == bandId)
            : null;

        if (target is null)
        {
            target = sameDay;
        }
        else if (sameDay is not null && sameDay.Id != target.Id)
        {
            Db.EggRecords.Remove(sameDay);
            await Db.SaveChangesAsync(); // libère l'index unique (bande, date) avant la mise à jour
        }

        if (target is null)
        {
            target = new EggRecord { BandId = bandId };
            Db.EggRecords.Add(target);
        }

        target.Date = date;
        target.EggsCollected = request.EggsCollected;
        target.BrokenEggs = request.BrokenEggs;
        target.Category = FarmRules.Clean(request.Category, 50);
        target.Notes = FarmRules.Clean(request.Notes, 2000);

        await Db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok(target.ToDto());
    }

    [HttpDelete("~/api/eggs/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await Db.EggRecords.Where(r => r.Id == id && r.Band!.UserId == CurrentUserId).ExecuteDeleteAsync();
        return deleted == 0 ? NotFound() : NoContent();
    }
}
