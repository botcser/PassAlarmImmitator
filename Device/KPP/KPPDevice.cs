using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IRAPROM.MyCore.Device.KPP
{
    public class KPPDevice : DeviceMetalDetector, INotifyPropertyChanged
    {
        public IReadOnlyList<DeviceMetalDetector> Devices => _devices;

        public override string SeriesName { get; }
        public override string ModelName { get; }
        public override MetalDetectorPassage LastPassage { get; set; }
        public override int ZonesCount { get; set; }
        public override List<short> AvailableZonesCount { get; }

        public SortedList<DateTime, MetalDetectorPassage> LastAlarmPassage =  new SortedList<DateTime, MetalDetectorPassage>();

        private object _lock = new object();
        private List<DeviceMetalDetector> _devices = new List<DeviceMetalDetector>();

        public KPPDevice() : base(new WorkParamsProto(), null) { }

        public bool TryAddDevice(DeviceMetalDetector device, DeviceMetalDetector oldDevice = null)
        {
            if (device == null)
            {
                return false;
            }

            if (oldDevice != null && oldDevice.MAC != device.MAC)
            {
                return false;
            }

            lock (_lock)
            {
                LeftFirstNumber += device.LastPassage.EnterPassagesCount - (oldDevice == null ? 0 : oldDevice.LastPassage.EnterPassagesCount);
                LeftSecondNumber += device.LastPassage.EnterAlarmCount - (oldDevice == null ? 0 : oldDevice.LastPassage.EnterAlarmCount); 
                RightFirstNumber += device.LastPassage.ExitPassagesCount - (oldDevice == null ? 0 : oldDevice.LastPassage.ExitPassagesCount); 
                RightSecondNumber += device.LastPassage.ExitAlarmCount - (oldDevice == null ? 0 : oldDevice.LastPassage.ExitAlarmCount); 

                UpdateAlarm(device);

                LastPassage = device.LastPassage;

                _devices.Remove(_devices.FirstOrDefault(i => i.MAC == device.MAC));
                _devices.Add(device);
            }

            return true;
        }

        private void UpdateAlarm(DeviceMetalDetector device)
        {
            var lastAlarm = LastAlarmPassage.Values.FirstOrDefault(i => i.MAC == device.MAC);

            if (device.LastPassage.IsAlarm)
            {
                if (lastAlarm == null)
                {
                    LastAlarmPassage.Add(device.LastPassage.Time, device.LastPassage);
                }
                else //если тревога уже была
                {
                    if (lastAlarm.Time > device.LastPassage.Time) return;

                    LastAlarmPassage.Remove(lastAlarm.Time);
                    LastAlarmPassage.Add(device.LastPassage.Time, device.LastPassage);
                }
            }
            else //если это не тревога
            {
                if (lastAlarm == null) return;

                if (lastAlarm.Time > device.LastPassage.Time) return;

                LastAlarmPassage.Remove(lastAlarm.Time);
            }

            UpdateAlarmText();
        }

        public void RemoveDevice(DeviceMetalDetector device)
        {
            lock (_lock)
            {
                LeftFirstNumber -= device.LastPassage.EnterPassagesCount;
                LeftSecondNumber -= device.LastPassage.EnterAlarmCount;
                RightFirstNumber -= device.LastPassage.ExitPassagesCount;
                RightSecondNumber -= device.LastPassage.ExitAlarmCount;

                LastAlarmPassage.Remove(device.LastPassage.Time);
                
                LastPassage = _devices.Select(i => i.LastPassage).Max();
                
                _devices.Remove(Devices.FirstOrDefault(i => i.MAC == device.MAC));
                _devices.Add(device);

                UpdateAlarmText();
            }
        }

        private void UpdateAlarmText()
        {
            CenterBottomText = LastAlarmPassage.Count > 0 ? LastAlarmPassage.LastOrDefault().Value.ToString() : "";
            Trigger = LastAlarmPassage.Count > 0;
        }

        public override void CleanStatistics()
        {
            throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
