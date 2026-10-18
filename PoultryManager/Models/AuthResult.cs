namespace PoultryManager.Models;

/// <summary>Résultat d'une tentative de connexion.</summary>
public sealed class AuthResult
{
    private AuthResult(bool succeeded, User? user, string? errorMessage)
    {
        Succeeded = succeeded;
        User = user;
        ErrorMessage = errorMessage;
    }

    public bool Succeeded { get; }

    public User? User { get; }

    public string? ErrorMessage { get; }

    public static AuthResult Success(User user) => new(true, user, null);

    public static AuthResult Failure(string errorMessage) => new(false, null, errorMessage);
}
