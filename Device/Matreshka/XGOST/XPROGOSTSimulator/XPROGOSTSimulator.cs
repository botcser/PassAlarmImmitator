using IRAPROM.MyCore.Device.Matreshka.XGOST;
using PassAlarmSimulator.Device.Simulator;

namespace PassAlarmSimulator.Device.Matreshka.XGOST.XPROGOSTSimulator
{
    public class XPROGOSTSimulator : DeviceSimulator
    {
        public XPROGOSTSimulator() : base(Constants.PortUDPDefault + 1, Constants.PortUDPListenDefault, Constants.PortTCPDefault, $"{Directory.GetCurrentDirectory()}\\XPROGOSTSimulator", new DatagramProto())
        {

        }
    }
}
