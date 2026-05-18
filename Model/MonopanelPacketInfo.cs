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
        
        
    }//class MonopanelPacketInfo
}
