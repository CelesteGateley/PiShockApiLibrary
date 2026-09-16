using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PiShockApiLibrary;

/// <summary>
/// Represents a single PiShock shocker device for sending shock/vibrate/beep commands via the V3 API.
/// </summary>
public class V3Shocker : IShocker
{
    private static readonly HttpClient Client = new();
    private double _maxDuration = 15;
    private double _maxIntensity = 100;
    private bool _canBeep = true;
    private bool _canVibrate = true;
    private bool _canShock = true;
    private readonly string _apiKey;
    private readonly string _shockerId;
    private readonly bool _useIntensityAsPercentage;
    private readonly string _agent;

    private V3Shocker(string apiKey, string shockerId, bool useIntensityAsPercentage = true, string agent = "C# PiShock Api")
    {
        _apiKey = apiKey;
        _shockerId = shockerId;
        _useIntensityAsPercentage = useIntensityAsPercentage;
        _agent = agent;
    }

    /// <summary>
    /// Creates a <see cref="V3Shocker"/> for the given shocker ID, fetching its capabilities and limits from the API.
    /// </summary>
    /// <param name="apiKey">The PiShock API key used to authenticate requests.</param>
    /// <param name="shockerId">The ID of the target shocker.</param>
    /// <param name="useIntensityAsPercentage">Should intensity be a percentage of maximum, rather than a raw value (default = true)</param>
    /// <param name="agent">Name reported to the PiShock API as the calling application.</param>
    /// <returns>A <see cref="V3Shocker"/> ready to send shock/vibrate/beep commands.</returns>
    /// <exception cref="PishockAuthenticationException">
    /// Thrown when the API key is invalid/missing or lacks permission to perform the action.
    /// </exception>
    /// <exception cref="PishockShockerException">
    /// Thrown when the shocker does not exist, or does not support the V3 API.
    /// </exception>
    /// <exception cref="PishockDataException">
    /// Thrown when the PiShock API returns a response that could not be parsed as shocker info.
    /// </exception>
    /// <exception cref="PishockException">
    /// Thrown for any other unrecognized error response from the PiShock API.
    /// </exception>
    public static async Task<V3Shocker> CreateShocker(string apiKey, string shockerId, bool useIntensityAsPercentage = true, string agent = "C# PiShock Api")
    {
        var shocker = new V3Shocker(apiKey, shockerId, useIntensityAsPercentage, agent);
        await shocker.InitializeShockerDetails();
        return shocker;
    }

    /// <summary>
    /// Activates the shocker to deliver a shock.
    /// </summary>
    /// <param name="duration">Duration in seconds. Must be between 0.3 and 15, or the maximum set for the shocker.</param>
    /// <param name="intensity">Intensity of the shock. Must be between 0 and 100, or the maximum set for the shocker.</param>
    /// <param name="minimumDuration">(Optional) If randomizing the duration, set this to the lower bounds</param>
    /// <param name="minimumIntensity">(Optional) If randomizing the intensity, set this to the lower bounds</param>
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
    public async Task Shock(double duration, int intensity, double? minimumDuration = null, int? minimumIntensity = null)
    {
        if (!_canShock)
        {
            throw new PishockPermissionException($"API Key cannot perform shock operation for shocker {_shockerId}.");
        }
        await Activate(0, duration, intensity, minimumDuration, minimumIntensity);
    }

    /// <summary>
    /// Activates the shocker to vibrate.
    /// </summary>
    /// <param name="duration">Duration in seconds. Must be between 0.3 and 15, or the maximum set for the shocker.</param>
    /// <param name="intensity">Intensity of the vibration. Must be between 0 and 100, or the maximum set for the shocker.</param>
    /// <param name="minimumDuration">(Optional) If randomizing the duration, set this to the lower bounds</param>
    /// <param name="minimumIntensity">(Optional) If randomizing the intensity, set this to the lower bounds</param>
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
    public async Task Vibrate(double duration, int intensity, double? minimumDuration = null, int? minimumIntensity = null)
    {
        if (!_canVibrate)
        {
            throw new PishockPermissionException($"API Key cannot perform vibrate operation for shocker {_shockerId}.");
        }
        await Activate(1, duration, intensity, minimumDuration, minimumIntensity);
    }

    /// <summary>
    /// Activates the shocker to beep.
    /// </summary>
    /// <param name="duration">Duration in seconds. Must be between 0.3 and 15, or the maximum set for the shocker.</param>
    /// <param name="minimumDuration">(Optional) If randomizing the duration, set this to the lower bounds</param>
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
    public async Task Beep(double duration, double? minimumDuration = null)
    {
        if (!_canBeep)
        {
            throw new PishockPermissionException($"API Key cannot perform beep operation for shocker {_shockerId}.");
        }
        await Activate(2, duration, 0, minimumDuration, 0);
    }

    public Task Refresh()
    {
        // Realistic No-Op. Everything is validated properly locally, so cannot
        return Task.CompletedTask;
    }

    private async Task InitializeShockerDetails()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.pishock.com/Shockers/" + _shockerId);
        request.Headers.Add("X-Pishock-Api-Key", _apiKey);

        var response = await Client.SendAsync(request);
        var content = await GetResponseBody(response);

        if (!response.IsSuccessStatusCode)
        {
            throw response.StatusCode switch
            {
                // API Key Errors
                HttpStatusCode.Unauthorized => new PishockAuthenticationException($"Invalid/missing API key. ({content})"),
                HttpStatusCode.Forbidden => new PishockAuthenticationException($"API Key does not have permission to perform action. ({content})"),
                // Shocker not known
                HttpStatusCode.NotFound => new PishockShockerException($"Requested shocker does not exist. ({content})"),
                _ => new PishockException($"An error occured with the PiShock API. ({content})")
            };
        }
        var shockerInfo = await response.Content.ReadFromJsonAsync<ShockerInfo>();
        
        if (shockerInfo is null)
        {
            throw new PishockDataException($"Invalid Shocker Info passed. ({content})");
        }

        if (!shockerInfo.IsV3)
        {
            throw new PishockShockerException("Shocker " + _shockerId + " does not support V3 API.");
        }
        
        _maxDuration = shockerInfo.MaxDuration;
        _canShock = shockerInfo.CanShock;
        _canVibrate = shockerInfo.CanVibrate;
        _canBeep = shockerInfo.CanBeep;
        if (!_useIntensityAsPercentage)
        {
            _maxIntensity = shockerInfo.MaxIntensity;
        }
    }

    private async Task Activate(int mode, double duration, int intensity, double? minimumDuration = null, int? minimumIntensity = null)
    {
        minimumDuration ??= duration;
        minimumIntensity ??= intensity;
        if (mode is < 0 or > 2) { throw new ArgumentOutOfRangeException(nameof(mode), mode, "Mode must be either 0 (Shock), 1 (Vibrate) or 2 (Beep)"); }
        if ((duration < 0.3 || duration > _maxDuration) || (minimumDuration < 0.3 || minimumDuration > _maxDuration)) { throw new ArgumentOutOfRangeException(nameof(duration), duration, $"Duration must be between 0.3 seconds and {_maxDuration} seconds for shocker {_shockerId}."); }
        if ((intensity < 0 || intensity > _maxIntensity) || (minimumIntensity < 0 || minimumIntensity > _maxIntensity)) { throw new ArgumentOutOfRangeException(nameof(intensity), intensity, $"Intensity must be between 0 and {_maxIntensity} for shocker {_shockerId}."); }
        if (duration < minimumDuration) { throw new ArgumentOutOfRangeException(nameof(minimumDuration), minimumDuration, "Minimum duration must be less than or equal to duration."); }
        if (intensity < minimumIntensity) { throw new ArgumentOutOfRangeException(nameof(minimumIntensity), minimumIntensity, "Minimum intensity must be less than or equal to intensity."); }
        
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.pishock.com/Shockers/" + _shockerId);
        request.Content = JsonContent.Create(new ShockerRequestData(mode, duration, intensity, _agent, minimumDuration, minimumIntensity, _useIntensityAsPercentage));
        request.Headers.Add("X-Pishock-Api-Key", _apiKey);
        
        await ValidateResponse(await Client.SendAsync(request));
    }

    private async Task ValidateResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var content = await GetResponseBody(response);
        
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        throw response.StatusCode switch
        {
            // Bad API Key errors
            HttpStatusCode.Unauthorized => new PishockAuthenticationException($"Invalid/missing API key. ({content})"),
            HttpStatusCode.Forbidden => new PishockAuthenticationException($"API Key does not have permission to perform action. ({content})"),
            // PiShock Permissions Error (Shocker Not found/available etc)
            HttpStatusCode.MethodNotAllowed => new PishockPermissionException($"Cannot use requested mode with this shocker. ({content})"),
            HttpStatusCode.NotFound => new PishockPermissionException($"Requested shocker could not be found. ({content})"),
            HttpStatusCode.Gone => new PishockPermissionException($"Requested share is locked. ({content})"),
            HttpStatusCode.ServiceUnavailable => new PishockPermissionException($"Requested shocker {_shockerId} is currently paused. ({content})"),
            // Bad data passed to API
            HttpStatusCode.PreconditionFailed => new PishockDataException($"Intensity was outside acceptable range. ({content})"),
            HttpStatusCode.RequestedRangeNotSatisfiable => new PishockDataException($"Duration was outside acceptable range. ({content})"),
            // PiShock does not support V3
            HttpStatusCode.NotAcceptable => new PishockShockerException("Shocker " + _shockerId + " does not support V3 API."),
            // Fallback
            HttpStatusCode.InternalServerError => new PishockException($"An error occured with the PiShock API. ({content})"),
            _ => new PishockException($"An error occured with the PiShock API. ({content})")
        };
    }

    private async Task<string> GetResponseBody(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return "Code: " + (int)response.StatusCode + " " + response.StatusCode + ", Body: " + content;
    }

    private record ShockerInfo
    {
        [JsonPropertyName("HubId")] public int HubId { get; init; }
        [JsonPropertyName("ShockerId")] private int ShockerIdRaw { get; init; }
        public string ShockerId => ShockerIdRaw.ToString();
        [JsonPropertyName("Name")] public string Name { get; init; } = string.Empty;
        [JsonPropertyName("IsV3")] public bool IsV3 { get; init; }
        [JsonPropertyName("CanBeep")] public bool CanBeep { get; init; }
        [JsonPropertyName("CanVibrate")] public bool CanVibrate { get; init; }
        [JsonPropertyName("CanShock")] public bool CanShock { get; init; }
        [JsonPropertyName("CanPause")] public bool CanPause { get; init; }
        [JsonPropertyName("MaxDuration")] public int MaxDuration { get; init; }
        [JsonPropertyName("MaxIntensity")] public int MaxIntensity { get; init; }
    }

    private class ShockerRequestData(int mode, double duration, int intensity, string agent, double? minimumDuration = null, int? minimumIntensity = null, bool intensityAsPercentage = true)
    {
        [JsonPropertyName("Operation")] public int Operation { get; } = mode;                                                                                                                                                                   
        [JsonPropertyName("Duration")] public int Duration { get; } = (int)Math.Floor(duration * 1000);                                                                                                                                         
        [JsonPropertyName("Intensity")] public int Intensity { get; } = intensity;                                                                                                                                                                      
        [JsonPropertyName("MinimumDuration")] public int MinimumDuration { get; } = (int)Math.Floor((minimumDuration ?? duration) * 1000);                                                                                                                                         
        [JsonPropertyName("MinimumIntensity")] public int MinimumIntensity { get; } = minimumIntensity ?? intensity;                                                                                                                                                              
        [JsonPropertyName("AgentName")] public string AgentName { get; } = agent;
        [JsonPropertyName("IntensityAsPercentage")] public bool IntensityAsPercentage { get; } = intensityAsPercentage;
    }
}