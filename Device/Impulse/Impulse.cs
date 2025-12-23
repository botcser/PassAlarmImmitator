using IRAPROM.MyCore.Model.MD;
using IRAPROM.MyCore.Model.WP;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Device.Impulse
{
    public class Impulse : DeviceMetalDetector, INotifyPropertyChanged
    {
        public override string SeriesName => "Импульс";
        public override string ModelName => Constants.GetModelName(Model);
        public Constants.Model Model => (Constants.Model)WorkParams.ModelId; 
        public override List<short> AvailableZonesCount => WorkParams == null ? null : Constants.Models[ModelName].AvailableZonesCount;
        public override List<int> GridCellDefinitions => WorkParams == null ? null : Constants.Models[ModelName].GridCellDefinitions;
        public override int RealCoilsCount => WorkParams == null ? 0 : Constants.Models[ModelName].RealCoilsCount;

        [JsonIgnore]
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
        
        public override MetalDetectorPassage LastPassage
        {
            get => _lastPassage;
            set
            {
                if (value == null) return;
                
                ProcessAlarm(value);
                OnPropertyChanged();
            }
        }

        public override void CleanStatistics()
        {
            WorkParamsProto.ClearPassageCount();
            LastPassage.Clean();
        }

        private MetalDetectorPassage _lastPassage = new MetalDetectorPassage();

        private int RowsCount => GridCellDefinitions != null ? GridCellDefinitions[0] : 0;
        private int ColumnsCount => GridCellDefinitions != null ? GridCellDefinitions[1] : 0;

        public Impulse() : base(new WorkParamsProto(new DatagramProto(), Constants.GetCommands, Constants.SetCommands), new Constants())
        {

        }
#if USE_COMMAND_CENTER            
        public Impulse(string ip, short portTCP) : base(new WorkParamsProto(new NetworkProtoHttp(ip, Constants.PortTCPDefault), new DatagramProto(), Constants.GetCommands, Constants.SetCommands), new Constants())
        {
            IP = ip;
            PortTCP = portTCP;
        }
#else
        public Impulse(string ip, short portTCP) : base(new WorkParamsProto(new NetworkProtoImpulse(ip, Constants.PortTCPDefault), new DatagramProto(), Constants.GetCommands, Constants.SetCommands), new Constants())
        {
            IP = ip;
            PortTCP = portTCP;
        }
#endif


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

        private void UpdateAlarmCells(byte[] sensors)
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

            using (var memoryStream = new MemoryStream(sensors))
            {
                using (var binaryReader = new BinaryReader(memoryStream))
                {
                    var leftPanelSensors = binaryReader.ReadBytes(RowsCount).Reverse().Select(i => i == 1);

                    IEnumerable<bool> rightPanelSensors = null;
                    IEnumerable<bool> centerSensors = null;

                    if (binaryReader.BaseStream.CanRead)
                    {
                        centerSensors = binaryReader.ReadBytes(RowsCount).Reverse().Select(i => i == 1);            // TODO разница с матрехой

                        if (binaryReader.BaseStream.CanRead)
                        {
                            rightPanelSensors = binaryReader.ReadBytes(RowsCount).Reverse().Select(i => i == 1);
                        }
                    }

                    _lastPassage.AlarmCells.Add(leftPanelSensors.ToList());
                    if (centerSensors != null) _lastPassage.AlarmCells.Add(centerSensors.ToList());
                    if (rightPanelSensors != null) _lastPassage.AlarmCells.Add(rightPanelSensors.ToList());
                }
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

                        for (var i = 0; i < realCoilsCount; i += halve)
                        {
                            rightAlarmCells.Add(rightPanelSensors[i] == 1);
                        }

                        for (var i = 0; i < leftPanelSensors.Length; i++)
                        {
                            centerAlarmCells.Add(leftPanelSensors[i] == 1 && rightPanelSensors[i] == 1);
                        }
                    }

                    _lastPassage.AlarmCells.Clear();
                    _lastPassage.AlarmCells.Add(leftAlarmCells);
                    _lastPassage.AlarmCells.Add(centerAlarmCells);
                    _lastPassage.AlarmCells.Add(rightAlarmCells);
                }
            }
        }

        private void ParseSensorsOld(byte[] sensors, int rowsCount, int realCoilsCount)
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
                        var centerSensors = binaryReader.ReadBytes(realCoilsCount).Reverse().ToArray();

                        for (var i = 0; i < realCoilsCount; i += halve)
                        {
                            centerAlarmCells.Add(centerSensors[i] == 1);
                        }

                        if (binaryReader.BaseStream.CanRead)
                        {
                            var rightPanelSensors = binaryReader.ReadBytes(RowsCount).Reverse().ToArray();

                            for (var i = 0; i < realCoilsCount; i += halve)
                            {
                                rightAlarmCells.Add(rightPanelSensors[i] == 1);
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
