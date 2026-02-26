
using Extensions;

await App.Main();

public class App
{
    public static int Loader_UDPPortRetransmission = 11; // заглушка импорта

    public static async Task Main()
    {
        Console.WriteLine("Hello, World!");
        Console.WriteLine($"Choose the program, press number:\n\t 1 = PC Pass Alarm Simulator\n\t 2 = OLD PC Pass Alarm Simulator\n\t 3 = PC Validator\n\t 0 = Exit\n");
        
        var programNumber = "?";
        IStart task = null;

        while (programNumber != "0")
        {
            programNumber = Console.ReadLine();

            switch (programNumber)
            {
                case "1":
                    task = new PassAlarmSimulator.PassAlarmSimulator();
                    break;
                case "2":
                    task = new PassAlarmSimulator.PassAlarmSimulator(true);
                    break;
                case "3":
                    task = new Validator.Validator();
                    break;
                case "0":
                    task?.Shutdown();
                    Console.WriteLine("Goodbye, World!");
                    Environment.Exit(0);
                    break;
                default:
                    break;
            }

            task?.Start().Wait();
        }
    }
}
