using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PiShockApiLibrary;

/// <summary>
/// Represents a single PiShock shocker device for sending shock/vibrate/beep commands via the V3 API.
/// </summary>
/// <param name="apiKey">The PiShock API key used to authenticate requests.</param>
/// <param name="shockerId">The ID of the target shocker.</param>
/// <param name="agent">Name reported to the PiShock API as the calling application.</param>
public class Shocker(string apiKey, string shockerId, string agent = "C# PiShock Api")
{
    private static readonly HttpClient Client = new();

    /// <summary>
    /// Activates the shocker to deliver a shock.
    /// </summary>
    /// <param name="duration">Duration in seconds. Must be between 0.3 and 15.</param>
    /// <param name="intensity">Intensity of the shock. Must be between 0 and 100.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="duration"/> or <paramref name="intensity"/> is outside its valid range.
    /// </exception>
    /// <exception cref="PishockAuthenticationException">
    /// Thrown when the API key is invalid/missing or lacks permission to perform the action.
    /// </exception>
    /// <exception cref="PishockPermissionException">
    /// Thrown when the shocker can't be found, the share is locked, or the shocker is paused.
    /// </exception>
    /// <exception cref="PishockDataException">
    /// Thrown when the PiShock API rejects the intensity or duration as out of range.
    /// </exception>
    /// <exception cref="PishockShockerException">
    /// Thrown when the shocker does not support the V3 API.
    /// </exception>
    /// <exception cref="PishockException">
    /// Thrown for any other unrecognized error response from the PiShock API.
    /// </exception>
    public async Task Shock(float duration, int intensity)
    {
        await ActivateV3(0, duration, intensity);
    }

    /// <summary>
    /// Activates the shocker to vibrate.
    /// </summary>
    /// <param name="duration">Duration in seconds. Must be between 0.3 and 15.</param>
    /// <param name="intensity">Intensity of the vibration. Must be between 0 and 100.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="duration"/> or <paramref name="intensity"/> is outside its valid range.
    /// </exception>
    /// <exception cref="PishockAuthenticationException">
    /// Thrown when the API key is invalid/missing or lacks permission to perform the action.
    /// </exception>
    /// <exception cref="PishockPermissionException">
    /// Thrown when the shocker can't be found, the share is locked, or the shocker is paused.
    /// </exception>
    /// <exception cref="PishockDataException">
    /// Thrown when the PiShock API rejects the intensity or duration as out of range.
    /// </exception>
    /// <exception cref="PishockShockerException">
    /// Thrown when the shocker does not support the V3 API.
    /// </exception>
    /// <exception cref="PishockException">
    /// Thrown for any other unrecognized error response from the PiShock API.
    /// </exception>
    public async Task Vibrate(float duration, int intensity)
    {
        await ActivateV3(1, duration, intensity);
    }

    /// <summary>
    /// Activates the shocker to beep.
    /// </summary>
    /// <param name="duration">Duration in seconds. Must be between 0.3 and 15.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="duration"/> is outside its valid range.
    /// </exception>
    /// <exception cref="PishockAuthenticationException">
    /// Thrown when the API key is invalid/missing or lacks permission to perform the action.
    /// </exception>
    /// <exception cref="PishockPermissionException">
    /// Thrown when the shocker can't be found, the share is locked, or the shocker is paused.
    /// </exception>
    /// <exception cref="PishockDataException">
    /// Thrown when the PiShock API rejects the duration as out of range.
    /// </exception>
    /// <exception cref="PishockShockerException">
    /// Thrown when the shocker does not support the V3 API.
    /// </exception>
    /// <exception cref="PishockException">
    /// Thrown for any other unrecognized error response from the PiShock API.
    /// </exception>
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
        
        await ValidateV3Response(await Client.SendAsync(request));
    }

    private async Task ValidateV3Response(HttpResponseMessage response)
    {
        Console.WriteLine(response);
        if (response.IsSuccessStatusCode) return;

        var content = await response.Content.ReadAsStringAsync();
        content = "Code: " + (int)response.StatusCode + " " + response.StatusCode + ", Body: " + content;
        
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (response.StatusCode)
        {
            // Bad API Key errors
            case HttpStatusCode.Unauthorized:
                throw new PishockAuthenticationException($"Invalid/missing API key. ({content})");
            case HttpStatusCode.Forbidden:
                throw new PishockAuthenticationException($"API Key does not have permission to perform action. ({content})");
                
            // PiShock Permissions Error (Shocker Not found/available etc)
            case HttpStatusCode.MethodNotAllowed:
                throw new PishockPermissionException($"Cannot use requested mode with this shocker. ({content})");
            case HttpStatusCode.NotFound:
                throw new PishockPermissionException($"Requested shocker could not be found. ({content})");
            case HttpStatusCode.Gone:
                throw new PishockPermissionException($"Requested share is locked. ({content})");
            case HttpStatusCode.ServiceUnavailable:
                throw new PishockPermissionException($"Requested shocker {shockerId} is currently paused. ({content})");
                
            // Bad data passed to API
            case HttpStatusCode.PreconditionFailed:
                throw new PishockDataException($"Intensity was outside acceptable range. ({content})");
            case HttpStatusCode.RequestedRangeNotSatisfiable:
                throw new PishockDataException($"Duration was outside acceptable range. ({content})");
                
            // PiShock does not support V3
            case HttpStatusCode.NotAcceptable:
                throw new PishockShockerException("Shocker " + shockerId + " does not support V3 API.");
                
            // Fallback
            case HttpStatusCode.InternalServerError:
            default:
                throw new PishockException($"An error occured with the PiShock API. ({content})");
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