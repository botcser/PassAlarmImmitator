await App.Main();

public class App
{
    public static int Loader_UDPPortRetransmission = 11; // заглушка импорта

    public static async Task Main()
    {
        Console.WriteLine("Hello, World!");
        Console.WriteLine($"I assume that the response files are in the directory {Directory.GetCurrentDirectory()}/<MatreshkaSimulator or/and ImpulseSimulator>");

        var cs = new CancellationTokenSource();

        PassAlarmSimulator.PassAlarmSimulator.Run();

        Console.WriteLine("Goodbye, World!");
    }
}
