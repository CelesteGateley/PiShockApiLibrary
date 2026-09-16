namespace PiShockApiLibrary.Sandbox;

public static class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: {api_key} {shocker_id} {username} [share_code]");
            return;
        }
        var apiKey = args[0];
        var shockerId = args[1];
        var username = args[2];
        string? shareCode = null;
        if (args.Length > 3)
        {
            shareCode = args[3];
        }

        var shocker = await ShockerFactory.CreateLegacyShocker(apiKey, username, shockerId, shareCode);
        await shocker.Beep(1);
    }
}