using DynamicData;
using DynamicData.Binding;
using IRAPROM.MyCore.Model;
using IRAPROM.MyCore.Model.WP;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Device.Matreshka
{
    public class Matreshka : DeviceMetalDetector, IMonopanel, INotifyPropertyChanged
    {
        public override string SeriesName => "Матрешка";
        public override string ModelName => Constants.GetModelName(Model);

        public Constants.Model Model => (Constants.Model)WorkParams.ModelId;
        public override List<short> AvailableZonesCount => WorkParams == null ? null : Constants.Models[ModelName].AvailableZonesCount;
        public override short PortTCP { get => _portTCP == 0 ? FamilyInfo.PortTCP : _portTCP; set {} }
        public override short PortUDP { get => _portUDP == 0 ? FamilyInfo.PortUDP : _portUDP; set {} }
        public override List<int> GridCellDefinitions => WorkParams == null ? null : Constants.Models[ModelName].GridCellDefinitions;
        public override int RealCoilsCount => WorkParams == null ? 0 : Constants.Models[ModelName].RealCoilsCount;

        public override MetalDetectorPassage LastPassage
        {
            get => _lastPassage;
            set
            {
                if (value?.MAC == null) return;
                
                ProcessAlarm(value);
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
                SceneMode = 1,
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
        public Matreshka(string ip, short port) : base(new WorkParamsProto(new NetworkProtoCommonDual(ip, Constants.PortTCPDefault), new DatagramProto(), Constants.GetCommands, Constants.SetCommands), new Constants())
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


        private void ProcessAlarm(MetalDetectorPassage value)
        {
            _lastPassage.SensorMode = WorkParams.ZonesSensorMode;
            _lastPassage.AlarmInf = value.AlarmInf;
            _lastPassage.LogId = value.LogId;
            _lastPassage.MAC = value.MAC;
            _lastPassage.LastPassageTime = value.LastPassageTime;
            _lastPassage.IsAlarm = value.IsAlarm;
            _lastPassage.Time = value.Time;

            _lastPassage.SensorsProcessed = value.SensorsProcessed;
            _lastPassage.Sensors = value.Sensors;
            _lastPassage.AlarmCells = value.AlarmCells;

            if (value.IsAlarm)
            {
                _lastPassage.LastAlarmTime = value.LastAlarmTime;

                UpdateAlarmCells(value.Sensors);

                if (value.EnterPassagesCount > 0) _lastPassage.EnterPassagesCount = value.EnterPassagesCount;
                if (value.EnterAlarmCount > 0) _lastPassage.EnterAlarmCount = value.EnterAlarmCount;
                if (value.ExitPassagesCount > 0) _lastPassage.ExitPassagesCount = value.ExitPassagesCount;
                if (value.ExitAlarmCount > 0) _lastPassage.ExitAlarmCount = value.ExitAlarmCount;
            }
            else
            {
                _lastPassage.EnterPassagesCount = value.EnterPassagesCount;
                _lastPassage.EnterAlarmCount = value.EnterAlarmCount;
                _lastPassage.ExitPassagesCount = value.ExitPassagesCount;
                _lastPassage.ExitAlarmCount = value.ExitAlarmCount;
            }
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

            ParseSensors(sensors, RowsCount, RealCoilsCount);
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
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
