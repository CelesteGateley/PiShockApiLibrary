namespace PiShockApiLibrary;

public static class ShockerFactory
{
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
    public static async Task<IShocker> CreateShocker(string apiKey, string shockerId, bool useIntensityAsPercentage = true, string agent = "C# PiShock Api")
    {
        try
        {
            return await V3Shocker.CreateShocker(apiKey, shockerId, useIntensityAsPercentage, agent);
        }
        catch (PishockShockerException e)
        {
            // TODO: Fallback to legacy
            throw;
        }
    }
}