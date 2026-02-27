using PassAlarmSimulator.Device.Simulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IRAPROM.MyCore.Device.Impulse;

namespace PassAlarmSimulator.Device.Impulse.ImpulseSimulator
{
    public class ImpulseSimulator : DeviceSimulator
    {
        public ImpulseSimulator() : base(Constants.PortUDPDefault, Constants.PortUDPListenDefault, Constants.PortTCPDefault, $"{Directory.GetCurrentDirectory()}/ImpulseSimulator", new DatagramProto())
        {

        }
    }
}
