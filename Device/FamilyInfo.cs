using System.Collections.Generic;
using System.Threading.Tasks;
using IRAPROM.MyCore.MyNetwork;

namespace IRAPROM.MyCore.Device
{
    public abstract class FamilyInfo
    {
        public abstract ushort PortTCP { get; }
        public abstract ushort PortUDP { get; }
        public abstract short PortUDPAdditional { get; set; }
        public abstract short PortUDPListen { get; }
        public abstract short PortUDPListenAdditional { get; set; }

        public abstract List<string> WorkPrograms { get; }

        public abstract List<string> GetAllModels();
        
        public abstract string GetModelName(int id);

        public abstract int GetModelId(string name);

        public abstract Task Find(string ip, IUDPSend sender);

        public abstract DeviceMetalDetector ParseFindCommandResponse(byte[] bytes, out ushort commandCode);
    }
}
