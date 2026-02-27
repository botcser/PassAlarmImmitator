using Extensions;
using PassAlarmSimulator.Device.Impulse.ImpulseSimulator;
using PassAlarmSimulator.Device.Matreshka.MatreshkaSimulator;
using PassAlarmSimulator.Device.Simulator;

namespace PassAlarmSimulator
{
    public class PassAlarmSimulator : IStart
    {
        private DeviceSimulator _simulator;

        public Task Start()
        {
            Console.WriteLine($"PassAlarmSimulator: I assume that the response files are in the directory {Directory.GetCurrentDirectory()}/<MatreshkaSimulator or/and ImpulseSimulator>");

            var task = new Task(() =>
            {
                Console.WriteLine("\t1 - Matreshka\n\t2 - Impulse");

                switch (Console.ReadLine())
                {
                    case "1":
                        _simulator = new MatreshkaSimulator();
                        _simulator.Start();
                        break;
                    case "2":
                        _simulator = new ImpulseSimulator();
                        _simulator.Start();
                        break;
                    default:
                        break;
                }
            });

            task.Start();

            return task;
        }

        public void Shutdown()
        {
            _simulator.Shutdown();
        }
    }
}
