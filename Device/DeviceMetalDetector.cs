using System.Collections.ObjectModel;
using IRAPROM.MyCore.Device.Impulse;
using IRAPROM.MyCore.Model;
using IRAPROM.MyCore.Model.WP;
using Newtonsoft.Json;
using PassAlarmSimulator.Device;

namespace IRAPROM.MyCore.Device
{
    public abstract class DeviceMetalDetector: CardItem, IEquatable<DeviceMetalDetector>
    {
        [JsonIgnore]
        public static List<FamilyInfo> FamilyInfoVariants = new List<FamilyInfo> { new IRAPROM.MyCore.Device.Matreshka.Constants(), new Constants() };
        
        [JsonIgnore]
        public static DeviceMetalDetector DefaultDeviceMetalDetector = IRAPROM.MyCore.Device.Matreshka.Matreshka.DefaultMatreshka;
        
        [JsonIgnore]
        public static ObservableCollection<DeviceMetalDetector> DefaultDevicesMetalDetector = new ObservableCollection<DeviceMetalDetector>()
            {
                DefaultDeviceMetalDetector, DefaultDeviceMetalDetector, DefaultDeviceMetalDetector, DefaultDeviceMetalDetector, DefaultDeviceMetalDetector, DefaultDeviceMetalDetector, DefaultDeviceMetalDetector, DefaultDeviceMetalDetector,
            };

        public Guid UID { get; set; }
        public string Name { get => _name; set { _name = value; TitleName = value; } }

        public WorkParams WorkParams { get; set; }

        [JsonProperty]
        public readonly FamilyInfo FamilyInfo;

        public virtual string IP { get => _ip; set { _ip = value; if (WorkParams != null) WorkParams.IP = value; } }
        public virtual string Mask { get => _mask; set { _mask = value; if (WorkParams != null) WorkParams.Mask = value; } }
        public virtual string Gateway { get => _gateway; set { _gateway = value; if (WorkParams != null) WorkParams.Gateway = value; } }
        public virtual string MAC { get => _mac; set { _mac = value; if (WorkParams != null) WorkParams.MAC = value; } }
        public virtual short PortTCP { get => _portTCP == 0 ? FamilyInfo?.PortTCP ?? 0 : _portTCP; set { _portTCP = value; if (WorkParams != null) WorkParams.PortTCP = value; } }
        public virtual short PortUDP { get => _portUDP == 0 ? FamilyInfo?.PortTCP ?? 0 : _portUDP; set { _portUDP = value; if (WorkParams != null) WorkParams.PortUDP = value; } }
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

        public string _name;

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
            var bufModelId = WorkParams?.ModelId ?? (byte)0xfe;

            WorkParams = WorkParamsProto.GetWorkParams();

            if (WorkParams != null)
            {
                if (WorkParams.ModelId == 0 || WorkParams.ModelId == 0xFF || WorkParams.ModelId == 0xFE)
                {
                    WorkParams.ModelId = bufModelId;
                }

                WorkParams.IP ??= _ip;
                WorkParams.Mask ??= _mask;
                WorkParams.Gateway ??= _gateway;
                WorkParams.MAC ??= _mac;
                
                if (WorkParams.ForwardAlarmsCount > 0 || WorkParams.ForwardPassageCount > 0 || WorkParams.BackwardAlarmsCount > 0 || WorkParams.BackwardPassageCount > 0)
                {
                    var unknownTime = LastPassage?.Time ?? default;
                    var unknownSensorMode = unknownTime == default ? WorkParams.InfraredPassCounterMode : LastPassage!.SensorMode;
                    
                    LastPassage = new MetalDetectorPassage(MAC, unknownTime, unknownSensorMode, WorkParams.ForwardPassageCount, WorkParams.ForwardAlarmsCount, WorkParams.BackwardPassageCount, WorkParams.BackwardAlarmsCount);
                }
            }

#if DEBUG
            Console.WriteLine($"GetWorkParams: complete {_ip}:{MAC}:{ModelName}");
#endif
            return WorkParams;
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

        public virtual bool StaticTest()
        {
#if DEBUG
            Console.Write($"StaticTest: GetWorkParams {_ip}:{MAC}... ");
#endif

            WorkParams = WorkParamsProto.GetWorkParams();
            WorkParams.MAC = MAC;
            WorkParams.IP = IP;

            var result = ((ITestsProto)WorkParamsProto).StaticTest(WorkParams);
#if DEBUG
            Console.WriteLine($"{(result ? " OK." : "FAIL!")}");
#endif

            return result;
        }

        public virtual void HandTest()
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> DynamicTest(int milliSecondsTimeout)
        {
            return ((ITestsProto)WorkParamsProto).DynamicTest(WorkParams, milliSecondsTimeout);
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
            return other != null && MAC == other.MAC && UID == other.UID && IP == other.IP && ZonesCount == other.ZonesCount && ModelName == other.ModelName && WorkParams.Equals(other.WorkParams);
        }
    }
}
