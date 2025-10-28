using IRAPROM.MyCore.DBModel;
using IRAPROM.MyCore.Model.MD;

namespace IRAPROM.MyCore.Model.WP
{
    [Serializable]
    public class WorkParams
    {
        public byte AlarmMode; // 86 байт данных
        public byte ZonesSensorMode { get; set; } = 0; // Определяет кол-во зон
        public byte InfraredPassCounterMode { get; set; } = 0; //Режим работы счетчика проходов
        public byte AlarmDuration { get; set; } //Длительность сигнала
        public byte AlarmVolume { get; set; } //Громкость сигнала
        public byte AlarmTone { get; set; } //Мелодия  сигнала
        public short BaseSensitivity { get; set; } //Базовая чувствительность
        public byte WorkingFreq { get; set; } //Рабочая частота
        public byte WorkProgram { get; set; } //Рабочая программа
        public byte ModelId { get; set; } = 0; //Модель

        public bool ExchangeFrontBack { get; set; }     // 85 byte Infrared Mode 8 7 6 5 bits
        public short[] SensorsSensitivity { get; set; } = new short[32];//Базовая чувствительность
        public string ZoneMode { get; set; }          // 86 byte Alarm Mode 7,6 bits: 00(33/24/18) 01(22/16/12) 10(11/8/6)
        public byte SceneMode { get; set; }
        public byte AlarmInfraMode { get; set; }         // 86 byte Alarm Mode 5,4 bits: Matreshka: off front rear Both | Impulse: Alarm any OR Alarm largest only UNUSED
        public byte MaxZoneMode { get; set; }              // 86 byte Alarm Mode 3,2 bits: 00(33) 01(24) 10(18)
        public byte AlarmLampSwapMode { get; set; }     // 86 byte Alarm Mode 1,0 bits
        public string IP { get; set; }
        public string Mask { get; set; }
        public string Gateway { get; set; }
        public int PortTCP { get; set; }
        public int PortUDP { get; set; }
        public long ForwardPassageCount { get; set; }
        public long BackwardPassageCount { get; set; }
        public long ForwardAlarmsCount { get; set; }
        public long BackwardAlarmsCount { get; set; }
        public long Password { get; set; }
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

            rec.Sens07 = SensorsSensitivity[6];
            rec.Sens08 = SensorsSensitivity[7];
            rec.Sens09 = SensorsSensitivity[8];
            rec.Sens10 = SensorsSensitivity[9];
            rec.Sens11 = SensorsSensitivity[10];
            rec.Sens12 = SensorsSensitivity[11];
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

        public void InitMetDetector(MetDetector rec)
        {
            rec.ZonesSensorMode = ZonesSensorMode;
            rec.InfraredPassCounterMode = InfraredPassCounterMode;
            rec.AlarmTimeLen = AlarmDuration;
            rec.AlarmVol = AlarmVolume;
            rec.AlarmTone = AlarmTone;
            rec.BaseSensitivity = BaseSensitivity;
            rec.WorkingFreq = WorkingFreq;
            rec.WorkProgram = WorkProgram;
            rec.Model = (MetalDetectorModel)ModelId;
            GetMetSensorsFieldsFromArray(rec);
        }
    } //class WorkParams
}