namespace PiShockApiLibrary;

public interface IShocker
{
    Task Shock(double duration, int intensity, double? minimumDuration = null, int? minimumIntensity = null);
    Task Vibrate(double duration, int intensity, double? minimumDuration = null, int? minimumIntensity = null);
    Task Beep(double duration, double? minimumDuration = null);
    Task Refresh();
}