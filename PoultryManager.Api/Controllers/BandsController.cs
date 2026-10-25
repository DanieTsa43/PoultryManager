using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoultryManager.Api.Contracts;
using PoultryManager.Api.Data;
using PoultryManager.Api.Domain;
using PoultryManager.Api.Services;

namespace PoultryManager.Api.Controllers;

/// <summary>Bandes de poules de l'utilisateur connecté (écrans 03, 04, 04b, 10).</summary>
[Route("api/bands")]
public sealed class BandsController : ApiControllerBase
{
    private readonly AppDbContext _db;

    public BandsController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Bandes de l'utilisateur : actives d'abord, puis par nom.</summary>
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var bands = await _db.Bands.AsNoTracking().Where(b => b.UserId == CurrentUserId).ToListAsync();
        var ordered = bands
            .OrderBy(b => b.IsClosed)
            .ThenBy(b => b.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(b => b.ToDto())
            .ToList();
        return Ok(ordered);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var band = await _db.Bands.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id && b.UserId == CurrentUserId);
        return band is null ? NotFound() : Ok(band.ToDto());
    }

    /// <summary>Vrai si une autre bande de l'utilisateur porte déjà ce nom.</summary>
    [HttpGet("name-exists")]
    public async Task<IActionResult> NameExists([FromQuery] string? name, [FromQuery] int? exceptBandId)
        => Ok(await NameExistsAsync(name ?? string.Empty, exceptBandId ?? 0));

    /// <summary>Une bande et toutes ses saisies, en une seule requête.</summary>
    [HttpGet("{id:int}/records")]
    public async Task<IActionResult> Records(int id)
    {
        var owned = await _db.Bands.AsNoTracking().AnyAsync(b => b.Id == id && b.UserId == CurrentUserId);
        if (!owned)
        {
            return NotFound();
        }

        return Ok(await _db.LoadBandRecordsAsync(id));
    }

    [HttpPost]
    public async Task<IActionResult> Create(BandRequest request)
    {
        var error = await ValidateAsync(request, existing: null);
        if (error is not null)
        {
            return error;
        }

        var band = new Band { UserId = CurrentUserId };
        Apply(band, request);
        _db.Bands.Add(band);
        await _db.SaveChangesAsync();
        return Ok(band.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, BandRequest request)
    {
        var band = await _db.Bands.FirstOrDefaultAsync(b => b.Id == id && b.UserId == CurrentUserId);
        if (band is null)
        {
            return NotFound();
        }

        var error = await ValidateAsync(request, band);
        if (error is not null)
        {
            return error;
        }

        Apply(band, request);
        await _db.SaveChangesAsync();
        return Ok(band.ToDto());
    }

    /// <summary>Supprime la bande et (en cascade, dans la base) toutes ses saisies.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _db.Bands.Where(b => b.Id == id && b.UserId == CurrentUserId).ExecuteDeleteAsync();
        return deleted == 0 ? NotFound() : NoContent();
    }

    /// <summary>Supprime toutes les saisies (œufs, aliment, mortalité) d'une bande pour un jour (écran 09).</summary>
    [HttpDelete("{id:int}/days/{date:datetime}")]
    public async Task<IActionResult> DeleteDay(int id, DateTime date)
    {
        var owned = await _db.Bands.AnyAsync(b => b.Id == id && b.UserId == CurrentUserId);
        if (!owned)
        {
            return NotFound();
        }

        var day = date.Date;
        await using var transaction = await _db.Database.BeginTransactionAsync();
        await _db.EggRecords.Where(r => r.BandId == id && r.Date == day).ExecuteDeleteAsync();
        await _db.FeedRecords.Where(r => r.BandId == id && r.Date == day).ExecuteDeleteAsync();
        await _db.MortalityRecords.Where(r => r.BandId == id && r.Date == day).ExecuteDeleteAsync();
        await transaction.CommitAsync();
        return NoContent();
    }

    // ------------------------------------------------------------------ Internes

    private async Task<bool> NameExistsAsync(string name, int exceptBandId)
    {
        var wanted = name.Trim();
        var names = await _db.Bands.AsNoTracking()
            .Where(b => b.UserId == CurrentUserId && b.Id != exceptBandId)
            .Select(b => b.Name)
            .ToListAsync();
        return names.Any(n => string.Equals(n.Trim(), wanted, StringComparison.CurrentCultureIgnoreCase));
    }

    private static void Apply(Band band, BandRequest request)
    {
        band.Name = FarmRules.Clean(request.Name, 100);
        band.Strain = FarmRules.Clean(request.Strain, 100);
        band.StartDate = request.StartDate.Date;
        band.InitialCount = request.InitialCount;
        band.AgeAtArrivalWeeks = request.AgeAtArrivalWeeks;
        band.Notes = FarmRules.Clean(request.Notes, 2000);
        band.IsClosed = request.IsClosed;
        band.ClosedDate = request.IsClosed
            ? (request.ClosedDate ?? FarmRules.Today).Date
            : null;
    }

    /// <summary>Mêmes règles et mêmes messages que le formulaire « Nouvelle bande » de l'application.</summary>
    private async Task<IActionResult?> ValidateAsync(BandRequest request, Band? existing)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Fail(400, "Ce champ est obligatoire.");
        }

        if (request.Name.Trim().Length > 100)
        {
            return Fail(400, "Le nom de la bande est trop long (100 caractères maximum).");
        }

        if (await NameExistsAsync(request.Name, existing?.Id ?? 0))
        {
            return Fail(409, "Une autre bande porte déjà ce nom.");
        }

        if (request.StartDate.Date > FarmRules.MaxAllowedDate)
        {
            return Fail(400, "La date de démarrage ne peut pas être dans le futur.");
        }

        if (request.InitialCount <= 0)
        {
            return Fail(400, "Indiquez un nombre de poules supérieur à 0.");
        }

        if (request.AgeAtArrivalWeeks is < 0 or > 150)
        {
            return Fail(400, "Âge invalide (nombre de semaines).");
        }

        if (request.IsClosed && request.ClosedDate is DateTime closed && closed.Date < request.StartDate.Date)
        {
            return Fail(400, "La date de clôture ne peut pas précéder la date de démarrage.");
        }

        if (existing is not null)
        {
            // La date de démarrage doit rester antérieure au premier suivi déjà saisi.
            var firstDates = new List<DateTime>();
            if (await _db.EggRecords.AnyAsync(r => r.BandId == existing.Id))
            {
                firstDates.Add(await _db.EggRecords.Where(r => r.BandId == existing.Id).MinAsync(r => r.Date));
            }

            if (await _db.FeedRecords.AnyAsync(r => r.BandId == existing.Id))
            {
                firstDates.Add(await _db.FeedRecords.Where(r => r.BandId == existing.Id).MinAsync(r => r.Date));
            }

            if (await _db.MortalityRecords.AnyAsync(r => r.BandId == existing.Id))
            {
                firstDates.Add(await _db.MortalityRecords.Where(r => r.BandId == existing.Id).MinAsync(r => r.Date));
            }

            if (firstDates.Count > 0 && request.StartDate.Date > firstDates.Min().Date)
            {
                return Fail(400, $"Des suivis existent depuis le {FarmRules.Date(firstDates.Min())} : la date de démarrage doit être antérieure.");
            }

            // L'effectif initial ne peut pas être inférieur à la mortalité déjà saisie.
            var deaths = await _db.MortalityRecords.Where(m => m.BandId == existing.Id).SumAsync(m => (int?)m.Deaths) ?? 0;
            if (request.InitialCount < deaths)
            {
                return Fail(400, $"L'effectif initial ne peut pas être inférieur à la mortalité déjà saisie ({FarmRules.Int(deaths)}).");
            }
        }

        return null;
    }
}
