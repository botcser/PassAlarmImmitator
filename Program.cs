
await App.Main();

public class App
{
    public static int Loader_UDPPortRetransmission = 11; // заглушка импорта

    public static async Task Main()
    {
        Console.WriteLine("Hello, World!");
        Console.WriteLine($"Choose the program, press number:\n\t 1 = PC Pass Alarm Simulator\n\t 2 = PC Validator\n\t 0 = Exit\n");
        
        var programNumber = "?";

        while (programNumber != "0")
        {
            programNumber = Console.ReadLine();

            switch (programNumber)
            {
                case "1":
                    var task1 = new PassAlarmSimulator.PassAlarmSimulator();
                    task1.Start().Wait();
                    Environment.Exit(0);
                    break;
                case "2":
                    var task2 = new Validator.Validator();
                    task2.Start().Wait();
                    Environment.Exit(0);
                    break;
                case "0":
                    break;
                default:
                    break;
            }
        }
        
        Console.WriteLine("Goodbye, World!");
    }
}
