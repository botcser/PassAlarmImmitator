using IRAPROM.MyCore.Model.MD;
using System.ComponentModel;
using Device;
using Device.Impulse;
using Device.Matreshka;

namespace IRAPROM.MyCore.DBModel
{
    [Serializable]
    public class MetDetectorBase
    {
        public DeviceMetalDetector DeviceMetalDetector = null;

        public MetDetectorBase() { }

        public MetDetectorBase(MetDetector md)
        {
            MAC = md.MAC;
            Name = md.Name;
            IdKPP = md.IdKPP;
            ModelId = md.ModelId;
            Version = md.Version;
            IP = md.IP;
            Mask = md.Mask;
            PortUDP = md.PortUDP;
            PortTCP = md.PortTCP;
            Gateway = md.Gateway;
            WorkingMode = md.WorkingMode;
            ZonesSensorMode = md.ZonesSensorMode;
            InfraredMode = md.InfraredPassCounterMode;
            BaseSensitivity = md.BaseSensitivity;
            AlarmTimeLen = md.AlarmTimeLen;
            AlarmVol = md.AlarmVol;
            AlarmTone = md.AlarmTone;
            WorkProgram = md.WorkProgram;
            WorkingFreq = md.WorkingFreq;
            Relevant = md.Relevant;
            Sens01 = md.Sens01;
            Sens02 = md.Sens02;
            Sens03 = md.Sens03;
            Sens04 = md.Sens04;
            Sens05 = md.Sens05;
            Sens06 = md.Sens06;
            Sens07 = md.Sens07;
            Sens08 = md.Sens08;
            Sens09 = md.Sens09;
            Sens10 = md.Sens10;
            Sens11 = md.Sens11;
            Sens12 = md.Sens12;

            WebCamIP = md.WebCamIP;
            ExplosivesSensor = md.ExplosivesSensor;
            TemperatureSensor = md.TemperatureSensor;
            RadiationSensor = md.RadiationSensor;

            //-----------
            KPPName = md.KPPName;
            dtLastInf = md.dtLastInf;
            dtLastAlarm = md.dtLastAlarm;
            FindNetworkStatus = md.FindNetworkStatus;
            dtLastInfFindMD = md.dtLastInfFindMD;
            dtLastPing = md.dtLastPing;
            NormalPass = md.NormalPass;
            NormalReturn = md.NormalReturn;
            AlarmPass = md.AlarmPass;
            AlarmReturn = md.AlarmReturn;
            dtLastTemperatureForLink = md.dtLastTemperatureForLink;
            dtLastTemperature = md.dtLastTemperature;
            LastTemperature = md.LastTemperature;
            dtLastExplosivesForLink = md.dtLastExplosivesForLink;
            dtLastExplosives = md.dtLastExplosives;
            LastExplosives = md.LastExplosives;
            RadiationTrevoga = md.RadiationTrevoga;
            ModelType = md.ModelSeries;

            if (md.DeviceMetalDetector == null)
            {
                switch ((MetalDetectorModel)md.ModelId)
                {
                    case MetalDetectorModel.z400:
                    case MetalDetectorModel.x400:
                    case MetalDetectorModel.z600:
                    case MetalDetectorModel.x600:
                    case MetalDetectorModel.MZ6MK:
                    case MetalDetectorModel.z1200:
                    case MetalDetectorModel.x1200:
                    case MetalDetectorModel.z1800:
                    case MetalDetectorModel.x1800:
                    case MetalDetectorModel.PCV900:
                    case MetalDetectorModel.PCV1800:
                    case MetalDetectorModel.PCV33006:
                    case MetalDetectorModel.PCV4800:
                    case MetalDetectorModel.PCV6300:
                    case MetalDetectorModel.PCV9300:
                    case MetalDetectorModel.PCVx900:
                    case MetalDetectorModel.PCVx1800:
                    case MetalDetectorModel.PCVx3300:
                    case MetalDetectorModel.PCVx4800_MZmb:
                    case MetalDetectorModel.PCVx6300_MZmb:
                    case MetalDetectorModel.PCVx9300_MZ6MK:
                    case MetalDetectorModel.PCV90011:
                    case MetalDetectorModel.PCV180011:
                    case MetalDetectorModel.PCV3300:
                    case MetalDetectorModel.PCV480011:
                    case MetalDetectorModel.PCV630011_MV6mb:
                    case MetalDetectorModel.PCV930011:
                    case MetalDetectorModel.PCVx90011:
                    case MetalDetectorModel.PCVx180011:
                    case MetalDetectorModel.PCVx330011:
                    case MetalDetectorModel.PCVx480011:
                    case MetalDetectorModel.PCVx630011:
                    case MetalDetectorModel.PCVx930011:
                    case MetalDetectorModel.PCV480016_PCVi1800mb:
                    case MetalDetectorModel.PCV630016:
                    case MetalDetectorModel.PCV930016:
                    case MetalDetectorModel.PCVx480016_PCVi3300mb:
                    case MetalDetectorModel.PCVx630016:
                    case MetalDetectorModel.PCVx930016:
                    case MetalDetectorModel.MV6:
                    case MetalDetectorModel.MVx6:
                    case MetalDetectorModel.MV11_hz:
                    case MetalDetectorModel.MVx11_hz:
                    case MetalDetectorModel.MV16_hz:
                    case MetalDetectorModel.MVx16_hz:
                        DeviceMetalDetector = new Matreshka(md.IP, md.PortTCP) { MAC = MAC };
                        DeviceMetalDetector.WorkParams = md.GetWorkParams();
                        break;
                    case MetalDetectorModel.PC600MKZ:
                    case MetalDetectorModel.PC600MKX:
                    case MetalDetectorModel.PC1800MKZ:
                    case MetalDetectorModel.PC4400MK:
                    case MetalDetectorModel.PC4400MKZ:
                    case MetalDetectorModel.PC4400MKX:
                    case MetalDetectorModel.PC6300MKZ:
                    case MetalDetectorModel.PC6300MKX:
                        DeviceMetalDetector = new Impulse(md.IP, md.PortTCP) { MAC = MAC };
                        DeviceMetalDetector.WorkParams = md.GetWorkParams();
                        break;
                    case MetalDetectorModel.Unknown:
                    default:                                                                            // TODO ???
                        break;
                }
            }
            else
            {
                DeviceMetalDetector = md.DeviceMetalDetector;
            }
        }
        
        public string MAC { get; set; }


        public string Name { get; set; }

        public int IdKPP { get; set; } //КПП


        public short ModelId { get; set; } //Модель


        public string Version { get; set; } //Версия


        public string IP { get; set; } //IP-адрес


        public string Mask { get; set; } //Маска сети


        public short PortUDP { get; set; }


        public short PortTCP { get; set; } //Порт TCP

        public string Gateway { get; set; }

        public short WorkingMode { get; set; } //?????


        public short ZonesSensorMode { get; set; } //Определяет кол-во зон обслуживания


        public short InfraredMode { get; set; } //Infrared  в Delphi Режим работы датчиков прохода


        public short BaseSensitivity { get; set; } //SCYlevel в Delphi //Базовая чувствительность

        public short AlarmTimeLen { get; set; } //Длительность сигнала


        public short AlarmVol { get; set; } //Громкость сигнала

        public short AlarmTone { get; set; } //Мелодия  сигнала


        public short WorkProgram { get; set; } //WorkPlace в Delphi //Рабочая программа

        public short WorkingFreq { get; set; } //Рабочая частота

        public short Relevant { get; set; } //?????

        public short Sens01 { get; set; }


        public short Sens02 { get; set; }

        public short Sens03 { get; set; }

        public short Sens04 { get; set; }

        public short Sens05 { get; set; }

        public short Sens06 { get; set; }

        public short Sens07 { get; set; }

        public short Sens08 { get; set; }

        public short Sens09 { get; set; }

        public short Sens10 { get; set; }

        public short Sens11 { get; set; }


        public short Sens12 { get; set; }

        public string WebCamIP { get; set; }

        public string ExplosivesSensor { get; set; }

        public string TemperatureSensor { get; set; }


        public string RadiationSensor { get; set; }


        //--------------

        public string KPPName { get; set; }

        public DateTime dtLastInf { get; set; }

        public DateTime dtLastAlarm { get; set; }

        public short FindNetworkStatus { get; set; }

        public DateTime dtLastInfFindMD { get; set; }


        public DateTime dtLastPing { get; set; }


        public int NormalPass { get; set; }

        public int NormalReturn { get; set; }

        public int AlarmPass { get; set; }

        public int AlarmReturn { get; set; }


        //Работает как триггер - при срабатывании прохода в запись о проходе добавляется информация о температуре по времени
        //При нахождении температуры - время сбрасывается
        public DateTime dtLastTemperatureForLink { get; set; }

        public DateTime dtLastTemperature { get; set; }

        public decimal LastTemperature { get; set; }

        //Работает как триггер - при срабатывании прохода в запись о проходе добавляется информация о в/в по времени
        //При нахождении в/в - время сбрасывается
        public DateTime dtLastExplosivesForLink { get; set; }


        public DateTime dtLastExplosives { get; set; }

        public string LastExplosives { get; set; }

        public DateTime dtLastRadiation { get; set; }

        public bool RadiationTrevoga { get; set; }
        
        public MetalDetectorSeries ModelType { get; set; } = MetalDetectorSeries.Unknown;// Для поиска
    }


    [Serializable]
    public partial class MetDetector : INotifyPropertyChanged
    {
        private short _alarmTimeLen;
        private short _alarmTone;
        private short _alarmVol;
        private short _baseSensitivity;
        private string _explosivesSensor;
        private string _gateway;
        private short _modelId = -1;
        private short _infraredPassCounterMode; //Infrared в Delphi Режим работы датчиков прохода
        private string _ip;
        private string _mac;
        private string _mask;
        private string _name;
        private short _portTcp;
        private short _portUdp;

        private string _radiationSensor;
        private short _sens01;
        private short _sens02;
        private short _sens03;
        private short _sens04;
        private short _sens05;
        private short _sens06;
        private short _sens07;
        private short _sens08;
        private short _sens09;
        private short _sens10;
        private short _sens11;
        private short _sens12;
        private short _zonesSensorMode; //Определяет кол-во зон обслуживания
        private string _temperatureSensor;
        private string _version; //Версия
        private string _webCamIp;
        private short _workingFreq; //Рабочая частота
        private short _workProgram; //WorkPlace в Delphi //Рабочая программа

        public MetalDetectorModel Model = MetalDetectorModel.Unknown;
        public string MAC
        {
            get => _mac ?? DeviceMetalDetector.MAC;
            set
            {
                if (_mac == value) return;

                _mac = value;
                OnPropertyChanged("MAC");
            }
        }
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;

                _name = value;
                OnPropertyChanged("Name");
            }
        }
        public int IdKPP { get; set; } //КПП
        public short ModelId
        {
            get => _modelId;
            set
            {
                if (_modelId == value) return;

                _modelId = value;
                OnPropertyChanged("ModelId");
                OnPropertyChanged("ModelName");
            }
        }
        public string Version
        {
            get => _version;
            set
            {
                if (_version == value) return;

                _version = value;
                OnPropertyChanged("Version");
            }
        }
        public string IP
        {
            get => _ip;
            set
            {
                if (_ip == value) return;

                _ip = value;
                OnPropertyChanged("IP");
            }
        }
        public string Mask
        {
            get => _mask;
            set
            {
                if (_mask == value) return;

                _mask = value;
                OnPropertyChanged("Mask");
            }
        }
        public short PortUDP
        {
            get => _portUdp;
            set
            {
                if (_portUdp == value) return;

                _portUdp = value;
                OnPropertyChanged("PortUDP");
            }
        }
        public short PortTCP
        {
            get => _portTcp;
            set
            {
                if (_portTcp == value) return;

                _portTcp = value;
                OnPropertyChanged("PortTCP");
            }
        }
        public string Gateway
        {
            get => _gateway;
            set
            {
                if (_gateway == value) return;

                _gateway = value;
                OnPropertyChanged("Gateway");
            }
        }
        public short WorkingMode { get; set; } //?????
        public short ZonesSensorMode
        {
            get => _zonesSensorMode;
            set
            {
                if (_zonesSensorMode == value) return;

                _zonesSensorMode = value;
                OnPropertyChanged("ZonesSensorMode");
                OnPropertyChanged("SensorModeName");
            }
        }
        public short InfraredPassCounterMode
        {
            get => _infraredPassCounterMode;
            set
            {
                if (_infraredPassCounterMode == value) return;

                _infraredPassCounterMode = value;
                OnPropertyChanged("InfraredPassCounterMode");
                OnPropertyChanged("InfraredModeName");
            }
        }
        public short BaseSensitivity
        {
            get => _baseSensitivity;
            set
            {
                if (_baseSensitivity == value) return;

                _baseSensitivity = value;
                OnPropertyChanged("BaseSensitivity");
            }
        }
        public short AlarmTimeLen
        {
            get => _alarmTimeLen;
            set
            {
                if (_alarmTimeLen == value) return;

                _alarmTimeLen = value;
                OnPropertyChanged("AlarmTimeLen");
            }
        }
        public short AlarmVol
        {
            get => _alarmVol;
            set
            {
                if (_alarmVol == value) return;

                _alarmVol = value;
                OnPropertyChanged("AlarmVol");
            }
        }
        public short AlarmTone
        {
            get => _alarmTone;
            set
            {
                if (_alarmTone == value) return;

                _alarmTone = value;
                OnPropertyChanged("AlarmTone");
            }
        }
        public short WorkProgram
        {
            get => _workProgram;
            set
            {
                if (_workProgram == value) return;

                _workProgram = value;
                OnPropertyChanged("WorkProgram");
                OnPropertyChanged("WorkProgramName");
            }
        }
        public short WorkingFreq
        {
            get => _workingFreq;
            set
            {
                if (_workingFreq == value) return;

                _workingFreq = value;
                OnPropertyChanged("WorkingFreq");
            }
        }
        public short Relevant { get; set; } //?????
        public short Sens01
        {
            get => _sens01;
            set
            {
                if (_sens01 == value) return;

                _sens01 = value;
                OnPropertyChanged("Sens01");
            }
        }
        public short Sens02
        {
            get => _sens02;
            set
            {
                if (_sens02 == value) return;

                _sens02 = value;
                OnPropertyChanged("Sens02");
            }
        }
        public short Sens03
        {
            get => _sens03;
            set
            {
                if (_sens03 == value) return;

                _sens03 = value;
                OnPropertyChanged("Sens03");
            }
        }
        public short Sens04
        {
            get => _sens04;
            set
            {
                if (_sens04 == value) return;

                _sens04 = value;
                OnPropertyChanged("Sens04");
            }
        }
        public short Sens05
        {
            get => _sens05;
            set
            {
                if (_sens05 == value) return;

                _sens05 = value;
                OnPropertyChanged("Sens05");
            }
        }
        public short Sens06
        {
            get => _sens06;
            set
            {
                if (_sens06 == value) return;

                _sens06 = value;
                OnPropertyChanged("Sens06");
            }
        }
        public short Sens07
        {
            get => _sens07;
            set
            {
                if (_sens07 == value) return;

                _sens07 = value;
                OnPropertyChanged("Sens07");
            }
        }
        public short Sens08
        {
            get => _sens08;
            set
            {
                if (_sens08 == value) return;

                _sens08 = value;
                OnPropertyChanged("Sens08");
            }
        }
        public short Sens09
        {
            get => _sens09;
            set
            {
                if (_sens09 == value) return;

                _sens09 = value;
                OnPropertyChanged("Sens09");
            }
        }
        public short Sens10
        {
            get => _sens10;
            set
            {
                if (_sens10 == value) return;

                _sens10 = value;
                OnPropertyChanged("Sens10");
            }
        }
        public short Sens11
        {
            get => _sens11;
            set
            {
                if (_sens11 == value) return;

                _sens11 = value;
                OnPropertyChanged("Sens11");
            }
        }
        public short Sens12
        {
            get => _sens12;
            set
            {
                if (_sens12 == value) return;

                _sens12 = value;
                OnPropertyChanged("Sens12");
            }
        }
        public string WebCamIP
        {
            get => _webCamIp;
            set
            {
                if (_webCamIp == value) return;

                _webCamIp = value;
                OnPropertyChanged("WebCamIP");
            }
        }
        public string ExplosivesSensor
        {
            get => _explosivesSensor;
            set
            {
                if (_explosivesSensor == value) return;

                _explosivesSensor = value;
                OnPropertyChanged("ExplosivesSensor");
            }
        }
        public string TemperatureSensor
        {
            get => _temperatureSensor;
            set
            {
                if (_temperatureSensor == value) return;

                _temperatureSensor = value;
                OnPropertyChanged("TemperatureSensor");
            }
        }
        public string RadiationSensor
        {
            get => _radiationSensor;
            set
            {
                if (_radiationSensor == value) return;

                _radiationSensor = value;
                OnPropertyChanged("RadiationSensor");
            }
        }


        //*********** Добавили ************************************

        #region Implement INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        public void Init(Model.WP.WorkParams workParams)
        {
            if (workParams == null) return;

            AlarmMode = workParams.AlarmMode;
            ZonesSensorMode = workParams.ZonesSensorMode;
            InfraredPassCounterMode = workParams.InfraredPassCounterMode;
            AlarmTimeLen = workParams.AlarmDuration;
            AlarmVol = workParams.AlarmVolume;
            AlarmTone = workParams.AlarmTone;
            BaseSensitivity = workParams.BaseSensitivity;
            WorkingFreq = workParams.WorkingFreq;
            WorkProgram = workParams.WorkProgram;
            Model = (MetalDetectorModel)workParams.ModelId;
            ModelId = workParams.ModelId;
            AlarmZoneMode = workParams.AlarmInfraMode;
            ZoneMode = workParams.ZoneMode;
            MaxZoneMode = workParams.MaxZoneMode;
            AlarmLampSwapMode = workParams.AlarmLampSwapMode;
            ExchangeFrontBack = workParams.ExchangeFrontBack;
            BackwardAlarmsCount = workParams.BackwardAlarmsCount;
            BackwardPassageCount = workParams.BackwardPassageCount;
            ForwardPassageCount = workParams.ForwardPassageCount;
            ForwardAlarmsCount = workParams.ForwardAlarmsCount;
            SceneMode = workParams.SceneMode;
            MAC = workParams.MAC;
            GetMetSensorsFieldsFromArray(workParams);
        }

        public long BackwardPassageCount { get; set; }

        public long BackwardAlarmsCount { get; set; }

        public long ForwardPassageCount { get; set; }

        public long ForwardAlarmsCount { get; set; }

        public byte SceneMode { get; set; }

        public Model.WP.WorkParams GetWorkParams()
        {
            var workParams = new Model.WP.WorkParams();

            workParams.AlarmMode = AlarmMode;
            workParams.ZonesSensorMode = (byte)ZonesSensorMode;
            workParams.InfraredPassCounterMode = (byte)InfraredPassCounterMode;
            workParams.AlarmDuration = (byte)AlarmTimeLen;
            workParams.AlarmVolume = (byte)AlarmVol;
            workParams.AlarmTone = (byte)AlarmTone;
            workParams.BaseSensitivity = (byte)BaseSensitivity;
            workParams.WorkingFreq = (byte)WorkingFreq;
            workParams.WorkProgram = (byte)WorkProgram;
            workParams.ModelId = (byte)Model;
            workParams.AlarmInfraMode = AlarmZoneMode;
            workParams.ZoneMode = ZoneMode;
            workParams.MaxZoneMode = MaxZoneMode;
            workParams.AlarmLampSwapMode = AlarmLampSwapMode;
            workParams.ExchangeFrontBack = ExchangeFrontBack;
            workParams.SensorsSensitivity = GetArrayFieldsFromMetSensors();
            workParams.IP = IP;
            workParams.PortTCP = PortTCP;
            workParams.PortUDP = PortUDP;
            workParams.Gateway = Gateway;
            workParams.Mask = Mask;
            workParams.MAC = MAC;

            return workParams;
        }

        public byte AlarmMode { get; set; }

        private void GetMetSensorsFieldsFromArray(Model.WP.WorkParams workParams)
        {
            Sens01 = workParams.SensorsSensitivity[0];
            Sens02 = workParams.SensorsSensitivity[1];
            Sens03 = workParams.SensorsSensitivity[2];
            Sens04 = workParams.SensorsSensitivity[3];
            Sens05 = workParams.SensorsSensitivity[4];
            Sens06 = workParams.SensorsSensitivity[5];

            switch (Model)
            {
                case MetalDetectorModel.z400:
                case MetalDetectorModel.x400:
                case MetalDetectorModel.z600:
                case MetalDetectorModel.x600:
                case MetalDetectorModel.z1200:
                case MetalDetectorModel.x1200:
                case MetalDetectorModel.z1800:
                case MetalDetectorModel.x1800:
                case MetalDetectorModel.PCV900:
                case MetalDetectorModel.PCV1800:
                case MetalDetectorModel.PCV33006:
                case MetalDetectorModel.PCV4800:
                case MetalDetectorModel.PCV6300:
                case MetalDetectorModel.PCV9300:
                case MetalDetectorModel.PCVx1800:
                case MetalDetectorModel.PCVx3300:
                case MetalDetectorModel.PCVx4800_MZmb:
                case MetalDetectorModel.PCVx6300_MZmb:
                case MetalDetectorModel.PCVx9300_MZ6MK:
                case MetalDetectorModel.PCV90011:
                case MetalDetectorModel.PCV180011:
                case MetalDetectorModel.PCV3300:
                case MetalDetectorModel.PCV480011:
                case MetalDetectorModel.PCV630011_MV6mb:
                case MetalDetectorModel.PCV930011:
                case MetalDetectorModel.PCVx90011:
                case MetalDetectorModel.PCVx180011:
                case MetalDetectorModel.PCVx330011:
                case MetalDetectorModel.PCVx480011:
                case MetalDetectorModel.PCVx630011:
                case MetalDetectorModel.PCVx930011:
                case MetalDetectorModel.PCV480016_PCVi1800mb:
                case MetalDetectorModel.PCV630016:
                case MetalDetectorModel.PCV930016:
                case MetalDetectorModel.PCVx480016_PCVi3300mb:
                case MetalDetectorModel.PCVx630016:
                case MetalDetectorModel.PCVx930016:
                case MetalDetectorModel.PC600MKZ:
                case MetalDetectorModel.PC600MKX:
                case MetalDetectorModel.PC1800MKZ:
                case MetalDetectorModel.PC4400MK:
                case MetalDetectorModel.PC4400MKZ:
                case MetalDetectorModel.PC4400MKX:
                case MetalDetectorModel.PC6300MKZ:
                case MetalDetectorModel.PC6300MKX:
                    Sens07 = workParams.SensorsSensitivity[6];
                    Sens08 = workParams.SensorsSensitivity[7];
                    Sens09 = workParams.SensorsSensitivity[8];
                    Sens10 = workParams.SensorsSensitivity[9];
                    Sens11 = workParams.SensorsSensitivity[10];
                    Sens12 = workParams.SensorsSensitivity[11];
                    break;
                case MetalDetectorModel.PCVx900:
                case MetalDetectorModel.MV6:
                case MetalDetectorModel.MVx6:
                case MetalDetectorModel.MV11_hz:
                case MetalDetectorModel.MVx11_hz:
                case MetalDetectorModel.MV16_hz:
                case MetalDetectorModel.MVx16_hz:
                case MetalDetectorModel.MZ6MK:
                case MetalDetectorModel.Unknown:
                default:
                    break;
            }
        }

        private short[] GetArrayFieldsFromMetSensors()
        {
            var sensorsSensitivity = new short[12];

            sensorsSensitivity[0] = Sens01;
            sensorsSensitivity[1] = Sens02;
            sensorsSensitivity[2] = Sens03;
            sensorsSensitivity[3] = Sens04;
            sensorsSensitivity[4] = Sens05;
            sensorsSensitivity[5] = Sens06;

            switch (Model)
            {
                case MetalDetectorModel.z400:
                case MetalDetectorModel.x400:
                case MetalDetectorModel.z600:
                case MetalDetectorModel.x600:
                case MetalDetectorModel.z1200:
                case MetalDetectorModel.x1200:
                case MetalDetectorModel.z1800:
                case MetalDetectorModel.x1800:
                case MetalDetectorModel.PCV900:
                case MetalDetectorModel.PCV1800:
                case MetalDetectorModel.PCV33006:
                case MetalDetectorModel.PCV4800:
                case MetalDetectorModel.PCV6300:
                case MetalDetectorModel.PCV9300:
                case MetalDetectorModel.PCVx1800:
                case MetalDetectorModel.PCVx3300:
                case MetalDetectorModel.PCVx4800_MZmb:
                case MetalDetectorModel.PCVx6300_MZmb:
                case MetalDetectorModel.PCVx9300_MZ6MK:
                case MetalDetectorModel.PCV90011:
                case MetalDetectorModel.PCV180011:
                case MetalDetectorModel.PCV3300:
                case MetalDetectorModel.PCV480011:
                case MetalDetectorModel.PCV630011_MV6mb:
                case MetalDetectorModel.PCV930011:
                case MetalDetectorModel.PCVx90011:
                case MetalDetectorModel.PCVx180011:
                case MetalDetectorModel.PCVx330011:
                case MetalDetectorModel.PCVx480011:
                case MetalDetectorModel.PCVx630011:
                case MetalDetectorModel.PCVx930011:
                case MetalDetectorModel.PCV480016_PCVi1800mb:
                case MetalDetectorModel.PCV630016:
                case MetalDetectorModel.PCV930016:
                case MetalDetectorModel.PCVx480016_PCVi3300mb:
                case MetalDetectorModel.PCVx630016:
                case MetalDetectorModel.PCVx930016:
                case MetalDetectorModel.PC600MKZ:
                case MetalDetectorModel.PC600MKX:
                case MetalDetectorModel.PC1800MKZ:
                case MetalDetectorModel.PC4400MK:
                case MetalDetectorModel.PC4400MKZ:
                case MetalDetectorModel.PC4400MKX:
                case MetalDetectorModel.PC6300MKZ:
                case MetalDetectorModel.PC6300MKX:
                    sensorsSensitivity[6] = Sens07;
                    sensorsSensitivity[7] = Sens08;
                    sensorsSensitivity[8] = Sens09;
                    sensorsSensitivity[9] = Sens10;
                    sensorsSensitivity[10] = Sens11;
                    sensorsSensitivity[11] = Sens12;
                    break;
                case MetalDetectorModel.PCVx900:
                case MetalDetectorModel.MV6:
                case MetalDetectorModel.MVx6:
                case MetalDetectorModel.MV11_hz:
                case MetalDetectorModel.MVx11_hz:
                case MetalDetectorModel.MV16_hz:
                case MetalDetectorModel.MVx16_hz:
                case MetalDetectorModel.MZ6MK:
                case MetalDetectorModel.Unknown:
                default:
                    break;
            }

            return sensorsSensitivity;
        }
    }


    public partial class MetDetector
    {
        public Device.DeviceMetalDetector DeviceMetalDetector = null;

        private int _AlarmPass;
        
        private int _AlarmReturn;
        
        private DateTime _dtLastAlarm;

        private DateTime _dtLastInf;
        
        private DateTime _dtLastInfFindMD;

        private DateTime _dtLastPing;

        private short _FindNetworkStatus;

        private string _KPPName;

        private string _LastExplosives;

        private decimal _LastTemperature;
        
        private int _NormalPass;
        
        private int _NormalReturn;

        private bool _RadiationTrevoga;

        public MetDetector()
        {
        }

        public MetDetector(MetDetectorBase md)
        {
            MAC = md.MAC;
            Name = md.Name;
            IdKPP = md.IdKPP;
            ModelId = md.ModelId;
            Version = md.Version;
            IP = md.IP;
            Mask = md.Mask;
            PortUDP = md.PortUDP;
            PortTCP = md.PortTCP;
            Gateway = md.Gateway;
            WorkingMode = md.WorkingMode;
            ZonesSensorMode = md.ZonesSensorMode;
            InfraredPassCounterMode = md.InfraredMode;
            BaseSensitivity = md.BaseSensitivity;
            AlarmTimeLen = md.AlarmTimeLen;
            AlarmVol = md.AlarmVol;
            AlarmTone = md.AlarmTone;
            WorkProgram = md.WorkProgram;
            WorkingFreq = md.WorkingFreq;
            Relevant = md.Relevant;
            Sens01 = md.Sens01;
            Sens02 = md.Sens02;
            Sens03 = md.Sens03;
            Sens04 = md.Sens04;
            Sens05 = md.Sens05;
            Sens06 = md.Sens06;
            Sens07 = md.Sens07;
            Sens08 = md.Sens08;
            Sens09 = md.Sens09;
            Sens10 = md.Sens10;
            Sens11 = md.Sens11;
            Sens12 = md.Sens12;

            WebCamIP = md.WebCamIP;
            ExplosivesSensor = md.ExplosivesSensor;
            TemperatureSensor = md.TemperatureSensor;
            RadiationSensor = md.RadiationSensor;


            //-----------
            KPPName = md.KPPName;
            dtLastInf = md.dtLastInf;
            dtLastAlarm = md.dtLastAlarm;
            FindNetworkStatus = md.FindNetworkStatus;
            dtLastInfFindMD = md.dtLastInfFindMD;
            dtLastPing = md.dtLastPing;
            NormalPass = md.NormalPass;
            NormalReturn = md.NormalReturn;
            AlarmPass = md.AlarmPass;
            AlarmReturn = md.AlarmReturn;
            dtLastTemperatureForLink = md.dtLastTemperatureForLink;
            dtLastTemperature = md.dtLastTemperature;
            LastTemperature = md.LastTemperature;
            dtLastExplosivesForLink = md.dtLastExplosivesForLink;
            dtLastExplosives = md.dtLastExplosives;
            LastExplosives = md.LastExplosives;
            RadiationTrevoga = md.RadiationTrevoga;
            ModelSeries = md.ModelType;

            if (md.DeviceMetalDetector == null)
            {
                switch ((MetalDetectorModel)md.ModelId)
                {
                    case MetalDetectorModel.z400:
                    case MetalDetectorModel.x400:
                    case MetalDetectorModel.z600:
                    case MetalDetectorModel.x600:
                    case MetalDetectorModel.MZ6MK:
                    case MetalDetectorModel.z1200:
                    case MetalDetectorModel.x1200:
                    case MetalDetectorModel.z1800:
                    case MetalDetectorModel.x1800:
                    case MetalDetectorModel.PCV900:
                    case MetalDetectorModel.PCV1800:
                    case MetalDetectorModel.PCV33006:
                    case MetalDetectorModel.PCV4800:
                    case MetalDetectorModel.PCV6300:
                    case MetalDetectorModel.PCV9300:
                    case MetalDetectorModel.PCVx900:
                    case MetalDetectorModel.PCVx1800:
                    case MetalDetectorModel.PCVx3300:
                    case MetalDetectorModel.PCVx4800_MZmb:
                    case MetalDetectorModel.PCVx6300_MZmb:
                    case MetalDetectorModel.PCVx9300_MZ6MK:
                    case MetalDetectorModel.PCV90011:
                    case MetalDetectorModel.PCV180011:
                    case MetalDetectorModel.PCV3300:
                    case MetalDetectorModel.PCV480011:
                    case MetalDetectorModel.PCV630011_MV6mb:
                    case MetalDetectorModel.PCV930011:
                    case MetalDetectorModel.PCVx90011:
                    case MetalDetectorModel.PCVx180011:
                    case MetalDetectorModel.PCVx330011:
                    case MetalDetectorModel.PCVx480011:
                    case MetalDetectorModel.PCVx630011:
                    case MetalDetectorModel.PCVx930011:
                    case MetalDetectorModel.PCV480016_PCVi1800mb:
                    case MetalDetectorModel.PCV630016:
                    case MetalDetectorModel.PCV930016:
                    case MetalDetectorModel.PCVx480016_PCVi3300mb:
                    case MetalDetectorModel.PCVx630016:
                    case MetalDetectorModel.PCVx930016:
                    case MetalDetectorModel.MV6:
                    case MetalDetectorModel.MVx6:
                    case MetalDetectorModel.MV11_hz:
                    case MetalDetectorModel.MVx11_hz:
                    case MetalDetectorModel.MV16_hz:
                    case MetalDetectorModel.MVx16_hz:
                        DeviceMetalDetector = new Matreshka(md.IP, md.PortTCP) { MAC = MAC };
                        DeviceMetalDetector.WorkParams = GetWorkParams();
                        break;
                    case MetalDetectorModel.PC600MKZ:
                    case MetalDetectorModel.PC600MKX:
                    case MetalDetectorModel.PC1800MKZ:
                    case MetalDetectorModel.PC4400MK:
                    case MetalDetectorModel.PC4400MKZ:
                    case MetalDetectorModel.PC4400MKX:
                    case MetalDetectorModel.PC6300MKZ:
                    case MetalDetectorModel.PC6300MKX:
                        DeviceMetalDetector = new Impulse(md.IP, md.PortTCP) { MAC = MAC };
                        DeviceMetalDetector.WorkParams = GetWorkParams();
                        break;
                    case MetalDetectorModel.Unknown:
                    default:                                                                            // TODO ???
                        break;
                }
            }
            else
            {
                DeviceMetalDetector = md.DeviceMetalDetector;
            }
        }

        public string ModelName => MDModel.GetItemName(ModelId);

        public string SensorModeName => $"Режим/Зоны обнар = {ZonesSensorMode}";
        
        public string WorkProgramName
        {
            get
            {
                if (DeviceMetalDetector.WorkParams != null)
                {
                    return DeviceMetalDetector.FamilyInfo.WorkPrograms[DeviceMetalDetector.WorkParams.WorkProgram - 1];
                }

                return "0";
            }
        }

        public string KPPName
        {
            get => _KPPName;
            set
            {
                if (_KPPName != value)
                {
                    _KPPName = value;
                    OnPropertyChanged("KPPName");
                }
            }
        }

        public DateTime dtLastInf
        {
            get { return _dtLastInf; }
            set
            {
                if (_dtLastInf != value)
                {
                    _dtLastInf = value;
                    OnPropertyChanged("dtLastInf");
                }
            }
        }

        public DateTime dtLastAlarm
        {
            get { return _dtLastAlarm; }
            set
            {
                if (_dtLastAlarm != value)
                {
                    _dtLastAlarm = value;
                    OnPropertyChanged("dtLastAlarm");
                    OnPropertyChanged("dtLastAlarmName");
                }
            }
        }

        public string dtLastAlarmName => dtLastAlarm != default ? dtLastAlarm.ToString("dd.MM.yy HH:mm:ss") : "";

        public short FindNetworkStatus
        {
            get => _FindNetworkStatus;
            set
            {
                if (_FindNetworkStatus != value)
                {
                    _FindNetworkStatus = value;
                    OnPropertyChanged("FindNetworkStatus");
                    OnPropertyChanged("FindNetworkStatusName");
                }
            }
        }

        public DateTime dtLastInfFindMD
        {
            get => _dtLastInfFindMD;
            set
            {
                if (_dtLastInfFindMD != value)
                {
                    _dtLastInfFindMD = value;
                    OnPropertyChanged("dtLastInfFindMD");
                }
            }
        }

        public DateTime dtLastPing
        {
            get => _dtLastPing;
            set
            {
                if (_dtLastPing != value)
                {
                    _dtLastPing = value;
                    OnPropertyChanged("dtLastPing");
                }
            }
        }
        
        public int NormalPass
        {
            get => _NormalPass;
            set
            {
                if (_NormalPass == value) return;

                _NormalPass = value;
                OnPropertyChanged("NormalPass");
            }
        }

        public int NormalReturn
        {
            get => _NormalReturn;
            set
            {
                if (_NormalReturn == value) return;
                
                _NormalReturn = value;
                OnPropertyChanged("NormalReturn");
            }
        }

        public int AlarmPass
        {
            get => _AlarmPass;
            set
            {
                if (_AlarmPass == value) return;
                
                _AlarmPass = value;
                OnPropertyChanged("AlarmPass");
            }
        }

        public int AlarmReturn
        {
            get => _AlarmReturn;
            set
            {
                if (_AlarmReturn == value) return;
                
                _AlarmReturn = value;
                OnPropertyChanged("AlarmReturn");
            }
        }


        //Работает как триггер - при срабатывании прохода в запись о проходе добавляется информация о температуре по времени
        //При нахождении температуры - время сбрасывается
        public DateTime dtLastTemperatureForLink { get; set; }


        public DateTime dtLastTemperature { get; set; }

        public decimal LastTemperature
        {
            get { return _LastTemperature; }
            set
            {
                if (_LastTemperature != value)
                {
                    _LastTemperature = value;
                    OnPropertyChanged("LastTemperature");
                }
            }
        }
        
        //Работает как триггер - при срабатывании прохода в запись о проходе добавляется информация о в/в по времени
        //При нахождении в/в - время сбрасывается
        public DateTime dtLastExplosivesForLink { get; set; }


        public DateTime dtLastExplosives { get; set; }

        public string LastExplosives
        {
            get { return _LastExplosives; }
            set
            {
                if (_LastExplosives != value)
                {
                    _LastExplosives = value;
                    OnPropertyChanged("LastExplosives");
                }
            }
        }
        
        public bool RadiationTrevoga
        {
            get { return _RadiationTrevoga; }
            set
            {
                if (_RadiationTrevoga != value)
                {
                    _RadiationTrevoga = value;
                    OnPropertyChanged("RadiationTrevoga");
                }
            }
        }

        public MetalDetectorSeries ModelSeries
        {
            get; 
            set;
        } = MetalDetectorSeries.Unknown;// Для поиска

        // Дич! WorkParams должен быть полем MedDetector
        public string ZoneMode { get; set; }          // 86 byte Alarm Mode 7,6 bits
        public byte AlarmZoneMode { get; set; }         // 86 byte Alarm Mode 5,4 bits
        public byte MaxZoneMode { get; set; }              // 86 byte Alarm Mode 3,2 bits
        public byte AlarmLampSwapMode { get; set; }     // 86 byte Alarm Mode 1,0 bits
        public bool ExchangeFrontBack { get; set; }     // 85 byte Infrared Mode 0 1 2 3 bits
    }



    


    
    
}