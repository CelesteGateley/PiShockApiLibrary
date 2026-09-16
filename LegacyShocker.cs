using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PiShockApiLibrary;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
public class LegacyShocker : IShocker
{
    private static readonly HttpClient Client = new();
    private readonly string _apiKey;
    private readonly string _username;
    private string? _shockerId;
    private string? _shareCode;
    private readonly bool _intensityAsPercentage;
    private readonly bool _strict;
    private int _maxIntensity = 100;
    private bool _canShock = true;
    private bool _canVibrate = true;
    private bool _canBeep = true;
    private readonly string _agent;

    private LegacyShocker(string apiKey, string username, string? shockerId = null, string? shareCode = null, bool intensityAsPercentage = true, string agent = "C# PiShock Api", bool strict = false)
    {
        _apiKey = apiKey;
        _username = username;
        _shockerId = shockerId;
        _shareCode = shareCode;
        _intensityAsPercentage = intensityAsPercentage;
        _strict = strict;
        _agent = agent;
    }

    /// <summary>
    /// Creates a <see cref="LegacyShocker"/>, resolving a share code (and its capabilities/limits) from the given <paramref name="shockerId"/>
    /// and/or <paramref name="shareCode"/> via the legacy API's resolution chain.
    /// </summary>
    /// <param name="apiKey">The PiShock API key used to authenticate requests.</param>
    /// <param name="username">The PiShock account username that owns the API key.</param>
    /// <param name="shockerId">(Optional) The ID of the target shocker. At least one of <paramref name="shockerId"/> or <paramref name="shareCode"/> is required.</param>
    /// <param name="shareCode">(Optional) An exact share code to use directly. At least one of <paramref name="shockerId"/> or <paramref name="shareCode"/> is required.</param>
    /// <param name="intensityAsPercentage">Should intensity be a percentage of the share's maximum, rather than a raw value (default = true)</param>
    /// <param name="agent">Name reported to the PiShock API as the calling application.</param>
    /// <param name="strict">
    /// When <c>true</c>, throw <see cref="PishockNotSupportedException"/> instead of silently approximating requests the legacy API can't fully honor
    /// (a randomized duration, or an ambiguous share code with no explicit <paramref name="shareCode"/> given). Default <c>false</c> = "close enough is good enough".
    /// </param>
    /// <returns>A <see cref="LegacyShocker"/> ready to send shock/vibrate/beep commands.</returns>
    /// <exception cref="PishockAuthenticationException">Thrown when the API key or username is invalid.</exception>
    /// <exception cref="PishockDataException">Thrown when neither <paramref name="shockerId"/> nor <paramref name="shareCode"/> is supplied, or the user has no share codes.</exception>
    /// <exception cref="PishockShockerException">Thrown when no share matches the given <paramref name="shockerId"/>/<paramref name="shareCode"/>.</exception>
    /// <exception cref="PishockPermissionException">Thrown when every matching share is currently paused.</exception>
    /// <exception cref="PishockNotSupportedException">Thrown in <paramref name="strict"/> mode when the share code is ambiguous.</exception>
    public static async Task<LegacyShocker> GetShocker(string apiKey, string username, string? shockerId = null, string? shareCode = null, bool intensityAsPercentage = true, string agent = "C# PiShock Api", bool strict = false)
    {
        var shocker = new LegacyShocker(apiKey, username, shockerId, shareCode, intensityAsPercentage, agent, strict);
        await shocker.Refresh();
        return shocker;
    }
    
    /// <summary>
    /// Activates the shocker to deliver a shock.
    /// </summary>
    /// <param name="duration">Duration in seconds. Must be between 0.3 and 15 seconds.</param>
    /// <param name="intensity">Intensity of the shock. Must be between 0 and 100, or the maximum set for the share.</param>
    /// <param name="minimumDuration">
    /// (Optional) Ignored by the legacy API — duration is never randomized server-side. Only checked in <c>strict</c> mode, where
    /// setting it to anything other than <paramref name="duration"/> throws <see cref="PishockNotSupportedException"/>.
    /// </param>
    /// <param name="minimumIntensity">(Optional) If randomizing the intensity, set this to the lower bounds.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="duration"/> or <paramref name="intensity"/> is outside its valid range.</exception>
    /// <exception cref="PishockPermissionException">Thrown when this share code does not have permission to shock.</exception>
    /// <exception cref="PishockNotSupportedException">Thrown in <c>strict</c> mode when <paramref name="minimumDuration"/> differs from <paramref name="duration"/>.</exception>
    /// <exception cref="PishockAuthenticationException">Thrown when the API key or username is invalid.</exception>
    /// <exception cref="PishockShockerException">Thrown when the share code no longer exists.</exception>
    /// <exception cref="PishockDataException">Thrown when the intensity is rejected as out of range by the API itself.</exception>
    /// <exception cref="PishockException">Thrown for any other unrecognized response from the PiShock API.</exception>
    public async Task Shock(double duration, int intensity, double? minimumDuration = null, int? minimumIntensity = null)
    {
        if (!_canShock)
        {
            throw new PishockPermissionException($"Share code {_shareCode} does not have permission to shock.");
        }
        await Activate(0, duration, intensity, minimumDuration, minimumIntensity);
    }

    /// <summary>
    /// Activates the shocker to vibrate.
    /// </summary>
    /// <param name="duration">Duration in seconds. Must be between 0.3 and 15 seconds.</param>
    /// <param name="intensity">Intensity of the vibration. Must be between 0 and 100, or the maximum set for the share.</param>
    /// <param name="minimumDuration">
    /// (Optional) Ignored by the legacy API — duration is never randomized server-side. Only checked in <c>strict</c> mode, where
    /// setting it to anything other than <paramref name="duration"/> throws <see cref="PishockNotSupportedException"/>.
    /// </param>
    /// <param name="minimumIntensity">(Optional) If randomizing the intensity, set this to the lower bounds.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="duration"/> or <paramref name="intensity"/> is outside its valid range.</exception>
    /// <exception cref="PishockPermissionException">Thrown when this share code does not have permission to vibrate.</exception>
    /// <exception cref="PishockNotSupportedException">Thrown in <c>strict</c> mode when <paramref name="minimumDuration"/> differs from <paramref name="duration"/>.</exception>
    /// <exception cref="PishockAuthenticationException">Thrown when the API key or username is invalid.</exception>
    /// <exception cref="PishockShockerException">Thrown when the share code no longer exists.</exception>
    /// <exception cref="PishockDataException">Thrown when the intensity is rejected as out of range by the API itself.</exception>
    /// <exception cref="PishockException">Thrown for any other unrecognized response from the PiShock API.</exception>
    public async Task Vibrate(double duration, int intensity, double? minimumDuration = null, int? minimumIntensity = null)
    {
        if (!_canVibrate)
        {
            throw new PishockPermissionException($"Share code {_shareCode} does not have permission to vibrate.");
        }
        await Activate(1, duration, intensity, minimumDuration, minimumIntensity);
    }

    /// <summary>
    /// Activates the shocker to beep.
    /// </summary>
    /// <param name="duration">Duration in seconds. Must be between 0.3 and 15 seconds.</param>
    /// <param name="minimumDuration">
    /// (Optional) Ignored by the legacy API — duration is never randomized server-side. Only checked in <c>strict</c> mode, where
    /// setting it to anything other than <paramref name="duration"/> throws <see cref="PishockNotSupportedException"/>.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="duration"/> is outside its valid range.</exception>
    /// <exception cref="PishockPermissionException">Thrown when this share code does not have permission to beep.</exception>
    /// <exception cref="PishockNotSupportedException">Thrown in <c>strict</c> mode when <paramref name="minimumDuration"/> differs from <paramref name="duration"/>.</exception>
    /// <exception cref="PishockAuthenticationException">Thrown when the API key or username is invalid.</exception>
    /// <exception cref="PishockShockerException">Thrown when the share code no longer exists.</exception>
    /// <exception cref="PishockException">Thrown for any other unrecognized response from the PiShock API.</exception>
    public async Task Beep(double duration, double? minimumDuration = null)
    {
        if (!_canBeep)
        {
            throw new PishockPermissionException($"Share code {_shareCode} does not have permission to beep.");
        }
        await Activate(2, duration, 0, minimumDuration);
    }

    /// <summary>
    /// Re-runs the legacy resolution chain (<see cref="GetUserId"/> → <see cref="GetShareCodeIds"/> → <see cref="GetShareInfo"/> → <see cref="GetShareCode"/>)
    /// and updates this instance's share code and cached capabilities/limits in place. Unlike <see cref="V3Shocker"/>, this is not a no-op: legacy's
    /// <c>Operate</c> endpoint doesn't validate duration server-side and capability data (e.g. <c>maxIntensity</c>) can drift after construction.
    /// </summary>
    /// <exception cref="PishockAuthenticationException">Thrown when the API key or username is invalid.</exception>
    /// <exception cref="PishockDataException">Thrown when the user has no share codes.</exception>
    /// <exception cref="PishockShockerException">Thrown when no share matches this instance's shocker ID/share code.</exception>
    /// <exception cref="PishockPermissionException">Thrown when every matching share is currently paused.</exception>
    /// <exception cref="PishockNotSupportedException">Thrown in <c>strict</c> mode when the share code is ambiguous.</exception>
    public async Task Refresh()
    {
        var userId = await GetUserId();
        var share = GetShareCode(await GetShareInfo(userId, await GetShareCodeIds(userId)));
        if (share == null)
        {
            throw new PishockShockerException("Could not find a valid share code to use");
        }
        UpdateFields(share);
    }

    private async Task Activate(int mode, double duration, int intensity, double? minimumDuration = null, int? minimumIntensity = null)
    {
        minimumDuration ??= duration;
        
        if (_shareCode == null) { throw new PishockShockerException("No share code is available for operation."); }
        if (_strict && !Equals(minimumDuration, duration)) { throw new PishockNotSupportedException("Legacy Shocker API does not support a randomized duration. Leave minimum duration empty, or set it to duration."); }
        
        if (mode is < 0 or > 2) { throw new ArgumentOutOfRangeException(nameof(mode), mode, "Mode must be either 0 (Shock), 1 (Vibrate) or 2 (Beep)"); }
        if (duration is < 0.3 or > 15) { throw new ArgumentOutOfRangeException(nameof(duration), duration, $"Duration must be between 0.3 seconds and 15 seconds for shocker {_shareCode}."); }
        if ((intensity < 0 || intensity > _maxIntensity) || (minimumIntensity < 0 || minimumIntensity > _maxIntensity)) { throw new ArgumentOutOfRangeException(nameof(intensity), intensity, $"Intensity must be between 0 and {_maxIntensity} for shocker {_shareCode}."); }

        var randomize = !(minimumIntensity == null || minimumIntensity == intensity);
        
        var request = new HttpRequestMessage(HttpMethod.Post, "https://ps.pishock.com/PiShock/Operate");
        request.Content = JsonContent.Create(new OperateRequest(_apiKey, _username, _shareCode, mode, duration, intensity, _agent, randomize, _intensityAsPercentage));
        var response = await Client.SendAsync(request);
        var content = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync());

        switch (content)
        {
            case "Operation Attempted.":
                break;
            case "Not Authorized.":
                throw new PishockAuthenticationException("You do not have permission to operate this shocker using those credentials");
            case "This code doesn't exist.":
                throw new PishockShockerException($"Share code {_shareCode} does not exist.");
            case var s when s.StartsWith("Intensity must be between 0 and"):
                throw new PishockDataException($"Intensity out of range, please refresh the internal shocker. ({content})");
            default:
                throw new PishockException($"An unknown error has occurred. ({content})");
        }
    }

    private async Task<int> GetUserId()
    {
        var url = "https://auth.pishock.com/Auth/GetUserIfAPIKeyValid?apikey=" + Uri.EscapeDataString(_apiKey) + "&username=" + Uri.EscapeDataString(_username);
        var response = await Client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new PishockAuthenticationException("The provided username or api key are invalid.");
        }
        
        var data = JsonSerializer.Deserialize<UserIdResponse>(content);
        return data?.UserId ?? throw new PishockException("The API returned invalid data.");
    }

    private async Task<int[]> GetShareCodeIds(int userId)
    {
        var url = "https://ps.pishock.com/PiShock/GetShareCodesByOwner?api=true&Token=" + Uri.EscapeDataString(_apiKey) + "&UserId=" + Uri.EscapeDataString(userId.ToString());
        var response = await Client.GetAsync(url);
        var data = JsonSerializer.Deserialize<Dictionary<string, List<int>>>(await response.Content.ReadAsStringAsync());

        var shareIds = data?.Values.SelectMany(ids => ids).ToArray() ?? [];
        return shareIds.Length > 0 ? shareIds : throw new PishockDataException("User does not have any available share codes for usage linked to their username");
    }

    private async Task<ShareInfo[]> GetShareInfo(int userId, int[] shareIds)
    {
        var shareQuery = string.Join("", shareIds.Select(id => "&shareIds=" + id));
        var url = "https://ps.pishock.com/PiShock/GetShockersByShareIds?api=true&Token=" + Uri.EscapeDataString(_apiKey) 
            + "&UserId=" + Uri.EscapeDataString(userId.ToString()) 
            + shareQuery;
        var response = await Client.GetAsync(url);
        var data = JsonSerializer.Deserialize<Dictionary<string, List<ShareInfo>>>(await response.Content.ReadAsStringAsync());
        
        return data?.Values.SelectMany(shares => shares).ToArray() ?? [];
    }
    
    private ShareInfo? GetShareCode(ShareInfo[] shares, ShareKeySelectionMode mode = ShareKeySelectionMode.First)
    {
        if (_shockerId == null && _shareCode == null)
        {
            throw new PishockDataException("Cannot locate shocker without share code or shocker id");
        }

        var candidates = shares
            .Where(s => _shockerId == null || s.ShockerId.ToString() == _shockerId)
            .Where(s => _shareCode == null || s.ShareCode == _shareCode)
            .ToArray();

        if (candidates.Length == 0)
        {
            throw new PishockShockerException("Unable to locate a shocker with the provided share code and/or shocker id");
        }

        var usable = candidates.Where(s => !s.IsPaused).ToArray();
        switch (usable.Length)
        {
            case 0:
                throw new PishockPermissionException("The only matching share code(s) for this shocker are currently paused");
            case 1:
                return usable[0];
            default:
            {
                if (_strict && _shareCode == null) { throw new PishockNotSupportedException("Multiple share codes are available for this shocker; strict mode requires an explicit share code."); }
                
                var best = usable.OrderByDescending(s => s.CanShock)
                    .ThenByDescending(s => s.CanVibrate)
                    .ThenByDescending(s => s.CanBeep)
                    .ThenByDescending(s => s.MaxIntensity);

                return mode switch
                {
                    ShareKeySelectionMode.First => usable.FirstOrDefault(),
                    ShareKeySelectionMode.MostPermissive => best.FirstOrDefault(),
                    ShareKeySelectionMode.LeastPermissive => best.LastOrDefault(),
                    _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Invalid key selection mode provided.")
                };
            }
        }
    }

    private void UpdateFields(ShareInfo info)
    {
        _shareCode = info.ShareCode;
        _shockerId = info.ShockerId.ToString();
        _maxIntensity = _intensityAsPercentage ? 100 : info.MaxIntensity;
        _canBeep = info.CanBeep;
        _canVibrate = info.CanVibrate;
        _canShock = info.CanShock;
    }

    public enum ShareKeySelectionMode
    {
        First = 0,
        MostPermissive = 1,
        LeastPermissive = 2
    }

    private record UserIdResponse
    {
        [JsonPropertyName("UserId")] public int UserId { get; init; }
    }

    private record ShareInfo
    {
        [JsonPropertyName("shareId")] public int ShareId { get; init; }
        [JsonPropertyName("clientId")] public int ClientId { get; init; }
        [JsonPropertyName("shockerId")] public int ShockerId { get; init; }
        [JsonPropertyName("shockerName")] public string ShockerName { get; init; } = string.Empty;
        [JsonPropertyName("isPaused")] public bool IsPaused { get; init; }
        [JsonPropertyName("maxIntensity")] public int MaxIntensity { get; init; }
        [JsonPropertyName("canContinuous")] public bool CanContinuous { get; init; }
        [JsonPropertyName("canShock")] public bool CanShock { get; init; }
        [JsonPropertyName("canVibrate")] public bool CanVibrate { get; init; }
        [JsonPropertyName("canBeep")] public bool CanBeep { get; init; }
        [JsonPropertyName("canLog")] public bool CanLog { get; init; }
        [JsonPropertyName("shareCode")] public string ShareCode { get; init; } = string.Empty;
    }

    private class OperateRequest(string apikey, string username, string code, int op, double duration, int intensity, string? name = null, bool random = false, bool scale = true)
    {
        [JsonPropertyName("code")] public string Code { get; } = code;
        [JsonPropertyName("duration")] public int Duration { get; } = (int)Math.Floor(duration);
        [JsonPropertyName("intensity")] public int Intensity { get; } = intensity;
        [JsonPropertyName("op")] public int Op { get; } = op;
        [JsonPropertyName("apikey")] public string ApiKey { get; } = apikey;
        [JsonPropertyName("username")] public string Username { get; } = username;
        [JsonPropertyName("name")] public string? Name { get; } = name;
        [JsonPropertyName("random")] public bool Random { get; } = random;
        [JsonPropertyName("scale")] public bool Scale { get; } = scale;
    }
}