using Device.Matreshka.Simulator;
using Extensions;

namespace PassAlarmSimulator
{
    public class PassAlarmSimulator : IStart
    {
        private MatreshkaSimulator _matreshkaSimulator;

        public Task Start()
        {
            Console.WriteLine($"PassAlarmSimulator: I assume that the response files are in the directory {Directory.GetCurrentDirectory()}/<MatreshkaSimulator or/and ImpulseSimulator>");

            var task = new Task(() =>
            {
                _matreshkaSimulator = new MatreshkaSimulator();
                _matreshkaSimulator.Start();
            });

            task.Start();

            return task;
        }

        public void Shutdown()
        {
            _matreshkaSimulator.Shutdown();
        }
    }
}
