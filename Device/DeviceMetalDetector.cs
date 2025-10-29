using System.Collections.Generic;
using Newtonsoft.Json;
using IRAPROM.MyCore.Model.WP;

namespace Device
{
    public abstract class DeviceMetalDetector
    {
        [JsonIgnore]
        public static List<FamilyInfo> FamilyInfoVariants = new List<FamilyInfo> { new Matreshka.Constants(), new Impulse.Constants() };

        public WorkParams WorkParams;
        public long UID;
        [JsonProperty]
        public readonly FamilyInfo FamilyInfo;

        public virtual string IP { get => _ip; set { _ip = value; if (WorkParams != null) WorkParams.IP = value; } }
        public virtual string Mask { get => _mask; set { _mask = value; if (WorkParams != null) WorkParams.Mask = value; } }
        public virtual string Gateway { get => _gateway; set { _gateway = value; if (WorkParams != null) WorkParams.Gateway = value; } }
        public virtual string MAC { get => _mac; set { _mac = value; if (WorkParams != null) WorkParams.MAC = value; } }

        public virtual short PortTCP { get => _portTCP == 0 ? FamilyInfo.PortTCP : _portTCP; set { _portTCP = value; if (WorkParams != null) WorkParams.PortTCP = value; } }
        public virtual short PortUDP { get => _portUDP == 0 ? FamilyInfo.PortUDP : _portUDP; set { _portUDP = value; if (WorkParams != null) WorkParams.PortUDP = value; } }

        [JsonIgnore]
        public abstract int ZonesCount { get; set; }
        [JsonIgnore]
        public abstract List<short> AvailableZonesCount { get; }
        public abstract string SeriesName { get; }
        public abstract string ModelName { get; }
        
        [JsonProperty]
        protected short _portUDP;
        [JsonProperty]
        protected short _portTCP;
        [JsonProperty]
        protected readonly IWorkParamsProto WorkParamsProto;

        private string _ip;
        private string _mac;        
        private string _mask;
        private string _gateway;

        protected DeviceMetalDetector() { }

        protected DeviceMetalDetector(IWorkParamsProto workParamsProto, FamilyInfo familyInfo)
        {
            WorkParamsProto = workParamsProto;
            FamilyInfo = familyInfo;
        }

        public virtual WorkParams GetWorkParams()
        {
            return WorkParams = WorkParamsProto.GetWorkParams();
        }

        public virtual bool SetWorkParams()
        {
            return WorkParamsProto.SetWorkParams(WorkParams);
        }
        
        public virtual bool SelfTest()
        {
            return WorkParamsProto.SelfTest(WorkParams);
        }

        public virtual DeviceMetalDetector Clone()
        {
            return (DeviceMetalDetector)MemberwiseClone();
        }
    }
}
