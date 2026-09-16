using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PiShockApiLibrary;

public class Shocker(string apiKey, string shockerId, string agent = "C# PiShock Api")
{
    private static readonly HttpClient Client = new();

    public async Task Shock(float duration, int intensity)
    {
        await ActivateV3(0, duration, intensity);
    }

    public async Task Vibrate(float duration, int intensity)
    {
        await ActivateV3(1, duration, intensity);
    }

    public async Task Beep(float duration)
    {
        await ActivateV3(2, duration, 0);
    }

    private async Task ActivateV3(int mode, double duration, int intensity)
    {
        if (mode is < 0 or > 2) { throw new ArgumentOutOfRangeException(nameof(mode), mode, "Mode must be either 0 (Shock), 1 (Vibrate) or 2 (Beep)"); }
        if (duration is < 0.3 or > 15) { throw new ArgumentOutOfRangeException(nameof(duration), duration, "Duration must be between 0.3 seconds and 15 seconds."); }
        if (intensity is < 0 or > 100) { throw new ArgumentOutOfRangeException(nameof(intensity), intensity, "Intensity must be between 0 and 100."); }
        
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.pishock.com/Shockers/" + shockerId);
        request.Content = JsonContent.Create(new ShockerRequestData(mode, duration, intensity, agent));
        request.Headers.Add("X-Pishock-Api-Key", apiKey);
        
        ValidateV3Response(await Client.SendAsync(request));
    }

    private void ValidateV3Response(HttpResponseMessage response)
    {
        Console.WriteLine(response);
        if (response.IsSuccessStatusCode) return;

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (response.StatusCode)
        {
            // Bad API Key errors
            case HttpStatusCode.Unauthorized:
                throw new PishockAuthenticationException($"Invalid/missing API key. ({response.Content})");
            case HttpStatusCode.Forbidden:
                throw new PishockAuthenticationException($"API Key does not have permission to perform action. ({response.Content})");
                
            // PiShock Permissions Error (Shocker Not found/available etc)
            case HttpStatusCode.MethodNotAllowed:
                throw new PishockPermissionException($"Cannot use requested mode with this shocker. ({response.Content})");
            case HttpStatusCode.NotFound:
                throw new PishockPermissionException($"Requested shocker could not be found. ({response.Content})");
            case HttpStatusCode.Gone:
                throw new PishockPermissionException($"Requested share is locked. ({response.Content})");
            case HttpStatusCode.ServiceUnavailable:
                throw new PishockPermissionException($"Requested shocker {shockerId} is currently paused. ({response.Content})");
                
            // Bad data passed to API
            case HttpStatusCode.PreconditionFailed:
                throw new PishockDataException($"Intensity was outside acceptable range. ({response.Content})");
            case HttpStatusCode.RequestedRangeNotSatisfiable:
                throw new PishockDataException($"Duration was outside acceptable range. ({response.Content})");
                
            // PiShock does not support V3
            case HttpStatusCode.NotAcceptable:
                throw new PishockShockerException("Shocker " + shockerId + " does not support V3 API.");
                
            // Fallback
            case HttpStatusCode.InternalServerError:
            default:
                throw new PishockException($"An error occured with the PiShock API. ({response.Content})");
        }
    }

    private class ShockerRequestData(int mode, double duration, int intensity, string agent)
    {
        [JsonPropertyName("Operation")] public int Operation { get; } = mode;                                                                                                                                                                   
        [JsonPropertyName("Duration")] public int Duration { get; } = (int)Math.Floor(duration * 1000);                                                                                                                                         
        [JsonPropertyName("Intensity")] public int Intensity { get; } = intensity;                                                                                                                                                              
        [JsonPropertyName("AgentName")] public string AgentName { get; } = agent;
    }
}