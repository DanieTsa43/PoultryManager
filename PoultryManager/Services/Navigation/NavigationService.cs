namespace PoultryManager.Services.Navigation;

/// <summary>Implémentation basée sur la navigation Shell de .NET MAUI.</summary>
public sealed class NavigationService : INavigationService
{
    public Task GoToAsync(string route, IDictionary<string, object>? parameters = null)
    {
        var shell = Shell.Current;
        if (shell is null)
        {
            return Task.CompletedTask;
        }

        return parameters is null
            ? shell.GoToAsync(route)
            : shell.GoToAsync(route, parameters);
    }

    public Task GoBackAsync()
        => Shell.Current?.GoToAsync("..") ?? Task.CompletedTask;
}
