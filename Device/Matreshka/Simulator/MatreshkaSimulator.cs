using PassAlarmSimulator.Device.Simulator;

namespace Device.Matreshka.Simulator
{
    public class MatreshkaSimulator : DeviceSimulator
    {
        public MatreshkaSimulator() : base(Constants.PortUDPDefault, Constants.PortUDPListenDefault, Constants.PortTCPDefault, $"{Directory.GetCurrentDirectory()}/MatreshkaSimulator", new DatagramProto())
        {

        }
    }
}
