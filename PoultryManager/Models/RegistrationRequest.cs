namespace PoultryManager.Models;

/// <summary>Données saisies sur l'écran 02 · Inscription.</summary>
public sealed record RegistrationRequest(
    string FullName,
    string FarmName,
    string Phone,
    string Email,
    string Password);
