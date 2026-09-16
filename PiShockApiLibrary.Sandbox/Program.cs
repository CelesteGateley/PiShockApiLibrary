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
        string apiKey = args[0];
        string shockerId = args[1];
        
        Console.WriteLine("Api Key: " + apiKey);
        Console.WriteLine("ShockerId: " + shockerId);
        
        var shocker = new Shocker(apiKey, shockerId);
        await shocker.Vibrate(1, 100);
    }
}