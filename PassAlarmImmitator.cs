using Extensions;
using IRAPROM.MyCore.Device.Matreshka.XGOST;
using PassAlarmSimulator.Device.Impulse.ImpulseSimulator;
using PassAlarmSimulator.Device.Matreshka.MatreshkaSimulator;
using PassAlarmSimulator.Device.Matreshka.XGOST;
using PassAlarmSimulator.Device.Matreshka.XGOST.XPROGOSTSimulator;
using PassAlarmSimulator.Device.Simulator;

namespace PassAlarmSimulator
{
    public class PassAlarmSimulator : IStart
    {
        private DeviceSimulator _simulator;

        public Task Start()
        {
            Console.WriteLine($"PassAlarmSimulator: I assume that the response files are in the directory {Directory.GetCurrentDirectory()}/<MatreshkaSimulator or ImpulseSimulator or XPROGOSTSimulator>");

            var task = new Task(() =>
            {
                Console.WriteLine("\t1 - Matreshka\n\t2 - Impulse\n\t3 - XPROGOST");

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
                    case "3":
                        _simulator = new XPROGOSTSimulator();
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
