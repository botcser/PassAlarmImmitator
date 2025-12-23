using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Device
{
    [Serializable]
    public class MetalDetectorPassage : INotifyPropertyChanged, INotifyCollectionChanged
    {            
        public byte[] Sensors;
        public byte[] SensorsProcessed;


        public List<List<bool>> AlarmCells { get => _alarmCells; set { _alarmCells = value; OnPropertyChanged();} }
        public short SensorMode { get; set; }
        public string AlarmInf { get; set; }
        public string LogId { get; set; }
        public string MAC { get; set; }
        public string LastAlarmTime { get; set; }
        public string LastPassageTime { get => _lastPassageTime; set { _lastPassageTime = value; OnPropertyChanged(); } }
        public bool IsAlarm { get => _isAlarm; set { _isAlarm = value; OnPropertyChanged(); } }
        public DateTime Time { get; set; }

        public int EnterPassagesCount { get => _enterPassagesCount; set { _enterPassagesCount = value; OnPropertyChanged(); } }
        public int EnterAlarmCount { get => _enterAlarmCount; set { _enterAlarmCount = value; OnPropertyChanged(); } }
        public int ExitPassagesCount { get => _exitPassagesCount; set { _exitPassagesCount = value; OnPropertyChanged(); } }
        public int ExitAlarmCount { get => _exitAlarmCount; set { _exitAlarmCount = value; OnPropertyChanged(); } }


        private int _enterPassagesCount;
        private int _enterAlarmCount;
        private int _exitPassagesCount;
        private int _exitAlarmCount;
        private bool _isAlarm;
        private string _lastPassageTime;
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

        public MetalDetectorPassage(string mac, DateTime time, short sensorMode, int enterPassagesCount, int enterAlarmCount, int exitPassagesCount, int exitAlarmCount)
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
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        public void Clean()
        {
            EnterPassagesCount = EnterAlarmCount = ExitPassagesCount = ExitAlarmCount = 0;
        }
    }
}
