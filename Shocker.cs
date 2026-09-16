using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PiShockApiLibrary;

public class Shocker(string apiKey, string shockerId, string agent = "C# PiShock Api")
{
    private static readonly HttpClient Client = new();

    public async Task Shock(float duration, int intensity)
    {
        await Activate(0, duration, intensity);
    }

    public async Task Vibrate(float duration, int intensity)
    {
        await Activate(1, duration, intensity);
    }

    public async Task Beep(float duration)
    {
        await Activate(2, duration, 0);
    }

    private async Task Activate(int mode, double duration, int intensity)
    {
        if (mode is < 0 or > 2) { throw new ArgumentOutOfRangeException(nameof(mode), mode, "Mode must be either 0 (Shock), 1 (Vibrate) or 2 (Beep)"); }
        if (duration is < 0.3 or > 10) { throw new ArgumentOutOfRangeException(nameof(duration), duration, "Duration must be between 0.3 seconds and 10 seconds."); }
        if (intensity is < 0 or > 100) { throw new ArgumentOutOfRangeException(nameof(intensity), intensity, "Intensity must be between 0 and 100."); }
        
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.pishock.com/Shockers/" + shockerId);
        request.Content = JsonContent.Create(new ShockerRequestData(mode, duration, intensity, agent));
        request.Headers.Add("X-Pishock-Api-Key", apiKey);
        
        var response = await Client.SendAsync(request);
    }

    private class ShockerRequestData(int mode, double duration, int intensity, string agent)
    {
        [JsonPropertyName("Operation")] public int Operation { get; } = mode;                                                                                                                                                                   
        [JsonPropertyName("Duration")] public int Duration { get; } = (int)Math.Floor(duration * 1000);                                                                                                                                         
        [JsonPropertyName("Intensity")] public int Intensity { get; } = intensity;                                                                                                                                                              
        [JsonPropertyName("AgentName")] public string AgentName { get; } = agent;
    }
}