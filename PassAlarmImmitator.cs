using Device.Matreshka.Simulator;

namespace PassAlarmSimulator
{
    public class PassAlarmSimulator
    {
        public static void Run()
        {
            var matreshkaSimulator = new MatreshkaSimulator();

            var task = matreshkaSimulator.Start();
            
            task.Wait();

            return;
        }
    }
}
