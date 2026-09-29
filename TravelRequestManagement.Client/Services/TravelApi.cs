using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TravelManagement.Services.Shared.DTOs;

namespace TravelRequestManagement.Client.Services;

public sealed partial class TravelApi(HttpClient http) : ITravelApi
{
    public Task<CurrentUserDto> MeAsync() =>
        Get<CurrentUserDto>("api/me");

  

    private async Task<T> Get<T>(string url)
    {
        var response = await http.GetAsync(url);
        await Ensure(response);
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    private async Task<IReadOnlyList<T>> GetList<T>(string url)
    {
        var response = await http.GetAsync(url);
        await Ensure(response);
        return await response.Content.ReadFromJsonAsync<List<T>>() ?? [];
    }

    private async Task<T> Post<T>(string url, object? body)
    {
        var response = await http.PostAsJsonAsync(url, body);
        await Ensure(response);
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    private async Task PostVoid(string url, object? body)
    {
        var response = await http.PostAsJsonAsync(url, body);
        await Ensure(response);
    }

    private async Task<T> Put<T>(string url, object body)
    {
        var response = await http.PutAsJsonAsync(url, body);
        await Ensure(response);
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    private async Task Delete(string url)
    {
        var response = await http.DeleteAsync(url);
        await Ensure(response);
    }

    private static async Task Ensure(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var body = await response.Content.ReadAsStringAsync();
        throw new ApiException(ExtractMessage(body, response.StatusCode), response.StatusCode);
    }

    private static string ExtractMessage(string body, HttpStatusCode status)
    {
        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object
                    && root.TryGetProperty("message", out var messageProp)
                    && messageProp.ValueKind == JsonValueKind.String
                    && !string.IsNullOrWhiteSpace(messageProp.GetString()))
                {
                    return messageProp.GetString()!;
                }

               
                if (root.ValueKind == JsonValueKind.Object
                    && root.TryGetProperty("errors", out var errorsProp)
                    && errorsProp.ValueKind == JsonValueKind.Object)
                {
                    var reasons = errorsProp.EnumerateObject()
                        .SelectMany(field => field.Value.ValueKind == JsonValueKind.Array
                            ? field.Value.EnumerateArray().Select(v => v.GetString()).Where(v => !string.IsNullOrWhiteSpace(v))
                            : Enumerable.Empty<string?>())
                        .ToList();

                    if (reasons.Count > 0) return string.Join(" ", reasons);
                }
            }
            catch (JsonException)
            {
                
            }
        }

        return DefaultMessageFor(status);
    }

    private static string DefaultMessageFor(HttpStatusCode status) => status switch
    {
        HttpStatusCode.Unauthorized => "Your session has expired. Please sign in again.",
        HttpStatusCode.Forbidden => "You don't have permission to do that.",
        HttpStatusCode.NotFound => "The item you were looking for could not be found. It may have been changed or removed.",
        HttpStatusCode.BadRequest => "That request couldn't be completed. Please check the details and try again.",
        HttpStatusCode.Conflict => "That couldn't be completed because it conflicts with the current data. Please refresh and try again.",
        _ => "Something went wrong while processing your request. Please try again, and contact support if the problem continues."
    };
}
