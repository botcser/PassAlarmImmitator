using System.Collections.Generic;
using System.Threading.Tasks;
using IRAPROM.MyCore.MyNetwork;

namespace Device
{
    public abstract class FamilyInfo
    {
        public abstract short PortTCP { get; }
        public abstract short PortUDP { get; }
        public abstract short PortUDPListen { get; }

        public abstract List<string> WorkPrograms { get; }

        public abstract List<string> GetAllModels();
        
        public abstract string GetModelName(int id);

        public abstract int GetModelId(string name);

        public abstract Task Find(string ip, IUDPSend sender);
    }
}
