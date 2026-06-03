using System.Collections.Generic;
using System.Linq;
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
        public abstract Task Find(string ip, IUDPSend sender);
        public abstract DeviceMetalDetector ParseFindCommandResponse(byte[] bytes, out ushort commandCode);
        public abstract Dictionary<int, string> InfraModesList { get; }

        public abstract Dictionary<ushort, MetalDetectorAttrs> Models { get; }

        public List<string> GetAllModelNames()
        {
            return Models.Values.Select(i => i.ModelName).ToList();
        }

        public string GetModelName(ushort modelId)
        {
            return Models.FirstOrDefault(i => i.Key == modelId).Value.ModelName;
        }

        public ushort GetModelId(string modeName)
        {
            return Models.FirstOrDefault(i => i.Value.ModelName == modeName).Key;
        }
    }
}
