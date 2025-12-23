using Google.Protobuf.Compiler;
using IRAPROM.MyCore.Model.WP;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI.Fody.Helpers;

namespace Device
{
    public abstract class DeviceMetalDetector: IEquatable<DeviceMetalDetector>
    {
        [JsonIgnore]
        public static List<FamilyInfo> FamilyInfoVariants = new List<FamilyInfo> { new Matreshka.Constants(), new Impulse.Constants() };
        
        [JsonIgnore]
        public static DeviceMetalDetector DefaultDeviceMetalDetector = Matreshka.Matreshka.DefaultMatreshka;
        
        [JsonIgnore]
        public static ObservableCollection<DeviceMetalDetector> DefaultDevicesMetalDetector = new ObservableCollection<DeviceMetalDetector>()
            {
                DefaultDeviceMetalDetector, DefaultDeviceMetalDetector, DefaultDeviceMetalDetector, DefaultDeviceMetalDetector, DefaultDeviceMetalDetector, DefaultDeviceMetalDetector, DefaultDeviceMetalDetector, DefaultDeviceMetalDetector,
            };

        public Guid UID { get; set; }
        public string Name { get; set; }
        public WorkParams WorkParams { get; set; }

        [JsonProperty]
        public readonly FamilyInfo FamilyInfo;

        public virtual string IP { get => _ip; set { _ip = value; if (WorkParams != null) WorkParams.IP = value; } }
        public virtual string Mask { get => _mask; set { _mask = value; if (WorkParams != null) WorkParams.Mask = value; } }
        public virtual string Gateway { get => _gateway; set { _gateway = value; if (WorkParams != null) WorkParams.Gateway = value; } }
        public virtual string MAC { get => _mac; set { _mac = value; if (WorkParams != null) WorkParams.MAC = value; } }
        public virtual short PortTCP { get => _portTCP == 0 ? FamilyInfo.PortTCP : _portTCP; set { _portTCP = value; if (WorkParams != null) WorkParams.PortTCP = value; } }
        public virtual short PortUDP { get => _portUDP == 0 ? FamilyInfo.PortUDP : _portUDP; set { _portUDP = value; if (WorkParams != null) WorkParams.PortUDP = value; } }
        [JsonIgnore]
        public virtual List<int> GridCellDefinitions { get; set; }
        [JsonIgnore]
        public virtual int RealCoilsCount { get; set; }

        [JsonIgnore]
        public abstract int ZonesCount { get; set; }
        [JsonIgnore]
        public abstract List<short> AvailableZonesCount { get; }
        public abstract string SeriesName { get; }
        public abstract string ModelName { get; }

        [JsonProperty]
        [Reactive]
        public abstract MetalDetectorPassage LastPassage { get; set; }

        public abstract void CleanStatistics();

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


        protected DeviceMetalDetector()
        {
            UID = Guid.NewGuid();
        }

        protected DeviceMetalDetector(IWorkParamsProto workParamsProto, FamilyInfo familyInfo)
        {
            WorkParamsProto = workParamsProto;
            FamilyInfo = familyInfo;
            UID = Guid.NewGuid();
        }

        public virtual WorkParams GetWorkParams()
        {
            var res = WorkParams = WorkParamsProto.GetWorkParams();

            if (WorkParams != null && WorkParams.ModelId != 0 && WorkParams.ModelId != 0xFF)
            {
                res.ModelId = WorkParams.ModelId;
            }

            WorkParams.IP ??= _ip;
            WorkParams.Mask ??= _mask;
            WorkParams.Gateway ??= _gateway;
            WorkParams.MAC ??= _mac;

#if DEBUG
            Console.WriteLine($"GetWorkParams: complete {_ip}:{MAC}:{ModelName}");
#endif
            return res;
        }

        public virtual bool SetWorkParams()
        {
            var res = WorkParamsProto.SetWorkParams(WorkParams);
#if DEBUG
            Console.WriteLine($"SetWorkParams: complete {_ip}:{MAC}:{ModelName}");
#endif
            return res;
        }

        public virtual void SetWorkProgramScene()
        {
            WorkParamsProto.SetWorkProgramScene(WorkParams);
#if DEBUG
            Console.WriteLine($"SetWorkProgramScene: complete {_ip}:{MAC}:{ModelName}");
#endif
        }

        public virtual bool SelfTest()
        {
            return WorkParamsProto.SelfTest(WorkParams);
        }

        public virtual void SimulatePassage()
        {
            WorkParamsProto.CallPassage();
        }

        public virtual void SimulateAlarm()
        {
            WorkParamsProto.CallAlarm();
        }

        public virtual DeviceMetalDetector Clone()
        {
            return (DeviceMetalDetector)MemberwiseClone();
        }

        public bool Equals(DeviceMetalDetector other)
        {
            return other != null && MAC == other.MAC && UID == other.UID && IP == other.IP && ZonesCount == other.ZonesCount && ModelName == other.ModelName && WorkParams == other.WorkParams;
        }
    }
}
