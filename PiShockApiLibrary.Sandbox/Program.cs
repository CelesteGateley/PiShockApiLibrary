namespace PiShockApiLibrary.Sandbox;

public static class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: {api_key} {shocker_id}");
            return;
        }
        var apiKey = args[0];
        var shockerId = args[1];
        
        Console.WriteLine("Api Key: " + apiKey);
        Console.WriteLine("ShockerId: " + shockerId);
        
        var shocker = await ShockerFactory.CreateShocker(apiKey, shockerId);
        await shocker.Vibrate(2, 5, 1, 1);
        await shocker.Beep(2, 1);
    }
}