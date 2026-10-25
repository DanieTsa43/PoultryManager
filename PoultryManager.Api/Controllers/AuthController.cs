using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PoultryManager.Api.Contracts;
using PoultryManager.Api.Data;
using PoultryManager.Api.Domain;
using PoultryManager.Api.Services;

namespace PoultryManager.Api.Controllers;

/// <summary>Inscription, connexion, profil et mots de passe.</summary>
[Route("api/auth")]
public sealed partial class AuthController : ApiControllerBase
{
    private const int MinPasswordLength = 6;

    private readonly AppDbContext _db;
    private readonly TokenService _tokens;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, TokenService tokens, IConfiguration config)
    {
        _db = db;
        _tokens = tokens;
        _config = config;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();

    private static bool IsValidEmail(string? email)
        => !string.IsNullOrWhiteSpace(email) && EmailRegex().IsMatch(email.Trim());

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    // ---------------------------------------------------------------- Inscription

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return Fail(400, "Le nom complet est obligatoire.");
        }

        if (!IsValidEmail(request.Email))
        {
            return Fail(400, "Adresse e-mail invalide.");
        }

        if ((request.Password ?? string.Empty).Length < MinPasswordLength)
        {
            return Fail(400, $"Le mot de passe doit contenir au moins {MinPasswordLength} caractères.");
        }

        var email = Normalize(request.Email!);
        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            return Fail(409, "Un compte existe déjà avec cette adresse e-mail.");
        }

        var (hash, salt) = PasswordHasher.Hash(request.Password!);
        var user = new User
        {
            FullName = FarmRules.Clean(request.FullName, 150),
            FarmName = FarmRules.Clean(request.FarmName, 150),
            Phone = FarmRules.Clean(request.Phone, 40),
            Email = email,
            UserName = email.Split('@')[0],
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok(new AuthResponse(_tokens.CreateToken(user), user.ToDto()));
    }

    // ---------------------------------------------------------------- Connexion

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        const string invalid = "Identifiant ou mot de passe incorrect.";
        if (string.IsNullOrWhiteSpace(request.Identifier) || string.IsNullOrEmpty(request.Password))
        {
            return Fail(401, invalid);
        }

        var key = Normalize(request.Identifier);
        var candidates = await _db.Users.Where(u => u.Email == key || u.UserName == key).ToListAsync();
        var user = candidates.FirstOrDefault(u => PasswordHasher.Verify(request.Password, u.PasswordHash, u.PasswordSalt));

        return user is null
            ? Fail(401, invalid)
            : Ok(new AuthResponse(_tokens.CreateToken(user), user.ToDto()));
    }

    // ---------------------------------------------------------------- Session et profil

    /// <summary>Utilisateur du jeton : sert à rouvrir la session mémorisée.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == CurrentUserId);
        return user is null ? Unauthorized() : Ok(user.ToDto());
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == CurrentUserId);
        if (user is null)
        {
            return Unauthorized();
        }

        if (!IsValidEmail(request.Email))
        {
            return Fail(400, "Adresse e-mail invalide.");
        }

        var email = Normalize(request.Email!);
        if (await _db.Users.AnyAsync(u => u.Email == email && u.Id != user.Id))
        {
            return Fail(409, "Cette adresse e-mail est déjà utilisée par un autre compte.");
        }

        user.FullName = FarmRules.Clean(request.FullName, 150);
        user.FarmName = FarmRules.Clean(request.FarmName, 150);
        user.Phone = FarmRules.Clean(request.Phone, 40);
        user.Email = email;
        user.UserName = email.Split('@')[0];
        await _db.SaveChangesAsync();
        return Ok(user.ToDto());
    }

    /// <summary>Erreur 400 (et non 401) si le mot de passe actuel est faux : 401 signifie « session expirée » pour l'application.</summary>
    [HttpPost("change-password")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == CurrentUserId);
        if (user is null)
        {
            return Unauthorized();
        }

        if (!PasswordHasher.Verify(request.CurrentPassword ?? string.Empty, user.PasswordHash, user.PasswordSalt))
        {
            return Fail(400, "Le mot de passe actuel est incorrect.");
        }

        if ((request.NewPassword ?? string.Empty).Length < MinPasswordLength)
        {
            return Fail(400, $"Le nouveau mot de passe doit contenir au moins {MinPasswordLength} caractères.");
        }

        (user.PasswordHash, user.PasswordSalt) = PasswordHasher.Hash(request.NewPassword!);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ---------------------------------------------------------------- Mot de passe oublié

    /// <summary>
    /// Réinitialisation directe par e-mail, comme dans l'application d'origine (écran « Mot de passe oublié »).
    /// ATTENTION : sans envoi d'e-mail de vérification, toute personne connaissant l'adresse peut changer le mot de passe.
    /// Désactivée par défaut (Auth:AllowDirectPasswordReset = false) ; activée seulement en développement.
    /// En production, la remplacer par un lien ou un code envoyé par e-mail.
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        if (!_config.GetValue<bool>("Auth:AllowDirectPasswordReset"))
        {
            return Fail(403, "La réinitialisation du mot de passe n'est pas disponible pour le moment.");
        }

        if (!IsValidEmail(request.Email))
        {
            return Fail(400, "Adresse e-mail invalide.");
        }

        if ((request.NewPassword ?? string.Empty).Length < MinPasswordLength)
        {
            return Fail(400, $"Le nouveau mot de passe doit contenir au moins {MinPasswordLength} caractères.");
        }

        var email = Normalize(request.Email!);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            return Fail(404, "Aucun compte n'est associé à cette adresse e-mail.");
        }

        (user.PasswordHash, user.PasswordSalt) = PasswordHasher.Hash(request.NewPassword!);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
