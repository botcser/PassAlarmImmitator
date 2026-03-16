using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Casualbunker.Server.Common;
using IRAPROM.MyCore.Auxiliary;
using IRAPROM.MyCore.DBModel;
using IRAPROM.MyCore.Device;
using IRAPROM.MyCore.Device.Matreshka;
using IRAPROM.MyCore.Model.MD;
using IRAPROM.MyCore.MyNetwork;

namespace IRAPROM.MyCore.Model
{
    public class MetalDetectPacketInfo : MDSaveInfo
    {

        public short IdModel = 0; //Добавил недавно

        public byte[] head = new byte[4];
        public byte deviceAdress = 0;
        public int frameLen = 0;
        public int msgNum = 0;
        public short command = 0;
        public byte[] body = new byte[256];

        // for alarm
        public byte[] timeStamp = new byte[8];
        byte ZonesSensorMode = 0;
        public string SensorModeName => $"Режим/Зоны обнаруж = {ZonesSensorMode}";

        public byte[] sensors = new byte[50];
        public byte[] sensorsProcessed = new byte[18];

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

                return result;
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
                            result += $"L{i} ";
                            break;

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
                    }

                }
                return result.Trim();
            }
        }




        ///// for normal
        public int NormalPassNum = 0;
        public int NormalReturnNum = 0;
        public int AlarmPassNum = 0;
        public int AlarmReturnNum = 0;

        //public decimal? Temperature = null;
        //public string Explosives = "";
        //public short? Radiation = null;
        
        public string NormalInfName => $"{NormalPassNum}/R{NormalReturnNum}  A{AlarmPassNum}/AR{AlarmReturnNum} ";



        ///// for FindAnswer
        public DeviceFindAnswerNetworkInf deviceFindAnswerNetworkInf = null;

        //extra info from TCP/IP components
        string IP = "";
        ushort port = 0;
        string DestCpuIP = "";
        short DestCpuPort = 0;     // this computer's UDP port




        public static bool CheckMatreshkaHeader(byte[] arr)
        {
            if ((arr[0] == Constants.ResponseMagicNumber[0]) && (arr[1] == Constants.ResponseMagicNumber[1]) && (arr[2] == Constants.ResponseMagicNumber[2]))
                return true;

            if ((arr[0] == Constants.RequestMagicNumber[0]) && (arr[1] == Constants.RequestMagicNumber[1]) && (arr[2] == Constants.RequestMagicNumber[2]))
                return true;

            if ((arr[0] == Constants.RequestMagicNumberMonopanel[0]) && (arr[1] == Constants.RequestMagicNumberMonopanel[1]) && (arr[2] == Constants.RequestMagicNumberMonopanel[2]))
                return true;

            return false;
        }

        public static MetalDetectPacketInfo ParseMatreshkaMessageUDP(byte[] arr)
        {
            if (arr.Length < 14) return null;

            if (!CheckMatreshkaHeader(arr)) return null;

            MetalDetectPacketInfo rec = null;

            using (var ms = new MemoryStream(arr))
            {
                using (var br = new BinaryReader(ms))
                {
                    rec = new MetalDetectPacketInfo();
                    rec.logTime = DateTime.Now;
                    rec.head = br.ReadBytes(4); //4
                    rec.deviceAdress = rec.head[3];

                    if (arr.Length == 0x14) // PC V X PRO
                    {
                        rec.body = br.ReadBytes(arr.Length - 6);

                        if (Constants.FindAnswerCodes.Contains(rec.command))
                        {
                            var devInf = new DeviceFindAnswerNetworkInf();

                            using (var ms2 = new MemoryStream(arr))
                            {
                                using (var br2 = new BinaryReader(ms))
                                {
                                    devInf.Model = Convert.ToHexString(br2.ReadBytes(6));
                                    devInf.mac = br2.ReadBytes(6);
                                }
                            }

                            rec.deviceFindAnswerNetworkInf = devInf;
                            rec.mac = devInf.mac;

                            return rec;
                        }
                    }
                    else // old PC V
                    {
                        rec.frameLen = br.ReadInt32(); //4
                        rec.msgNum = br.ReadInt32(); //4
                        rec.command = br.ReadInt16(); //2

                        rec.body = br.ReadBytes(arr.Length - 14);

                        if (Constants.FindAnswerCodes.Contains(rec.command))
                        {
                            var devInf = DeviceFindAnswerNetworkInf.GetRecFromPacket(rec.body);

                            if (devInf == null)
                                return null;

                            rec.deviceFindAnswerNetworkInf = devInf;
                            rec.mac = devInf.mac;

                            return rec;
                        }
                    }
                }
            }

            //С 15 байта
            using (var ms = new MemoryStream(rec.body))
            {
                using (var br = new BinaryReader(ms))
                {
                    //с 15 байта
                    if (rec.body.Length >= 6)
                    {
                        rec.mac = br.ReadBytes(6);
                    }

                    if (arr.Length < 36)
                    {
                        return rec;
                    }


                    MetDetector md = null;
                    short modelId = 0;
                    var MAC = Convert.ToHexString(rec.mac);
                    
                    switch (rec.command)
                    {
                        case MDCommands.METDET_CMD_ALARM:
                            rec.timeStamp = br.ReadBytes(7); //с 21 байта
                            rec.ZonesSensorMode = br.ReadByte(); //28 байт
                            rec.sensors = br.ReadBytes(18); //с 29 байта    // 6 байт у PC Z 3300 MK
                            
                            if (md != null)
                            {
                                md.DeviceMetalDetector.LastPassage = new MetalDetectorPassage(MAC, rec.sensors, rec.logTime, rec.ZonesSensorMode);
                            }
                            break;

                        case MDCommands.METDET_CMD_NORMAL_GET_PASSAGES:
                            rec.NormalPassNum = br.ReadInt32(); // с 21 байта
                            rec.NormalReturnNum = br.ReadInt32(); //с 25 байта
                            rec.AlarmPassNum = br.ReadInt32(); //с 29 байта
                            rec.AlarmReturnNum = br.ReadInt32(); //с 33 байта

                            if (md != null)
                            {
                                md.DeviceMetalDetector.LastPassage = new MetalDetectorPassage(MAC, rec.logTime, rec.ZonesSensorMode, rec.NormalPassNum, rec.AlarmPassNum, rec.NormalReturnNum, rec.AlarmReturnNum);
                            }
                            break;

                        default:
                            break;
                    }
                }

            }

            return rec;
        }

        public static MetalDetectPacketInfo ParseXGOSTMatreshkaMessageUDP(byte[] arr)
        {
            if (!CheckMatreshkaHeader(arr)) return null;

            var rec = new MetalDetectPacketInfo();

            if (arr.Length == Device.Matreshka.XGOST.Constants.CommandResponseLength)
            {
                var deviceMetalDetector = DeviceMetalDetector.FamilyInfoVariants[2].ParseFindCommandResponse(arr, out var commandCode);

                rec.MetDetector.DeviceMetalDetector = deviceMetalDetector;
                rec.Ip = rec.MetDetector.IP = deviceMetalDetector.IP;
                rec.Mask = rec.MetDetector.Mask = deviceMetalDetector.Mask;
                rec.Gateway = rec.MetDetector.Gateway = deviceMetalDetector.Gateway;
                rec.port = rec.MetDetector.PortTCP = deviceMetalDetector.PortTCP;
                rec.Mac = rec.MetDetector.MAC = deviceMetalDetector.MAC;
                rec.mac = Convert.FromHexString(deviceMetalDetector.MAC);
                rec.MetDetector.ModelId = (short)deviceMetalDetector.ModelId;
                rec.ProductModel = rec.MetDetector.Name = deviceMetalDetector.ProductModelName;
                rec.UDPPort = rec.MetDetector.PortUDP = deviceMetalDetector.PortUDP;
                rec.TCPPort = rec.MetDetector.PortTCP = deviceMetalDetector.PortTCP;
                rec.command = (short)commandCode;

                return rec;
            }

            //С 15 байта
            using (var ms = new MemoryStream(rec.body))
            {
                using (var br = new BinaryReader(ms))
                {
                    //с 15 байта
                    if (rec.body.Length >= 6)
                    {
                        rec.mac = br.ReadBytes(6);
                    }

                    if (arr.Length < 36)
                    {
                        return rec;
                    }


                    MetDetector md = null;
                    short modelId = 0;
                    var MAC = Convert.ToHexString(rec.mac);

                    switch (rec.command)
                    {
                        case MDCommands.METDET_CMD_ALARM:
                            rec.timeStamp = br.ReadBytes(7); //с 21 байта
                            rec.ZonesSensorMode = br.ReadByte(); //28 байт
                            rec.sensors = br.ReadBytes(18); //с 29 байта    // 6 байт у PC Z 3300 MK

                            if (md != null)
                            {
                                md.DeviceMetalDetector.LastPassage = new MetalDetectorPassage(MAC, rec.sensors, rec.logTime, rec.ZonesSensorMode);
                            }
                            break;

                        case MDCommands.METDET_CMD_NORMAL_GET_PASSAGES:
                            rec.NormalPassNum = br.ReadInt32(); // с 21 байта
                            rec.NormalReturnNum = br.ReadInt32(); //с 25 байта
                            rec.AlarmPassNum = br.ReadInt32(); //с 29 байта
                            rec.AlarmReturnNum = br.ReadInt32(); //с 33 байта

                            if (md != null)
                            {
                                md.DeviceMetalDetector.LastPassage = new MetalDetectorPassage(MAC, rec.logTime, rec.ZonesSensorMode, rec.NormalPassNum, rec.AlarmPassNum, rec.NormalReturnNum, rec.AlarmReturnNum);
                            }
                            break;

                        default:
                            break;
                    }
                }

            }

            return rec;
        }

        public static DeviceMetalDetector MakeMetalDeviceFromPacketInfo(MetalDetectPacketInfo rec, MetalDetectorSeries series)
        {
            var dev = new MetDetector
            {
                MAC = rec.MAC,
                IP = rec.deviceFindAnswerNetworkInf.IP,
                Mask = rec.deviceFindAnswerNetworkInf.Mask,
                PortTCP = rec.deviceFindAnswerNetworkInf.PortTCP,
                PortUDP = rec.deviceFindAnswerNetworkInf.PortUDP,
                Gateway = rec.deviceFindAnswerNetworkInf.IPGateway
            };

            if (rec.deviceFindAnswerNetworkInf.Model == MDModel.cMZ6MK)
            {
                dev.ModelId = (short)MetalDetectorModel.PCVx9300_MZ6MK;
            }

            switch (series)
            {
                case MetalDetectorSeries.Matryoshka:
                    dev.Name = $"{dev.IP}:{dev.MAC}";
                    dev.ModelSeries = MetalDetectorSeries.Matryoshka;
                    dev.DeviceMetalDetector = new Device.Matreshka.Matreshka(dev.IP, dev.PortTCP)
                        { MAC = dev.MAC, Gateway = dev.Gateway, Mask = dev.Mask };
                    break;
                case MetalDetectorSeries.Impulse:
                    dev.Name = $"{dev.IP}:{dev.MAC}";
                    dev.ModelSeries = MetalDetectorSeries.BlockPost;
                    dev.DeviceMetalDetector = new Device.Impulse.Impulse(dev.IP, dev.PortTCP)
                        { MAC = dev.MAC, Gateway = dev.Gateway, Mask = dev.Mask };
                    break;
                case MetalDetectorSeries.Unknown:
                case MetalDetectorSeries.BlockPost:
                default:
                    dev.Name = $"Неизвестный detector {dev.IP}:{dev.MAC}";
                    dev.ModelSeries = MetalDetectorSeries.BlockPost;
                    break;
            }

            dev.dtLastInfFindMD = DateTime.Now;

            return dev.DeviceMetalDetector;
        }

    } //class MetalDetectPacketInfo
}
