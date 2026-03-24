using IRAPROM.MyCore.Auxiliary;
using IRAPROM.MyCore.DBModel;
using IRAPROM.MyCore.Device.KPP;
//using ReactiveUI;
//using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using IRAPROM.MyCore.Device;

namespace IRAPROM.MyCore.Model
{
    public class MyARM
    {
        public bool DeviceSearchInProgress = false;
        public bool bOtladka = true;
        public List<string> msgList = new List<string>();
        //public List<KPP> lstKPP = new List<KPP>();
        //public List<MyCOMTemperature> lstSensorsTemperature = new List<MyCOMTemperature>(); //Список МД с датчиками температур
        //public List<MyExplosivesSensor> lstExplosivesSensor = new List<MyExplosivesSensor>(); //Список датчиков взрывчатых веществ
        //public List<MyRadiationSensor> lstRadiationSensor = new List<MyRadiationSensor>(); //Список датчиков радиации
        public List<MetDetector> DevicesFound = new List<MetDetector>();
        
        public event Action<MetDetector, MetDetector> AddedDeviceOnChange;

        public const string cUserStartSettings = "adm";
        public const string cUserStartSettingsPassword = "11";

        public static MyARM Instance => _instance ??= new MyARM();

        public bool Busy { get; set; }

        public ObservableCollection<KPPDevice> KPPs
        {
            get => _kpps;
            set
            {
                _kpps = value;
                //OnPropertyChanged();
            }
        }

        private ObservableCollection<KPPDevice> _kpps = new ObservableCollection<KPPDevice>();
        private ConcurrentDictionary<string, MetDetector> _addedDevices = new ConcurrentDictionary<string, MetDetector>();
        private readonly object _lock = new object();

        private static MyARM _instance;

        public bool AddedDevicesTryAdd(string mac, MetDetector newMetDetector)
        {
            try
            {
                Busy = true;

                var deviceKpp = FindOrCreateKpp();
                
                if (_addedDevices.ContainsKey(mac))
                {
                    _addedDevices.TryGetValue(mac, out var oldMetDetector);

                    if (oldMetDetector == null)
                    {
                        deviceKpp?.TryAddDevice(newMetDetector.DeviceMetalDetector);
                    }
                    else
                    {
                        if (newMetDetector.Equals(oldMetDetector))
                        {
                            return true;
                        }

                        AddedDevicesTryRemove(mac, out _);

                        deviceKpp?.TryAddDevice(newMetDetector.DeviceMetalDetector, oldMetDetector.DeviceMetalDetector);
                    }
                }
                else
                {
                    deviceKpp?.TryAddDevice(newMetDetector.DeviceMetalDetector);
                }

                CallBindKpp(deviceKpp);
            }
            finally
            {
                Busy = false;
            }

            return AddedDeviceAdd(mac, newMetDetector);
            

            
            KPPDevice FindOrCreateKpp()
            {
                KPPDevice kpp;

                if (KPPs.Any(i => i.Id == newMetDetector.IdKPP))
                {
                    kpp = KPPs.FirstOrDefault(i => i.Id == newMetDetector.IdKPP);
                }
                else
                {
                    kpp = new KPPDevice() { Name = newMetDetector.KPPName, Id = newMetDetector.IdKPP };
                    KPPs.Add(kpp);
                }

                return kpp;
            }
        }

        public bool AddedDevicesAddForValidatorOnly(string mac, MetDetector newMetDetector)
        {
            if (_addedDevices.ContainsKey(mac))
            {
                _addedDevices.TryRemove(mac, out _);
            }

            return AddedDeviceAdd(mac, newMetDetector);
        }

        public bool AddedDevicesTryGetValue(string mac, out MetDetector metDetector, out Action<MetDetector> onChanged)
        {
            metDetector = null;
            onChanged = null;

            if (!_addedDevices.ContainsKey(mac)) return false;

            _addedDevices.TryGetValue(mac, out metDetector);

            onChanged = AddedDeviceOnChanged;

            return true;
        }

        public int AddedDevicesCount()
        {
            return ShowAddedDevices().Count;
        }

        public FrozenDictionary<string, MetDetector> ShowAddedDevices()
        {
            return _addedDevices.ToFrozenDictionary();
        }
        
        public void AddedDevicesSync(List<MetDetector> newDevices)
        {
            if (newDevices == null) return;

            try
            {
                lock (_lock)
                {
                    Busy = true;

                    RemoveNotExistDevices();

                    newDevices.ForEach(i =>
                    {
                        Busy = true;
                        AddedDevicesTryAdd(i.MAC, i);
                    });

                    void RemoveNotExistDevices()
                    {
                        var newMACs = newDevices.Select(i => i.MAC);
                        var listToRemove = _addedDevices.Where(addedDevice => !newMACs.Contains(addedDevice.Key));

                        foreach (var device in listToRemove)
                        {
                            AddedDevicesTryRemove(device.Key, out _);
                        }
                    }
                }
            }
            finally
            {
                Busy = false;
            }
        }
        
        public void AddedDevicesTryRemove(string mac, out MetDetector removeDetector)
        {
            _addedDevices.TryRemove(mac, out removeDetector);

            if (removeDetector != null)
            {
                var oldKpp = KPPs.FirstOrDefault(i => i.Devices.Any(d => d.MAC == mac));

                if (oldKpp != null)
                {
                    oldKpp.RemoveDevice(removeDetector.DeviceMetalDetector);
                    CallBindKpp(oldKpp);
                }
            }

            AddedDeviceOnChange?.Invoke(removeDetector, null);
        }

        public void CallBindKpp(KPPDevice kpp)
        {
            KPPs[KPPs.IndexOf(kpp)] = kpp;
        }

        public void UpdateKpps(ObservableCollection<KPPDevice> newKppItems)
        {
            try
            {
                Busy = true;

                RemoveNotExistKpPs();

                foreach (var newKpp in newKppItems)
                {
                    if (KPPs.All(i => i.Id != newKpp.Id))
                    {
                        KPPs.Add(newKpp);
                    }
                }
            }
            finally
            {
                Busy = false;
            }


            void RemoveNotExistKpPs()
            {
                var newIds = newKppItems.Select(i => i.Id);
                var listToRemove = KPPs.Where(kpp => !newIds.Contains(kpp.Id) && kpp.Id != 0);

                foreach (var kpp in listToRemove)
                {
                    KPPs.Remove(kpp);
                }
            }
        }

        public void AddedDevicesCleanPassage(DeviceMetalDetector device)
        {
            var metDevice = _addedDevices.FirstOrDefault(i => i.Value.MAC == device.MAC).Value;

            if (metDevice == null) return;

            KPPs.FirstOrDefault(i => i.Id == metDevice.IdKPP)?.CleanStatistics(device);

            metDevice.DeviceMetalDetector.LastPassage.Clean();
        }

        private bool AddedDeviceAdd(string mac, MetDetector metDetector)
        {
            var result = _addedDevices.TryAdd(mac, metDetector);

            AddedDeviceOnChange?.Invoke(null, metDetector);

            return result;
        }

        private void AddedDeviceOnChanged(MetDetector metDetector)
        {
            AddedDeviceOnChange?.Invoke(metDetector, metDetector);
        }



        //protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => this.RaisePropertyChanged(propertyName);
    }
}
