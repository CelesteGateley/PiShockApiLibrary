namespace PiShockApiLibrary;

/// <summary>
/// Represents a single PiShock shocker device, regardless of which underlying API (V3 or legacy) is used to control it.
/// </summary>
public interface IShocker
{
    /// <summary>
    /// Activates the shocker to deliver a shock.
    /// </summary>
    /// <param name="duration">Duration in seconds. Must be between 0.3 and 15, or the maximum set for the shocker.</param>
    /// <param name="intensity">Intensity of the shock. Must be between 0 and 100, or the maximum set for the shocker.</param>
    /// <param name="minimumDuration">(Optional) If randomizing the duration, set this to the lower bounds. Not honored by every implementation.</param>
    /// <param name="minimumIntensity">(Optional) If randomizing the intensity, set this to the lower bounds.</param>
    Task Shock(double duration, int intensity, double? minimumDuration = null, int? minimumIntensity = null);

    /// <summary>
    /// Activates the shocker to vibrate.
    /// </summary>
    /// <param name="duration">Duration in seconds. Must be between 0.3 and 15, or the maximum set for the shocker.</param>
    /// <param name="intensity">Intensity of the vibration. Must be between 0 and 100, or the maximum set for the shocker.</param>
    /// <param name="minimumDuration">(Optional) If randomizing the duration, set this to the lower bounds. Not honored by every implementation.</param>
    /// <param name="minimumIntensity">(Optional) If randomizing the intensity, set this to the lower bounds.</param>
    Task Vibrate(double duration, int intensity, double? minimumDuration = null, int? minimumIntensity = null);

    /// <summary>
    /// Activates the shocker to beep.
    /// </summary>
    /// <param name="duration">Duration in seconds. Must be between 0.3 and 15, or the maximum set for the shocker.</param>
    /// <param name="minimumDuration">(Optional) If randomizing the duration, set this to the lower bounds. Not honored by every implementation.</param>
    Task Beep(double duration, double? minimumDuration = null);

    /// <summary>
    /// Re-fetches the shocker's capabilities/limits from the API and updates the instance in place.
    /// Implementations for which this is a no-op (e.g. because every call is already validated server-side) should document that explicitly.
    /// </summary>
    Task Refresh();
}
