using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PoultryManager.Api.Controllers;

/// <summary>Base des contrôleurs protégés : identifiant de l'utilisateur connecté et réponses d'erreur.</summary>
[ApiController]
[Authorize]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Identifiant de l'utilisateur, lu dans le jeton (claim « sub »).</summary>
    protected int CurrentUserId
        => int.TryParse(User.FindFirstValue("sub"), out var id) ? id : 0;

    /// <summary>Erreur lisible : l'application affiche le champ « detail » tel quel.</summary>
    protected ObjectResult Fail(int statusCode, string message)
        => Problem(detail: message, statusCode: statusCode);
}
