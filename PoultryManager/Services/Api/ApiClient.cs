using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using PoultryManager.Constants;

namespace PoultryManager.Services.Api;

/// <summary>Implémentation de <see cref="IApiClient"/> basée sur <see cref="HttpClient"/>.</summary>
public sealed class ApiClient : IApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private readonly HttpClient _http;
    private readonly ITokenStore _tokens;

    public ApiClient(ITokenStore tokens)
    {
        _tokens = tokens;
        _http = new HttpClient
        {
            BaseAddress = new Uri(ApiSettings.BaseUrl),
            Timeout = ApiSettings.RequestTimeout
        };
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new UnspecifiedDateTimeConverter());
        return options;
    }

    public async Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, path, null, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }

        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
    }

    public async Task<byte[]> GetBytesAsync(string path, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, path, null, cancellationToken);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    public async Task<T?> PostAsync<T>(string path, object? body, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, path, body, cancellationToken);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
    }

    public async Task PostAsync(string path, object? body, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, path, body, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task<T?> PutAsync<T>(string path, object? body, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Put, path, body, cancellationToken);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Delete, path, null, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        await EnsureSuccessAsync(response);
        return true;
    }

    // ------------------------------------------------------------------ Internes

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path);

        if (!string.IsNullOrEmpty(_tokens.Token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokens.Token);
        }

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, body.GetType(), options: JsonOptions);
        }

        try
        {
            return await _http.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new ApiException(0, "Impossible de joindre le serveur. Vérifiez votre connexion.");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ApiException(0, "Le serveur met trop de temps à répondre. Veuillez réessayer.");
        }
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var message = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Votre session a expiré. Reconnectez-vous.",
            HttpStatusCode.TooManyRequests => "Trop de tentatives. Patientez une minute puis réessayez.",
            _ => "Le serveur a rencontré une erreur. Veuillez réessayer."
        };

        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemPayload>(JsonOptions);
            var detail = problem?.Detail;
            if (!string.IsNullOrWhiteSpace(detail))
            {
                message = detail;
            }
        }
        catch (Exception)
        {
            // Corps absent ou illisible : on garde le message générique.
        }

        throw new ApiException((int)response.StatusCode, message);
    }

    private sealed record ProblemPayload(string? Title, string? Detail);
}
