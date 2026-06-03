using IRAPROM.MyCore.Auxiliary;
using IRAPROM.MyCore.DBModel;
using IRAPROM.MyCore.Model.MD;
//using Npgsql;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Casualbunker.Server.Common;
using IRAPROM.MyCore.Device;

namespace IRAPROM.MyCore.Model
{
    public class MonopanelPacketInfo : MDSaveInfo
    {
        //public byte[] mac = new byte[6];
        //public string MAC => MyTools.ConvertByteArrayToHexString(mac).Replace(" ", "");

        //public DateTime logTime;

        public uint NormalPassNum = 0;
        public uint NormalReturnNum = 0;
        public uint AlarmPassNum = 0;
        public uint AlarmReturnNum = 0;


        public byte ZonesSensorMode = 0;
        //public string SensorModeName => MDSensorMode.GetItemName(sensorMode);
        public string SensorModeName => $"Режим/Зоны обнаруж = {ZonesSensorMode}";

        public byte[] sensors = new byte[18];
        public byte[] sensorsProcessed = new byte[18];


        //public decimal? Temperature = null;
        //public string Explosives = "";
        //public short? Radiation = null;


        /*
        public byte alarm_leftzone_l = 0;
        public byte alarm_leftzone_h = 0;
        public byte alarm_rightzone_l = 0;
        public byte alarm_rightzone_h = 0;
        */

        //public string NormalInfName => $"{NormalPassNum}/R{NormalReturnNum}  A{AlarmPassNum}/AR{AlarmReturnNum} ";
        public string NormalInfName => $"{NormalPassNum}  A{AlarmPassNum} ";


        public string SensorsStrDB
        {
            get
            {
                var result = "";
                for (var i = 1; i <= sensors.Length; i++)
                {
                    if (sensors[i - 1] == 0)
                        continue;

                    result += $"{i} ";

                }
                return result.Trim();
            }
        }


        public string SensorsStrMsg
        {
            get
            {
                var result = "";
                for (var i = 1; i <= sensors.Length; i++)
                {
                    if (sensors[i - 1] == 0)
                        continue;

                    switch (i)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                            //result += $"L{i} ";
                            result += $"{i} ";
                            break;

                            /*
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                            result += $"R{i - 6} ";
                            break;


                        case 13:
                        case 14:
                        case 15:
                        case 16:
                        case 17:
                        case 18:
                            result += $"C{i - 12} ";
                            break;
                            */
                    }

                }
                return result.Trim();
            }
        }


        public bool ExistAlarm()
        {
            return sensors.Any(i => i > 0);
        }

        public static MonopanelPacketInfo ParseImpulseMessageUDP(byte[] arr)
        {
            if (arr.Length != 29)
                return null;

            if (!Device.Impulse.Constants.CheckImpulseHeader(arr))
                return null;

            var rec = new MonopanelPacketInfo();

            rec.logTime = DateTime.Now;

            using (var ms = new MemoryStream(arr))
            {
                using (var br = new BinaryReader(ms))
                {
                    var artmp = br.ReadBytes(5); //0-4

                    var byAlarmPassNum = br.ReadBytes(3); //5-7 
                    rec.NormalPassNum = (uint)(((byAlarmPassNum[0] >> 4) * 10 + (byAlarmPassNum[0] & 0x0f)) * 10000
                                               + ((byAlarmPassNum[1] >> 4) * 10 + (byAlarmPassNum[1] & 0x0f)) * 100
                                               + (byAlarmPassNum[2] >> 4) * 10 + (byAlarmPassNum[2] & 0x0f));


                    byAlarmPassNum = br.ReadBytes(3); //8-10

                    rec.NormalReturnNum = (uint)(((byAlarmPassNum[0] >> 4) * 10 + (byAlarmPassNum[0] & 0x0f)) * 10000
                                                 + ((byAlarmPassNum[1] >> 4) * 10 + (byAlarmPassNum[1] & 0x0f)) * 100
                                                 + (byAlarmPassNum[2] >> 4) * 10 + (byAlarmPassNum[2] & 0x0f));



                    byAlarmPassNum = br.ReadBytes(3); //11-13
                    rec.AlarmPassNum = (uint)(((byAlarmPassNum[0] >> 4) * 10 + (byAlarmPassNum[0] & 0x0f)) * 10000 + ((byAlarmPassNum[1] >> 4) * 10 + (byAlarmPassNum[1] & 0x0f)) * 100 + (byAlarmPassNum[2] >> 4) * 10 + (byAlarmPassNum[2] & 0x0f));

                    byAlarmPassNum = br.ReadBytes(3); //8-10
                    rec.AlarmReturnNum = (uint)(((byAlarmPassNum[0] >> 4) * 10 + (byAlarmPassNum[0] & 0x0f)) * 10000 + ((byAlarmPassNum[1] >> 4) * 10 + (byAlarmPassNum[1] & 0x0f)) * 100 + (byAlarmPassNum[2] >> 4) * 10 + (byAlarmPassNum[2] & 0x0f));

                    var sen = br.ReadBytes(4); //17-20

                    for (var i = 0; i < 6; i++)
                    {
                        rec.sensors[i] = (byte)((sen[0] >> i) & 0x01);
                        rec.sensors[i + 12] = (byte)((sen[2] >> i) & 0x01);
                        rec.sensors[i + 6] = (byte)(rec.sensors[i] & rec.sensors[i + 12]);
                    }

                    var bMode = br.ReadByte(); //21

                    rec.ZonesSensorMode = (byte)MDSensorMode.enItems.Zones_0;

                    var btmp = br.ReadByte(); //22

                    rec.mac = br.ReadBytes(6); //23-28

                    if (MyARM.Instance.AddedDevicesTryGetValue(rec.MAC, out var metDetector, out var onChanged))
                    {
                        rec.MetDetector = metDetector;
                        SetSensorsProcessedImpulse(metDetector.Model, metDetector.ModelSeries, rec.sensors, rec.sensorsProcessed);  // PC 1800 MK

                        if (rec.ExistAlarm())
                        {
                            metDetector.DeviceMetalDetector.LastPassage = new MetalDetectorPassage(rec.MAC, rec.sensors, rec.logTime, rec.ZonesSensorMode, rec.NormalPassNum, rec.AlarmPassNum, rec.NormalReturnNum, rec.AlarmReturnNum);
                        }
                        else
                        {
                            metDetector.DeviceMetalDetector.LastPassage = new MetalDetectorPassage(rec.MAC, rec.logTime, rec.ZonesSensorMode, rec.NormalPassNum, rec.AlarmPassNum, rec.NormalReturnNum, rec.AlarmReturnNum);
                        }

                        onChanged(metDetector);
                    }
                }
            }

            return rec;
        }
        public static void SetSensorsProcessedImpulse(MetalDetectorModel idModel, MetalDetectorSeries series, byte[] sensors, byte[] sensorsProcessed)
        {
            if (idModel == MetalDetectorModel.PCVx9300_MZ6MK)
            {

                for (var i = 0; i < 6; i++)             //2 и 3 ряд ставим как 2
                {
                    sensorsProcessed[i] = (byte)sensors[i];
                    sensorsProcessed[i + 6] = (byte)sensors[i];
                    sensorsProcessed[i + 12] = (byte)sensors[i];
                }
                return;
            }

            if (series == MetalDetectorSeries.Impulse)
            {
                //SetSensorsProcessed(IdModel, 0, sensors, sensorsProcessed)
            }

            for (var i = 0; i < sensorsProcessed.Count(); i++)
            {
                sensorsProcessed[i] = (byte)sensors[i];
            }


            for (var i = 0; i < 6; i++)             //Если крайние зоны 1, то и центр устанавливаем в 1
            {
                if (sensorsProcessed[i] == 1)
                {
                    sensorsProcessed[i + 6] = 1;
                    sensorsProcessed[i + 12] = 1;
                }
            }
        }

    }//class MonopanelPacketInfo


}
