namespace PiShockApiLibrary;

public static class ShockerFactory
{
    /// <summary>
    /// Creates an <see cref="IShocker"/> for the given shocker ID, preferring the V3 API and transparently falling back to the
    /// legacy API (via <see cref="CreateLegacyShocker"/>) when the shocker doesn't support V3.
    /// </summary>
    /// <param name="apiKey">The PiShock API key used to authenticate requests.</param>
    /// <param name="username">The PiShock account username that owns the API key. Only used if falling back to legacy.</param>
    /// <param name="shockerId">The ID of the target shocker.</param>
    /// <param name="shareCode">(Optional) An exact share code to use if falling back to legacy, bypassing lookup.</param>
    /// <param name="useIntensityAsPercentage">Should intensity be a percentage of maximum, rather than a raw value (default = true)</param>
    /// <param name="agent">Name reported to the PiShock API as the calling application.</param>
    /// <param name="strict">Only used if falling back to legacy — see <see cref="LegacyShocker.GetShocker"/>.</param>
    /// <returns>A <see cref="V3Shocker"/> or <see cref="LegacyShocker"/> ready to send shock/vibrate/beep commands.</returns>
    /// <exception cref="PishockAuthenticationException">Thrown when the API key/username is invalid/missing or lacks permission to perform the action.</exception>
    /// <exception cref="PishockShockerException">Thrown when the shocker does not exist under either API.</exception>
    /// <exception cref="PishockDataException">Thrown when the PiShock API returns a response that could not be parsed, or (legacy fallback) no share codes are available.</exception>
    /// <exception cref="PishockPermissionException">Thrown (legacy fallback) when every matching share is currently paused.</exception>
    /// <exception cref="PishockNotSupportedException">Thrown (legacy fallback) in <paramref name="strict"/> mode when the share code is ambiguous.</exception>
    /// <exception cref="PishockException">Thrown for any other unrecognized error response from the PiShock API.</exception>
    public static async Task<IShocker> CreateShocker(string apiKey, string username, string shockerId, string? shareCode = null, bool useIntensityAsPercentage = true, string agent = "C# PiShock Api", bool strict = false)
    {
        try
        {
            return await CreateV3Shocker(apiKey, shockerId, useIntensityAsPercentage, agent);
        }
        catch (PishockShockerException)
        {
            return await CreateLegacyShocker(apiKey, username , shockerId, shareCode, useIntensityAsPercentage, strict, agent);
        }
    }

    /// <summary>
    /// Creates a <see cref="V3Shocker"/> for the given shocker ID, fetching its capabilities and limits from the API.
    /// Use this instead of <see cref="CreateShocker"/> to force V3 with no legacy fallback.
    /// </summary>
    /// <param name="apiKey">The PiShock API key used to authenticate requests.</param>
    /// <param name="shockerId">The ID of the target shocker.</param>
    /// <param name="useIntensityAsPercentage">Should intensity be a percentage of maximum, rather than a raw value (default = true)</param>
    /// <param name="agent">Name reported to the PiShock API as the calling application.</param>
    /// <returns>A <see cref="V3Shocker"/> ready to send shock/vibrate/beep commands.</returns>
    /// <exception cref="PishockAuthenticationException">Thrown when the API key is invalid/missing or lacks permission to perform the action.</exception>
    /// <exception cref="PishockShockerException">Thrown when the shocker does not exist, or does not support the V3 API.</exception>
    /// <exception cref="PishockDataException">Thrown when the PiShock API returns a response that could not be parsed as shocker info.</exception>
    /// <exception cref="PishockException">Thrown for any other unrecognized error response from the PiShock API.</exception>
    public static async Task<IShocker> CreateV3Shocker(string apiKey, string shockerId, bool useIntensityAsPercentage = true, string agent = "C# PiShock Api")
    {
        return await V3Shocker.CreateShocker(apiKey, shockerId, useIntensityAsPercentage, agent);
    }

    /// <summary>
    /// Creates a <see cref="LegacyShocker"/>, resolving a share code from the given <paramref name="shockerId"/> and/or <paramref name="shareCode"/>.
    /// Use this instead of <see cref="CreateShocker"/> to force legacy with no V3 attempt.
    /// </summary>
    /// <param name="apiKey">The PiShock API key used to authenticate requests.</param>
    /// <param name="username">The PiShock account username that owns the API key.</param>
    /// <param name="shockerId">(Optional) The ID of the target shocker. At least one of <paramref name="shockerId"/> or <paramref name="shareCode"/> is required.</param>
    /// <param name="shareCode">(Optional) An exact share code to use directly. At least one of <paramref name="shockerId"/> or <paramref name="shareCode"/> is required.</param>
    /// <param name="useIntensityAsPercentage">Should intensity be a percentage of the share's maximum, rather than a raw value (default = true)</param>
    /// <param name="strict">
    /// When <c>true</c>, throw <see cref="PishockNotSupportedException"/> instead of silently approximating requests the legacy API can't fully honor.
    /// </param>
    /// <param name="agent">Name reported to the PiShock API as the calling application.</param>
    /// <returns>A <see cref="LegacyShocker"/> ready to send shock/vibrate/beep commands.</returns>
    /// <exception cref="PishockAuthenticationException">Thrown when the API key or username is invalid.</exception>
    /// <exception cref="PishockDataException">Thrown when neither <paramref name="shockerId"/> nor <paramref name="shareCode"/> is supplied, or the user has no share codes.</exception>
    /// <exception cref="PishockShockerException">Thrown when no share matches the given <paramref name="shockerId"/>/<paramref name="shareCode"/>.</exception>
    /// <exception cref="PishockPermissionException">Thrown when every matching share is currently paused.</exception>
    /// <exception cref="PishockNotSupportedException">Thrown in <paramref name="strict"/> mode when the share code is ambiguous.</exception>
    public static async Task<IShocker> CreateLegacyShocker(string apiKey, string username, string? shockerId = null, string? shareCode = null, bool useIntensityAsPercentage = true, bool strict = false, string agent = "C# PiShock Api")
    {
        return await LegacyShocker.GetShocker(apiKey, username, shockerId, shareCode, useIntensityAsPercentage, agent, strict);
    }
}