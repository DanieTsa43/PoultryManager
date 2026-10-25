using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoultryManager.Api.Contracts;
using PoultryManager.Api.Data;
using PoultryManager.Api.Services;

namespace PoultryManager.Api.Controllers;

/// <summary>Sauvegarde des données (écran 12 · Sauvegarde) : export JSON de tout le compte.</summary>
[Route("api/backup")]
public sealed class BackupController : ApiControllerBase
{
    private readonly AppDbContext _db;

    public BackupController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Export()
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == CurrentUserId);
        if (user is null)
        {
            return Unauthorized();
        }

        var bands = await _db.Bands.AsNoTracking().Where(b => b.UserId == user.Id).OrderBy(b => b.Id).ToListAsync();
        var items = new List<BandBackupDto>();
        foreach (var band in bands)
        {
            items.Add(new BandBackupDto(band.ToDto(), await _db.LoadBandRecordsAsync(band.Id)));
        }

        return Ok(new BackupDto(DateTime.UtcNow, user.ToDto(), items));
    }
}
