using IRAPROM.MyCore.Model;
using IRAPROM.MyCore.Model.WP;
using Newtonsoft.Json;
using PassAlarmSimulator.Device;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace IRAPROM.MyCore.Device
{
    public abstract class DeviceMetalDetector: CardItem, IEquatable<DeviceMetalDetector>, INotifyPropertyChanged
    {
        [JsonIgnore]
        public static List<FamilyInfo> FamilyInfoVariants = new List<FamilyInfo> { new Matreshka.Constants(), new Device.Impulse.Constants(), new IRAPROM.MyCore.Device.Matreshka.XGOST.Constants() };
        
        [JsonIgnore]
        public static DeviceMetalDetector DefaultDeviceMetalDetector = IRAPROM.MyCore.Device.Matreshka.Matreshka.DefaultMatreshka;
        
        [JsonIgnore]
        public static ObservableCollection<DeviceMetalDetector> DefaultDevicesMetalDetector = new ObservableCollection<DeviceMetalDetector>()
            {
                DefaultDeviceMetalDetector, DefaultDeviceMetalDetector, DefaultDeviceMetalDetector, DefaultDeviceMetalDetector, DefaultDeviceMetalDetector, DefaultDeviceMetalDetector, DefaultDeviceMetalDetector, DefaultDeviceMetalDetector,
            };

        public string Guid { get; set; } = System.Guid.NewGuid().ToString();
        public string Name { get => _name; set { _name = value; TitleName = value; OnPropertyChanged(); } }

        public WorkParams WorkParams { get; set; }

        [JsonIgnore]
        public readonly FamilyInfo FamilyInfo;

        public virtual string IP { get => _ip; set { _ip = value; if (WorkParams != null) WorkParams.IP = value; } }
        public virtual string Mask { get => _mask; set { _mask = value; if (WorkParams != null) WorkParams.Mask = value; } }
        public virtual string Gateway { get => _gateway; set { _gateway = value; if (WorkParams != null) WorkParams.Gateway = value; } }
        public virtual string MAC { get => _mac; set { _mac = value; if (WorkParams != null) WorkParams.MAC = value; } }
        public virtual ushort PortTCP { get => _portTCP == 0 ? FamilyInfo?.PortTCP ?? 0 : _portTCP; set { _portTCP = value; if (WorkParams != null) WorkParams.PortTCP = value; } }
        public virtual ushort PortUDP { get => _portUDP == 0 ? FamilyInfo?.PortTCP ?? 0 : _portUDP; set { _portUDP = value; if (WorkParams != null) WorkParams.PortUDP = value; } }
        [JsonIgnore]
        public virtual List<int> GridCellDefinitions { get; set; }
        [JsonIgnore]
        public virtual int RealCoilsCount { get; set; }

        [JsonIgnore]
        public abstract byte ZonesCount { get; set; }
        [JsonIgnore]
        public abstract List<short> AvailableZonesCount { get; }
        [JsonIgnore]
        public abstract Dictionary<int, string> InfraModesList { get; }
        public abstract string SeriesName { get; }
        public abstract ushort ModelId { set; get; }
        public abstract string ModelName { get; }
        public abstract string ProductModelName { set; get; }
        public abstract string InfraModeName { get; }

        [JsonProperty]
        public abstract MetalDetectorPassage LastPassage { get; set; }


        public abstract void CleanStatistics();

        [JsonProperty]
        protected ushort _portUDP;
        [JsonProperty]
        protected ushort _portTCP;
        [JsonProperty]
        protected readonly IWorkParamsProto WorkParamsProto;

        private string _ip;
        private string _mac;        
        private string _mask;
        private string _gateway;

        public string _name;

        protected DeviceMetalDetector()
        {
        }

        protected DeviceMetalDetector(IWorkParamsProto workParamsProto, FamilyInfo familyInfo)
        {
            WorkParamsProto = workParamsProto;
            FamilyInfo = familyInfo;
        }

        protected DeviceMetalDetector(string ip, ushort portTCP, IWorkParamsProto workParamsProto, FamilyInfo familyInfo)
        {
            WorkParamsProto = workParamsProto;
            FamilyInfo = familyInfo;
            _ip = ip;
            _portTCP = portTCP;
        }

        public virtual WorkParams GetWorkParams()
        {
            WorkParams = WorkParamsProto.GetWorkParams();

            if (WorkParams != null)
            {
                if (WorkParams.ModelId != 0)        // TODO проверить на импульсах
                {
                    ModelId = WorkParams.ModelId;
                }

                WorkParams.ModelId = (byte)ModelId;
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
            Console.WriteLine($"GetWorkParams: complete {_ip} : {MAC} : {ModelName} : {WorkParams?.FirmwareVersion}");
#endif
            return WorkParams;
        }

        public virtual bool SetWorkParams()
        {
            var res = WorkParamsProto.SetWorkParams(WorkParams);
#if DEBUG
            Console.WriteLine($@"SetWorkParams: {_ip}:{MAC}:{ModelName} {(res ? "success" : "FAIL!")}");
#endif
            return res;
        }

        public virtual bool SetWorkProgramScene()
        {
            var res = WorkParamsProto.SetWorkingMode(WorkParams);

#if DEBUG
            Console.WriteLine($@"SetWorkingMode: complete {_ip}:{MAC}:{ModelName} {(res ? "success" : "FAIL!")}");
#endif

            return res;
        }

        public virtual bool StaticTest()
        {
            Console.WriteLine($@"

____________Starting Static Tests ModelName={ModelName}:{ModelId:X}:{WorkParams?.ModelId:X} SerialNumber={WorkParams.SerialNumber} FirmwareVersion={WorkParams.FirmwareVersion}");

            var result = ((ITestsProto)WorkParamsProto).StaticTest(WorkParams);

#if DEBUG
            Console.WriteLine($"___StaticTest: {_ip}:{MAC} {(result ? " OK." : "FAIL!")} {ModelName} {WorkParams.SerialNumber}");
#endif

            return result;
        }

        public virtual bool BruteTest()
        {
            Console.WriteLine($"\n\n____________Starting Brute Tests ModelName={ModelName} SerialNumber={WorkParams.SerialNumber} FirmwareVersion={WorkParams.FirmwareVersion}\n");

            //var result = ((ITestsProto)WorkParamsProto).BruteTest(WorkParams);
            var result = ((ITestsProto)WorkParamsProto).BrutePortsTest(WorkParams);
#if DEBUG
            Console.WriteLine($"___BruteTest: {_ip}:{MAC} {(result ? " OK." : "FAIL!")} {ModelName} {WorkParams.SerialNumber}");
#endif

            return result;
        }

        public virtual async Task<bool> DynamicTest(int milliSecondsTimeout)
        {
#if DEBUG
            Console.WriteLine($"\n___DynamicTest: {_ip}:{MAC} ModelName={ModelName} SerialNumber={WorkParams.SerialNumber}");
#endif

            var enterPassagesCount = WorkParams?.ForwardPassageCount ?? 0;
            var enterAlarmCount = WorkParams?.ForwardAlarmsCount ?? 0;
            var exitPassagesCount = WorkParams?.BackwardPassageCount ?? 0;
            var exitAlarmCount = WorkParams?.BackwardAlarmsCount ?? 0;

#if DEBUG
            Console.WriteLine($"\n___DynamicTest: \n\t EnterPassagesCount {enterPassagesCount}\n\t EnterAlarmCount {enterAlarmCount}" +
                              $"\n\t ExitPassagesCount {exitPassagesCount}\n\t ExitAlarmCount {exitAlarmCount}");
#endif
            var success = ((ITestsProto)WorkParamsProto).DynamicTest(WorkParams, milliSecondsTimeout, true);
            
            Thread.Sleep(2000);


#if DEBUG
            Console.WriteLine($"\n___DynamicTest After: \n\t EnterPassagesCount {LastPassage.EnterPassagesCount}\n\t EnterAlarmCount {LastPassage.EnterAlarmCount}" +
                              $"\n\t ExitPassagesCount {LastPassage.ExitPassagesCount}\n\t ExitAlarmCount {LastPassage.ExitAlarmCount}");
#endif

            if (LastPassage == null || !LastPassage.IsAlarm || !success)
            {
#if DEBUG
                Console.WriteLine($"DynamicTest: Error: missing simulate alarm or it was not alarm!");
#endif
                return false;
            }

            if (enterPassagesCount == LastPassage.EnterPassagesCount && enterAlarmCount == LastPassage.EnterAlarmCount && exitPassagesCount == LastPassage.ExitPassagesCount &&
                exitAlarmCount == LastPassage.ExitAlarmCount)
            {
#if DEBUG
                Console.WriteLine($"DynamicTest: Error: passage count before and after simulate alarm is equal!");
#endif
                return false;
            }

            enterPassagesCount = LastPassage?.EnterPassagesCount ?? 0;
            enterAlarmCount = LastPassage?.EnterAlarmCount ?? 0;
            exitPassagesCount = LastPassage?.ExitPassagesCount ?? 0;
            exitAlarmCount = LastPassage?.ExitAlarmCount ?? 0;
            success = ((ITestsProto)WorkParamsProto).DynamicTest(WorkParams, milliSecondsTimeout, false);

            Thread.Sleep(2000);




            if (LastPassage == null || LastPassage.IsAlarm || !success)
            {
#if DEBUG
                Console.WriteLine($"DynamicTest: Error: missing simulate passage or it was alarm!");
#endif
                return false;
            }

            if (enterPassagesCount == LastPassage.EnterPassagesCount && enterAlarmCount == LastPassage.EnterAlarmCount && exitPassagesCount == LastPassage.ExitPassagesCount &&
                exitAlarmCount == LastPassage.ExitAlarmCount)
            {
#if DEBUG
                Console.WriteLine($"DynamicTest: Error: passage count before and after simulate passage is equal!");
#endif
                return false;
            }

#if DEBUG
            Console.WriteLine($"___DynamicTest: {_ip}:{MAC} OK {ModelName} {WorkParams.SerialNumber}");
#endif

            return true;
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

        public virtual void SetIp(string ip, string mask)
        {
            var tempWorkParams = new WorkParams
            {
                IP = IP = ip,
                PortTCP = PortTCP,
                PortUDP = PortUDP,
                Mask = mask,
                Gateway = Gateway,
                MAC = MAC
            };

            try
            {
                WorkParamsProto.SetNetworkParams(tempWorkParams);

                if (WorkParams != null)
                {
                    WorkParams.IP = ip;
                    WorkParams.Mask = mask;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public bool Equals(DeviceMetalDetector other)
        {
            return other != null && MAC == other.MAC && Guid == other.Guid && IP == other.IP && ZonesCount == other.ZonesCount && ModelName == other.ModelName && WorkParams.Equals(other.WorkParams);
        }

        public new event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
