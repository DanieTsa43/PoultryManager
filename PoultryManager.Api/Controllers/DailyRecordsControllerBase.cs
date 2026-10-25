using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoultryManager.Api.Data;
using PoultryManager.Api.Domain;
using PoultryManager.Api.Services;

namespace PoultryManager.Api.Controllers;

/// <summary>Éléments communs aux saisies quotidiennes (œufs, aliment, mortalité).</summary>
public abstract class DailyRecordsControllerBase : ApiControllerBase
{
    protected DailyRecordsControllerBase(AppDbContext db)
    {
        Db = db;
    }

    protected AppDbContext Db { get; }

    /// <summary>La bande, à condition qu'elle appartienne à l'utilisateur connecté.</summary>
    protected Task<Band?> FindBandAsync(int bandId)
        => Db.Bands.AsNoTracking().FirstOrDefaultAsync(b => b.Id == bandId && b.UserId == CurrentUserId);

    /// <summary>Date dans la période de la bande : ni avant le démarrage, ni dans le futur, ni après la clôture.</summary>
    protected IActionResult? ValidateDate(Band band, DateTime date)
    {
        if (date.Date < band.StartDate.Date)
        {
            return Fail(400, $"La bande a démarré le {FarmRules.Date(band.StartDate)} : choisissez une date ultérieure.");
        }

        if (date.Date > FarmRules.MaxAllowedDate)
        {
            return Fail(400, "La date ne peut pas être dans le futur.");
        }

        if (band.IsClosed && band.ClosedDate is DateTime closed && date.Date > closed.Date)
        {
            return Fail(400, $"La bande a été clôturée le {FarmRules.Date(closed)}.");
        }

        return null;
    }

    /// <summary>Effectif en début de journée = effectif initial − morts des jours précédents.</summary>
    protected async Task<int> CountAtStartOfDayAsync(Band band, DateTime date)
    {
        var day = date.Date;
        var deaths = await Db.MortalityRecords
            .Where(m => m.BandId == band.Id && m.Date < day)
            .SumAsync(m => (int?)m.Deaths) ?? 0;
        return band.InitialCount - deaths;
    }

    protected static (DateTime? Lower, DateTime? Upper) Range(DateTime? from, DateTime? to)
        => (from?.Date, to?.Date);
}
