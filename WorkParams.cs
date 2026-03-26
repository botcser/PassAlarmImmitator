using Casualbunker.Server.Common;
using IRAPROM.MyCore.DBModel;
using IRAPROM.MyCore.Model.MD;
using System.Net.Sockets;
using IRAPROM.MyCore.Device;

namespace IRAPROM.MyCore.Model.WP
{
    [Serializable]
    public class WorkParams : IEquatable<WorkParams>
    {
        public byte AlarmMode; // 86 байт данных
        public byte ZonesSensorMode { get; set; } = 0; // кол-во зон
        public byte InfraredPassCounterMode { get; set; } = 0; //Режим работы счетчика проходов
        public byte AlarmDuration { get; set; } //Длительность сигнала
        public byte AlarmVolume { get; set; } //Громкость сигнала
        public byte AlarmTone { get; set; } //Мелодия  сигнала
        public short BaseSensitivity { get; set; } //Базовая чувствительность
        public byte WorkingFreq { get; set; } //Рабочая частота
        public byte WorkProgram { get; set; } //Рабочая программа
        public byte ModelId { get; set; } = 0; //Модель

        public bool ExchangeFrontBack { get; set; }     // 85 byte Infrared Mode 8 7 6 5 bits
        public short[] SensorsSensitivity { get; set; }
        public string ZoneMode { get; set; }          // 86 byte Alarm Mode 7,6 bits: 00(33/24/18) 01(22/16/12) 10(11/8/6) || Matreshka: 2(33/24/18) 1(22/16/12) 0(11/8/6)
        public byte AlarmInfraMode { get; set; }         // 86 byte Alarm Mode 5,4 bits: Matreshka: UNUSABLE | Impulse: Alarm any OR Alarm largest only UNUSED
        public byte MaxZoneMode { get; set; }              // 86 byte Alarm Mode 3,2 bits: 00(33) 01(24) 10(18)
        public byte AlarmLampSwapMode { get; set; }     // 86 byte Alarm Mode 1,0 bits
        public string IP { get; set; }
        public string Mask { get; set; }
        public string Gateway { get; set; }
        public int PortTCP { get; set; }
        public int PortUDP { get; set; }
        public uint ForwardPassageCount { get; set; }
        public uint BackwardPassageCount { get; set; }
        public uint ForwardAlarmsCount { get; set; }
        public uint BackwardAlarmsCount { get; set; }
        public int Password { get; set; }
        public string FirmwareVersion { get; set; }
        public string SerialNumber { get; set; }
        public DateTime DateTime { get; set; }
        public string MAC { get; set; }
        
        public WorkParams() { }

        public WorkParams(MetDetector rec)
        {
            ZonesSensorMode = (byte)rec.ZonesSensorMode;
            InfraredPassCounterMode = (byte)rec.InfraredPassCounterMode;
            AlarmDuration = (byte)rec.AlarmTimeLen;
            AlarmVolume = (byte)rec.AlarmVol;
            AlarmTone = (byte)rec.AlarmTone;
            BaseSensitivity = (byte)rec.BaseSensitivity;
            ModelId = (byte)rec.Model;
            WorkingFreq = (byte)rec.WorkingFreq;
            WorkProgram = (byte)rec.WorkProgram;
            AlarmLampSwapMode = rec.AlarmLampSwapMode;
            AlarmInfraMode = rec.AlarmZoneMode;
            MaxZoneMode = rec.MaxZoneMode;
            ZoneMode = rec.ZoneMode;
            ExchangeFrontBack = rec.ExchangeFrontBack;
            MAC = rec.MAC;
            CheckSetOgranicheniya(rec);
            SetSensorsFieldsToArray(rec);
        }

        void CheckSetOgranicheniya(MetDetector rec)
        {
            if (rec.ModelId == (short)MetalDetectorModel.PCVx900)
            {
                if (AlarmDuration > 12)
                    AlarmDuration = 12;

                if (AlarmVolume > 12)
                    AlarmVolume = 12;

                if (AlarmTone > 12)
                    AlarmTone = 12;

                if (BaseSensitivity > 60)
                    BaseSensitivity = 60;

                if (WorkingFreq > 99)
                    WorkingFreq = 99;

                if (WorkProgram > 32)
                    WorkProgram = 32;

            }
            else
            {
                if (AlarmDuration > 99)
                    AlarmDuration = 99;

                if (AlarmVolume > 99)
                    AlarmVolume = 99;

                if (AlarmTone > 99)
                    AlarmTone = 99;

                if (BaseSensitivity > 60)   //?????
                    BaseSensitivity = 60;

                if (WorkingFreq > 50)
                    WorkingFreq = 50;

                if (WorkProgram > 34)
                    WorkProgram = 34;

            }

        }

        public void GetMetSensorsFieldsFromArray(MetDetector rec)
        {
            rec.Sens01 = SensorsSensitivity[0];
            rec.Sens02 = SensorsSensitivity[1];
            rec.Sens03 = SensorsSensitivity[2];
            rec.Sens04 = SensorsSensitivity[3];
            rec.Sens05 = SensorsSensitivity[4];
            rec.Sens06 = SensorsSensitivity[5];

            if (rec.ModelId != (short)MetalDetectorModel.PCVx900)
            {

                rec.Sens07 = SensorsSensitivity[6];
                rec.Sens08 = SensorsSensitivity[7];
                rec.Sens09 = SensorsSensitivity[8];
                rec.Sens10 = SensorsSensitivity[9];
                rec.Sens11 = SensorsSensitivity[10];
                rec.Sens12 = SensorsSensitivity[11];
            }
        }

        public void SetSensorsFieldsToArray(MetDetector rec)
        {
            short imax = 200;

            SensorsSensitivity[0] = rec.Sens01;
            if (rec.Sens01 > imax)
                SensorsSensitivity[0] = imax;


            SensorsSensitivity[1] = rec.Sens02;
            if (rec.Sens02 > imax)
                SensorsSensitivity[1] = imax;

            SensorsSensitivity[2] = rec.Sens03;
            if (rec.Sens03 > imax)
                SensorsSensitivity[2] = imax;

            SensorsSensitivity[3] = rec.Sens04;
            if (rec.Sens04 > imax)
                SensorsSensitivity[3] = imax;

            SensorsSensitivity[4] = rec.Sens05;
            if (rec.Sens05 > imax)
                SensorsSensitivity[4] = imax;

            SensorsSensitivity[5] = rec.Sens06;
            if (rec.Sens06 > imax)
                SensorsSensitivity[5] = imax;


            if (rec.ModelId != (short)MetalDetectorModel.PCVx900)
            {
                SensorsSensitivity[6] = rec.Sens07;
                if (rec.Sens07 > imax)
                    SensorsSensitivity[6] = imax;

                SensorsSensitivity[7] = rec.Sens08;
                if (rec.Sens08 > imax)
                    SensorsSensitivity[7] = imax;

                SensorsSensitivity[8] = rec.Sens09;
                if (rec.Sens09 > imax)
                    SensorsSensitivity[8] = imax;

                SensorsSensitivity[9] = rec.Sens10;
                if (rec.Sens10 > imax)
                    SensorsSensitivity[9] = imax;

                SensorsSensitivity[10] = rec.Sens11;
                if (rec.Sens11 > imax)
                    SensorsSensitivity[10] = imax;


                SensorsSensitivity[11] = rec.Sens12;
                if (rec.Sens12 > imax)
                    SensorsSensitivity[11] = imax;

            }
        }
        
        private static WorkParams ParseImpulseResponse(byte[] response, MetalDetectorModel model, MetalDetectorSeries series)
        {
            var resultWorkParams = new WorkParams();
            var zonesCount = (byte)((response[86] >> 2) & 0x03); //Кол-во зон
            
            if (model == MetalDetectorModel.Unknown)
            {
                switch (series)
                {
                    case MetalDetectorSeries.Impulse:
                        model = (MetalDetectorModel)response[80];
                        break;
                    case MetalDetectorSeries.Unknown:
                    case MetalDetectorSeries.BlockPost:
                    case MetalDetectorSeries.Matryoshka:
                    default:
                        break;
                }
            }

            //Режим счетчика проходов
            switch (model)
            {
                case MetalDetectorModel.PC600MKX:
                case MetalDetectorModel.PC1800MKZ:
                case MetalDetectorModel.PC4400MK:
                case MetalDetectorModel.PC600MKZ:
                case MetalDetectorModel.PC4400MKZ:
                case MetalDetectorModel.PC4400MKX:
                case MetalDetectorModel.PC6300MKZ:
                case MetalDetectorModel.PC6300MKX:
                    resultWorkParams.AlarmMode = response[86];
                    resultWorkParams.InfraredPassCounterMode = (byte)(response[85] & 0x0f);
                    resultWorkParams.ZonesSensorMode = (byte)(response[86] >> 6);
                    break;
                case MetalDetectorModel.PCVx900:
                case MetalDetectorModel.Unknown:
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
                default:
                    var passCounterMode = (byte)(response[85] & 0x0f);              // WTF!!!
                    //resultWorkParams.InfraredPassCounterMode = (byte)MDInfraredModeMonopanel.enItems.On;
                    //resultWorkParams.ZonesSensorMode = (byte)MDSensorMode.enItems.Zones_0;
                    break;
            }

            resultWorkParams.BaseSensitivity = response[76];
            resultWorkParams.WorkingFreq = response[77];
            resultWorkParams.AlarmDuration = response[78];
            resultWorkParams.WorkProgram = response[79];
            resultWorkParams.ModelId = response[80];
            resultWorkParams.AlarmVolume = response[81];
            resultWorkParams.AlarmTone = response[82];

            var nSens = new int[12];

            for (var i = 0; i < 6; i++)
            {
                nSens[i] = (response[2 * i + 9] << 8) + response[2 * i + 10];
                resultWorkParams.SensorsSensitivity[i] = (short)nSens[i];
            }

            for (var i = 0; i < 6; i++)
            {
                //nSens[6+i] = (response[2 * i + 31] << 8) + response[2 * i + 32];
                nSens[6 + i] = (response[2 * i + 21] << 8) + response[2 * i + 22];
                resultWorkParams.SensorsSensitivity[6+i] = (short)nSens[6 + i];
            }

            return resultWorkParams;
        }
        
        //-----------------------------------------------
        

        public bool Equals(WorkParams other)
        {
            return other != null && WorkProgram == other.WorkProgram && WorkingFreq == other.WorkingFreq && ExchangeFrontBack == other.ExchangeFrontBack && IP == other.IP &&
                   InfraredPassCounterMode == other.InfraredPassCounterMode && AlarmTone == other.AlarmTone && AlarmInfraMode == other.AlarmInfraMode && AlarmLampSwapMode == other.AlarmLampSwapMode &&
                   AlarmDuration == other.AlarmDuration && AlarmMode == other.AlarmMode && AlarmVolume == other.AlarmVolume && ForwardAlarmsCount == other.ForwardAlarmsCount && 
                   ForwardPassageCount == other.ForwardPassageCount && ZoneMode == other.ZoneMode && ZonesSensorMode == other.ZonesSensorMode && MAC == other.MAC && ModelId == other.ModelId;
        }
    } //class WorkParams
}