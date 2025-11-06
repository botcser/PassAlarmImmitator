using Device.Matreshka.Simulator;
using Extensions;

namespace PassAlarmSimulator
{
    public class PassAlarmSimulator : IStart
    {
        public Task Start()
        {
            Console.WriteLine($"PassAlarmSimulator: I assume that the response files are in the directory {Directory.GetCurrentDirectory()}/<MatreshkaSimulator or/and ImpulseSimulator>");

            var task = new Task(() =>
            {
                new MatreshkaSimulator().Start();
            });

            task.Start();

            return task;
        }
    }
}
