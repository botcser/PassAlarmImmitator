using Extensions;
using IRAPROM.MyCore.Device.Matreshka.MatreshkaSimulator;

namespace PassAlarmSimulator
{
    public class PassAlarmSimulator : IStart
    {
        private MatreshkaSimulator _matreshkaSimulator;
        private readonly bool _oldPC;

        public PassAlarmSimulator(bool oldPC = false)
        {
            _oldPC = oldPC;
        }

        public Task Start()
        {
            Console.WriteLine($"PassAlarmSimulator: I assume that the response files are in the directory {Directory.GetCurrentDirectory()}/<MatreshkaSimulator or/and ImpulseSimulator>");

            var task = new Task(() =>
            {
                _matreshkaSimulator = new MatreshkaSimulator(_oldPC);
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
