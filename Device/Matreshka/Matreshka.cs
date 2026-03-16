using IRAPROM.MyCore.Model.WP;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace IRAPROM.MyCore.Device.Matreshka
{
    public class Matreshka : DeviceMetalDetector, IMonopanel, INotifyPropertyChanged
    {
#if OLDPCV
        public bool PrevPackageIsAlarm;
#endif

        public override string SeriesName => "Матрешка";
        public override ushort ModelId { get; set; }
        public override string ModelName => WorkParams == null ? "Unknown Matreshka" : Constants.GetModelName(Model);
        public override string ProductModelName { get; set; }

        public Constants.Model Model => WorkParams == null ? Constants.Model.UnknownMatreshka : (Constants.Model)WorkParams.ModelId;
        public override List<short> AvailableZonesCount => WorkParams == null ? null : Constants.Models[ModelName].AvailableZonesCount;
        public override ushort PortTCP { get => _portTCP == 0 ? FamilyInfo.PortTCP : _portTCP; set {} }
        public override ushort PortUDP { get => _portUDP == 0 ? FamilyInfo.PortUDP : _portUDP; set {} }
        public override List<int> GridCellDefinitions => WorkParams == null ? null : Constants.Models[ModelName].GridCellDefinitions;
        public override int RealCoilsCount => WorkParams == null ? 0 : Constants.Models[ModelName].RealCoilsCount;

        public override MetalDetectorPassage LastPassage
        {
            get => _lastPassage;
            set
            {
                if (value?.MAC == null) return;

#if OLDPCV
                RegisterPassage(value);
#else
                ProcessAlarm(value);
#endif

                OnPropertyChanged();
            }
        }

        public override void CleanStatistics()
        {
            WorkParamsProto.ClearPassageCount();
            LastPassage.Clean();
        }

        public override int ZonesCount
        {
            get => WorkParams == null ? 0 : Constants.Models[ModelName].AvailableZonesCount[WorkParams.ZonesSensorMode];
            set
            { 
                if (WorkParams == null || value >= Constants.Models[ModelName].AvailableZonesCount.Count) return;      
                
                WorkParams.ZonesSensorMode = (byte)value;
                WorkParams.ZoneMode = Constants.Models[ModelName].AvailableZonesCount[WorkParams.ZonesSensorMode].ToString();
            }
        }

        [JsonIgnore]
        public static Matreshka DefaultMatreshka = new Matreshka("127.0.0.1", 9998)
        {
            WorkParams = new WorkParams()
            {
                AlarmDuration = 1,
                AlarmInfraMode = 0,
                AlarmLampSwapMode = 0,
                AlarmMode = 1,
                AlarmTone = 1,
                AlarmVolume = 1,
                BackwardAlarmsCount = 11111,
                BackwardPassageCount = 22222,
                ForwardAlarmsCount = 33333,
                ForwardPassageCount = 44444,
                BaseSensitivity = 1,
                ExchangeFrontBack = false,
                InfraredPassCounterMode = 0,
                Gateway = "192.168.0.1",
                ZoneMode = "33",
                ModelId = 42,
                MAC = "123456789abcdef",
                Mask = "255.255.255.0",
                MaxZoneMode = 33,
                WorkProgram = 1,
                SensorsSensitivity = new short[]
                {
                    1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                },
                ZonesSensorMode = 2,
            },
            GridCellDefinitions = new List<int>() {6,3},
            LastPassage = new MetalDetectorPassage()
            {
                AlarmCells = new List<List<bool>>()  { 
                    new List<bool>() {false, false, false, true, false, false,},
                    new List<bool>() {false, false, false, true, false, false,},
                    new List<bool>() {false, false, false, true, false, false,}

                },
                MAC = "0004a30009a3",
                LastAlarmTime = "01.12.25 17:46:23",
                LastPassageTime = "01.12.25 17:46:23",
                IsAlarm = true,
                Time = DateTime.Parse("2025-12-01T17:46:23.5633115+03:00"),
                Sensors = new byte[]{
                    0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0,  },
                EnterAlarmCount = 22222,
                EnterPassagesCount = 11111,
                ExitPassagesCount = 33333,
                ExitAlarmCount = 44444
            },
            Name = "PC V 3300 Name"
        };

        private MetalDetectorPassage _lastPassage = new MetalDetectorPassage();
        private int RowsCount => GridCellDefinitions != null ? GridCellDefinitions[0] : 0;
        private int ColumnsCount => GridCellDefinitions != null ? GridCellDefinitions[1] : 0;


        public Matreshka() : base(new WorkParamsProto(new DatagramProto(), Constants.GetCommands, Constants.SetCommands), new Constants()) { }

#if USE_COMMAND_CENTER
        public Matreshka(string ip, short port) : base(new WorkParamsProto(new NetworkProtoHttp(ip, Constants.PortTCPDefault), new DatagramProto(), Constants.GetCommands, Constants.SetCommands), new Constants())
        {
            IP = ip;
            PortTCP = port;
        }
#else
        public Matreshka(string ip, ushort port) : base(new WorkParamsProto(new NetworkProtoMatreshka(ip, Constants.PortTCPDefault), new DatagramProto(), Constants.GetCommands, Constants.SetCommands), new Constants())
        {
            IP = ip;
            PortTCP = port;
        }
#endif

        public void ScanCommands()
        {
            WorkParamsProto.ScanCommands(0x28, 0x28);
            WorkParamsProto.ScanCommands(0x2C, 0xFF);
        }

        public bool IsMonopanel()
        {
            return (int)Model >= 100;
        }

#if OLDPCV
        public void RegisterPassage(MetalDetectorPassage newPassage)
        {
            if (newPassage.IsAlarm)
            {
                ProcessAlarm(newPassage);
            }
            else
            {
                _lastPassage.AlarmInf = newPassage.AlarmInf;
                _lastPassage.LogId = newPassage.LogId;
                _lastPassage.MAC = newPassage.MAC;

                if (LastPassage.IsAlarm && !PrevPackageIsAlarm)
                {
                    CleanAlarm();
                }
                
                UpdatePassageCounters(newPassage);
            }

            PrevPackageIsAlarm = newPassage.IsAlarm;
        }

        private void CleanAlarm()
        {
            LastPassage.IsAlarm = false;
            LastPassage.SensorsProcessed = _lastPassage.Sensors = Array.Empty<byte>();
            LastPassage.AlarmCells.Clear();
            UpdateAlarmCells(LastPassage.Sensors);
        }
#endif

        private void ProcessAlarm(MetalDetectorPassage newPassage)
        {
            _lastPassage.AlarmInf = newPassage.AlarmInf;
            _lastPassage.LogId = newPassage.LogId;
            _lastPassage.MAC = newPassage.MAC;

            _lastPassage.IsAlarm = newPassage.IsAlarm;
            _lastPassage.SensorsProcessed = newPassage.SensorsProcessed;
            _lastPassage.Sensors = newPassage.Sensors;
            _lastPassage.AlarmCells = newPassage.AlarmCells;

            UpdatePassageCounters(newPassage);

            if (newPassage.IsAlarm)
            {
                _lastPassage.LastAlarmTime = newPassage.LastAlarmTime;

                UpdateAlarmCells(newPassage.Sensors);
            }
        }

        private void UpdatePassageCounters(MetalDetectorPassage newPassage)
        {
            if (!newPassage.IsAlarm)
            {
                _lastPassage.EnterPassagesCount = newPassage.EnterPassagesCount;
                _lastPassage.EnterAlarmCount = newPassage.EnterAlarmCount;
                _lastPassage.ExitPassagesCount = newPassage.ExitPassagesCount;
                _lastPassage.ExitAlarmCount = newPassage.ExitAlarmCount;
            }

            _lastPassage.Time = newPassage.Time;
            _lastPassage.LastPassageTime = newPassage.LastPassageTime;
            _lastPassage.SensorMode = WorkParams.ZonesSensorMode;
        }

        private void UpdateAlarmCells(byte[] sensors)                   // TODO
        {
            if (_lastPassage.Sensors == sensors && _lastPassage.AlarmCells != null) return;

            if (_lastPassage.AlarmCells == null)
            {
                _lastPassage.AlarmCells = new List<List<bool>>();
            }
            else
            {
                _lastPassage.AlarmCells.Clear();
            }

            switch (Model)
            {
                case Constants.Model.PCZ3300MK:
                case Constants.Model.PCV3300:
                    ParseSensors33(sensors, RowsCount, RealCoilsCount);
                    break;
                default:
                    ParseSensors(sensors, RowsCount, RealCoilsCount);
                    break;
            }
        }

        private void ParseSensors(byte[] sensors, int rowsCount, int realCoilsCount)
        {
            using (var memoryStream = new MemoryStream(sensors))
            {
                using (var binaryReader = new BinaryReader(memoryStream))
                {
                    var leftPanelSensors = binaryReader.ReadBytes(realCoilsCount).Reverse().ToArray();
                    var leftAlarmCells = new List<bool>();
                    var halve = realCoilsCount / rowsCount;

                    for (var i = 0; i < realCoilsCount; i += halve)
                    {
                        leftAlarmCells.Add(leftPanelSensors[i] == 1);
                    }

                    var rightAlarmCells = new List<bool>();
                    var centerAlarmCells = new List<bool>();

                    if (binaryReader.BaseStream.CanRead)
                    {
                        var rightPanelSensors = binaryReader.ReadBytes(realCoilsCount).Reverse().ToArray();
                        
                        for (var i = 0; i < rightPanelSensors.Length; i += halve)
                        {
                            rightAlarmCells.Add(rightPanelSensors[i] == 1);
                        }

                        if (binaryReader.BaseStream.CanRead)
                        {
                            var centerSensors = binaryReader.ReadBytes(realCoilsCount).Reverse().ToArray();

                            for (var i = 0; i < centerSensors.Length; i += halve)
                            {
                                centerAlarmCells.Add(centerSensors[i] == 1);
                            }
                        }
                    }

                    _lastPassage.AlarmCells.Clear();
                    _lastPassage.AlarmCells.Add(leftAlarmCells);
                    _lastPassage.AlarmCells.Add(centerAlarmCells);
                    _lastPassage.AlarmCells.Add(rightAlarmCells);
                }
            }
        }

        private void ParseSensors33(byte[] sensors, int rowsCount, int realCoilsCount)
        {
            using (var memoryStream = new MemoryStream(sensors))
            {
                using (var binaryReader = new BinaryReader(memoryStream))
                {
                    var rightAlarmCells = new List<bool>();
                    var centerAlarmCells = new List<bool>();
                    var leftAlarmCells = new List<bool>();
                    var metalContent = binaryReader.ReadBytes(2).ToArray();
                    var leftPanelSensors = binaryReader.ReadBytes(2).ToArray();
                    var bitsArray = new BitArray(leftPanelSensors);

                    for (var i = 0; i < 11; i ++)
                    {
                        leftAlarmCells.Add(bitsArray[i]);
                    }

                    leftAlarmCells.Reverse();

                    if (binaryReader.BaseStream.CanRead)
                    {
                        var rightPanelSensors = binaryReader.ReadBytes(2).ToArray();
                        
                        bitsArray = new BitArray(rightPanelSensors);

                        for (var i = 0; i < 11; i ++)
                        {
                            rightAlarmCells.Add(bitsArray[i]);
                        }

                        rightAlarmCells.Reverse();

                        for (var i = 0; i < 11; i++)
                        {
                            centerAlarmCells.Add(rightAlarmCells[i] && rightAlarmCells[i] == leftAlarmCells[i]);
                        }
                    }

                    _lastPassage.AlarmCells.Clear();
                    _lastPassage.AlarmCells.Add(leftAlarmCells);
                    _lastPassage.AlarmCells.Add(centerAlarmCells);
                    _lastPassage.AlarmCells.Add(rightAlarmCells);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
