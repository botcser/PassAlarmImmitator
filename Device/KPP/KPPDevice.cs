using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using IRAPROM.MyCore.Model;

namespace IRAPROM.MyCore.Device.KPP
{
    public class KPPDevice : DeviceMetalDetector, INotifyPropertyChanged
    {
        public IReadOnlyList<DeviceMetalDetector> Devices => _devices;

        public bool HaveDevices
        {
            get { return Devices.Count > 0; }
        }

        public override string SeriesName { get; }
        public override ushort ModelId { get; set; }
        public override string ModelName { get; }
        public override string ProductModelName { get; set; }
        public override MetalDetectorPassage LastPassage { get; set; }
        public override byte ZonesCount { get; set; }
        public override List<short> AvailableZonesCount { get; }

        public SortedList<DateTime, MetalDetectorPassage> ActiveAlarms =  new SortedList<DateTime, MetalDetectorPassage>();

        private object _lock = new object();
        private readonly List<DeviceMetalDetector> _devices = new List<DeviceMetalDetector>();
        private bool _prevOnlineStatus;
        
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
                LeftFirstNumber += device.LastPassage.EnterPassagesCount;// - (oldDevice == null ? 0 : oldDevice.LastPassage.EnterPassagesCount);
                LeftSecondNumber += device.LastPassage.EnterAlarmCount;// - (oldDevice == null ? 0 : oldDevice.LastPassage.EnterAlarmCount); 
                RightFirstNumber += device.LastPassage.ExitPassagesCount;// - (oldDevice == null ? 0 : oldDevice.LastPassage.ExitPassagesCount); 
                RightSecondNumber += device.LastPassage.ExitAlarmCount;// - (oldDevice == null ? 0 : oldDevice.LastPassage.ExitAlarmCount);

                LastPassage = device.LastPassage;

                UpdateAlarm(device);
                UpdateCenterBottomText();

                _devices.Remove(_devices.FirstOrDefault(i => i.MAC == device.MAC));
                _devices.Add(device);

                UpdateOnline();
            }

            return true;
        }

        private void UpdateAlarm(DeviceMetalDetector device)
        {
            var lastDeviceAlarm = ActiveAlarms.Values.FirstOrDefault(i => i.MAC == device.MAC);

            if (device.LastPassage.IsAlarm)
            {
                if (lastDeviceAlarm != null)
                {
                    if (lastDeviceAlarm.Time > device.LastPassage.Time) return;

                    ActiveAlarms.Remove(lastDeviceAlarm.Time);
                }

                ActiveAlarms.Add(device.LastPassage.Time, device.LastPassage);
            }
            else
            {
                if (lastDeviceAlarm != null)
                {
                    if (lastDeviceAlarm.Time > device.LastPassage.Time) return;

                    ActiveAlarms.Remove(lastDeviceAlarm.Time);
                }
            }
        }

        public void RemoveDevice(DeviceMetalDetector device)
        {
            lock (_lock)
            {
                LeftFirstNumber -= device.LastPassage.EnterPassagesCount;
                LeftSecondNumber -= device.LastPassage.EnterAlarmCount;
                RightFirstNumber -= device.LastPassage.ExitPassagesCount;
                RightSecondNumber -= device.LastPassage.ExitAlarmCount;

                ActiveAlarms.Remove(device.LastPassage.Time);
                
                _devices.Remove(_devices.FirstOrDefault(i => i.MAC == device.MAC));

                LastPassage = _devices.Select(i => i.LastPassage).Max();

                UpdateCenterBottomText();
                UpdateOnline();
            }
        }

        private void UpdateCenterBottomText()
        {
            CenterBottomText = ActiveAlarms.Count > 0 ? ActiveAlarms.LastOrDefault().Key.ToString(CultureInfo.CurrentCulture) : LastPassage?.Time.ToString(CultureInfo.CurrentCulture);
            Trigger = ActiveAlarms.Count > 0;
        }

        public override void CleanStatistics()
        {
            throw new NotImplementedException();
        }

        public void CleanStatistics(DeviceMetalDetector device)
        {
            lock (_lock)
            {
                LeftFirstNumber -= device.LastPassage.EnterPassagesCount;// - (oldDevice == null ? 0 : oldDevice.LastPassage.EnterPassagesCount);
                LeftSecondNumber -= device.LastPassage.EnterAlarmCount;// - (oldDevice == null ? 0 : oldDevice.LastPassage.EnterAlarmCount); 
                RightFirstNumber -= device.LastPassage.ExitPassagesCount;// - (oldDevice == null ? 0 : oldDevice.LastPassage.ExitPassagesCount); 
                RightSecondNumber -= device.LastPassage.ExitAlarmCount;// - (oldDevice == null ? 0 : oldDevice.LastPassage.ExitAlarmCount);

                if (LastPassage.Time == device.LastPassage.Time)
                {
                    LastPassage.Time = default;
                    LastPassage.LastPassageTime = "";
                }

                ActiveAlarms.Remove(device.LastPassage.Time);
                UpdateCenterBottomText();
                UpdateOnline();
            }
        }

        public void UpdateOnline()
        {
            var someOffline = _devices.Count == 0 || _devices.Any(d => !d.OnlineStatus);

            OnlineStatus = !someOffline;

            if (_prevOnlineStatus != OnlineStatus)
            {
                _prevOnlineStatus = OnlineStatus;
                //MyARM.Instance.CallBindKpp(this);           // avalonia не хочет биндить поэтому кастыль
            }
        }
        




        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
