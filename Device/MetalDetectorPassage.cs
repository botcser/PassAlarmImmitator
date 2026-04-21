using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IRAPROM.MyCore.Device
{
    [Serializable]
    public class MetalDetectorPassage : INotifyPropertyChanged, IComparable
    {            
        public byte[] Sensors;
        public byte[] SensorsProcessed;


        public List<List<bool>> AlarmCells { get => _alarmCells; set { _alarmCells = value; OnPropertyChanged();} }
        public short SensorMode { get; set; }
        public string AlarmInf { get; set; }
        public string LogId { get; set; }
        public string MAC { get; set; }
        public string LastAlarmTime { get; set; }

        public string LastPassageTime
        {
            get => Time.ToString("dd.MM.yy HH:mm:ss");
            //set { _lastPassageTime = value; OnPropertyChanged(); }
            set { OnPropertyChanged(); }
        }
        public bool IsAlarm { get => _isAlarm; set { _isAlarm = value; OnPropertyChanged(); } }
        public DateTime Time { get; set; }

        public uint EnterPassagesCount { get => _enterPassagesCount; set { _enterPassagesCount = value; OnPropertyChanged(); } }
        public uint EnterAlarmCount { get => _enterAlarmCount; set { _enterAlarmCount = value; OnPropertyChanged(); } }
        public uint ExitPassagesCount { get => _exitPassagesCount; set { _exitPassagesCount = value; OnPropertyChanged(); } }
        public uint ExitAlarmCount { get => _exitAlarmCount; set { _exitAlarmCount = value; OnPropertyChanged(); } }


        private uint _enterPassagesCount;
        private uint _enterAlarmCount;
        private uint _exitPassagesCount;
        private uint _exitAlarmCount;
        private bool _isAlarm;
        private List<List<bool>> _alarmCells;

        public MetalDetectorPassage() { }

        public MetalDetectorPassage(string mac, byte[] sensors, DateTime time, short sensorMode)
        {
            MAC = mac;
            SensorMode = sensorMode;
            Sensors = sensors;
            Time = time;
            LastPassageTime = LastAlarmTime = time.ToString("dd.MM.yy HH:mm:ss");
            IsAlarm = true;
        }

        public MetalDetectorPassage(string mac, DateTime time, short sensorMode, uint enterPassagesCount, uint enterAlarmCount, uint exitPassagesCount, uint exitAlarmCount)
        {
            MAC = mac;
            SensorMode = sensorMode;
            Sensors = new byte[18];
            Time = time;
            LastPassageTime = time.ToString("dd.MM.yy HH:mm:ss");
            IsAlarm = false;
            EnterPassagesCount = enterPassagesCount;
            EnterAlarmCount = enterAlarmCount;
            ExitPassagesCount = exitPassagesCount;
            ExitAlarmCount = exitAlarmCount;
        }

        public MetalDetectorPassage(string mac, byte[] sensors, DateTime time, short sensorMode, uint enterPassagesCount, uint enterAlarmCount, uint exitPassagesCount, uint exitAlarmCount)
        {
            MAC = mac;
            SensorMode = sensorMode;
            Sensors = sensors;
            Time = time;
            LastPassageTime = LastAlarmTime = time.ToString("dd.MM.yy HH:mm:ss");
            IsAlarm = true;
            EnterPassagesCount = enterPassagesCount;
            EnterAlarmCount = enterAlarmCount;
            ExitPassagesCount = exitPassagesCount;
            ExitAlarmCount = exitAlarmCount;
        }

        public void Clean()
        {
            EnterPassagesCount = EnterAlarmCount = ExitPassagesCount = ExitAlarmCount = 0;

            if (Sensors != null)
            {
                for (var i = 0; i < Sensors.Length; i++)
                {
                    Sensors[i] = 0;
                }
            }

            AlarmCells?.ForEach(list =>
            {
                if (list == null) return;

                for (var i = 0; i < list.Count; i++)
                {
                    list[i] = false;
                }
            });

            Time = default;
            LastPassageTime = LastAlarmTime = Time.ToString("dd.MM.yy HH:mm:ss");
            IsAlarm = false;
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int CompareTo(MetalDetectorPassage other)
        {
            return (int)(Time - other.Time).TotalSeconds;
        }

        public int CompareTo(object other)
        {
            return (int)(Time - ((MetalDetectorPassage)other).Time).TotalSeconds;
        }

        public virtual MetalDetectorPassage Clone()
        {
            return (MetalDetectorPassage)MemberwiseClone();
        }

    }
}
