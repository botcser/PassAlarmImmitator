using PassAlarmSimulator.Device.Simulator;

namespace IRAPROM.MyCore.Device.Matreshka.MatreshkaSimulator
{
    public class MatreshkaSimulator : DeviceSimulator
    {
        public MatreshkaSimulator(bool oldPC = false) : base(Constants.PortUDPDefault, Constants.PortUDPListenDefault, Constants.PortTCPDefault, $"{Directory.GetCurrentDirectory()}\\MatreshkaSimulator", new DatagramProto(), oldPC)
        {

        }
    }
}
