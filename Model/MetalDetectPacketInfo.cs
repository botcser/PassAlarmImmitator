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
using IRAPROM.MyCore.Model.WP;
using IRAPROM.MyCore.MyNetwork;
//using Npgsql;

namespace IRAPROM.MyCore.Model
{
    public class MetalDetectPacketInfo : MDSaveInfo
    {

        public short IdModel = 0; //Добавил недавно
        public string ProductModel = "";

        public byte[] head = new byte[4];
        public ushort hardwareAdress;
        public byte result;
        public int frameLen;
        public int msgNum;
        public short command;
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
        public uint NormalPassNum = 0;
        public uint NormalReturnNum = 0;
        public uint AlarmPassNum = 0;
        public uint AlarmReturnNum = 0;

        //public decimal? Temperature = null;
        //public string Explosives = "";
        //public short? Radiation = null;
        
        public string NormalInfName => $"{NormalPassNum}/R{NormalReturnNum}  A{AlarmPassNum}/AR{AlarmReturnNum} ";



        ///// for FindAnswer
        public DeviceFindAnswerNetworkInf deviceFindAnswerNetworkInf = null;

        //extra info from TCP/IP components
        public string Ip = "";
        public string Mask = "";
        public string Mac = "";
        public string Gateway = "";
        public ushort TCPPort;
        public ushort UDPPort;

        ushort port = 0;
        string DestCpuIP = "";
        short DestCpuPort = 0;     // this computer's UDP port


        public MetalDetectPacketInfo()
        {
            MetDetector = new MetDetector();
        }

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
                    rec.hardwareAdress = rec.head[3];
                    
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
                        rec.Ip = rec.MetDetector.IP = devInf.IP;
                        rec.Mac = rec.MetDetector.MAC = devInf.MAC;
                        rec.Gateway = rec.MetDetector.Gateway = devInf.IPGateway;
                        rec.Mask = rec.MetDetector.Mask = devInf.Mask;
                        rec.UDPPort = rec.MetDetector.PortUDP = devInf.PortUDP;
                        rec.TCPPort = rec.MetDetector.PortTCP = devInf.PortTCP;

                        return rec;
                    }
                }
            }

            using (var ms = new MemoryStream(rec.body))
            {
                using (var br = new BinaryReader(ms))
                {
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

                    if (MyARM.Instance.AddedDevicesTryGetValue(MAC, out md, out var onChanged))
                    {
                        rec.IdModel = modelId = md.ModelId;
                        rec.MetDetector = md;
                    }

                    switch (rec.command)
                    {
                        case MDCommands.METDET_CMD_ALARM:
                            rec.timeStamp = br.ReadBytes(7); //с 21 байта
                            rec.ZonesSensorMode = br.ReadByte(); //28 байт
                            rec.sensors = br.ReadBytes(18); //с 29 байта    // 6 байт у PC Z 3300 MK
                            
                            if (md != null)
                            {
                                md.DeviceMetalDetector.LastPassage = new MetalDetectorPassage(MAC, rec.sensors, rec.logTime, rec.ZonesSensorMode);
                                onChanged(md);
                            }
                            break;

                        case MDCommands.METDET_CMD_NORMAL_GET_PASSAGES:
                            rec.NormalPassNum = br.ReadUInt32(); // с 21 байта
                            rec.NormalReturnNum = br.ReadUInt32(); //с 25 байта
                            rec.AlarmPassNum = br.ReadUInt32(); //с 29 байта
                            rec.AlarmReturnNum = br.ReadUInt32(); //с 33 байта

                            if (md != null)
                            {
                                md.DeviceMetalDetector.LastPassage = new MetalDetectorPassage(MAC, rec.logTime, rec.ZonesSensorMode, rec.NormalPassNum, rec.AlarmPassNum, rec.NormalReturnNum, rec.AlarmReturnNum);
                                onChanged(md);
                            }
                            break;

                        default:
                            break;
                    }
                }

            }

            return rec;
        }

        public static MetalDetectPacketInfo ParseXGOSTMatreshkaMessageUDP(byte[] arr, IPEndPoint ipEndPoint)
        {
            var rec = new MetalDetectPacketInfo();

            try
            {
                if (!CheckMatreshkaHeader(arr)) return null;

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
                    rec.IdModel = rec.MetDetector.ModelId = (short)deviceMetalDetector.ModelId;
                    rec.ProductModel = rec.MetDetector.Name = deviceMetalDetector.ProductModelName;
                    rec.UDPPort = rec.MetDetector.PortUDP = deviceMetalDetector.PortUDP;
                    rec.TCPPort = rec.MetDetector.PortTCP = deviceMetalDetector.PortTCP;
                    rec.command = (short)commandCode;

                    return rec;
                }

                if (arr.Length == Device.Matreshka.XGOST.Constants.AlarmResponseLength)
                {
                    using (var ms = new MemoryStream(arr))
                    {
                        using (var br = new BinaryReader(ms))
                        {
                            var odd = br.ReadBytes(Device.Matreshka.XGOST.Constants.CommandCodeOffset);

                            rec.command = (short)br.ReadUInt16();

                            var someTime = br.ReadByte();
                            var someDate = br.ReadBytes(6);

                            //Console.WriteLine($"\n\todd {Convert.ToHexString(odd)}" +
                            //                  $"\n\tcommand {rec.command}" +
                            //                  $"\n\tsomeTime {someTime}" +
                            //                  $"\n\tsomeDate {Convert.ToHexString(someDate)}");

                            var enterPassagesCount = br.ReadUInt32();
                            var exitPassagesCount = br.ReadUInt32();
                            var enterAlarmCount = br.ReadUInt32();
                            var exitAlarmCount = br.ReadUInt32();
                            var alarmZoneMode = rec.ZonesSensorMode = br.ReadByte();
                            var infraredPassCounterMode = (byte)(br.ReadByte() & 0x0F);
                            var metalQuantity = br.ReadUInt16();
                            var sensors = rec.sensors = br.ReadBytes(8);
                            var mac = br.ReadBytes(6);

                            var md = MyARM.Instance.ShowAddedDevices().FirstOrDefault(i => i.Value.MAC == Convert.ToHexString(mac));
                            MetDetector metDetector = null;
                            Action<MetDetector> metDetectorOnChanged = null;

                            if (md.Value != null)
                            {
                                MyARM.Instance.AddedDevicesTryGetValue(md.Value.MAC, out metDetector, out metDetectorOnChanged);
                                rec.IdModel = md.Value.ModelId;
                                rec.MetDetector = md.Value;
                                rec.Ip = md.Value.IP;
                                rec.Mac = md.Value.MAC;
                                rec.IdModel = metDetector.ModelId;
                                rec.ProductModel = metDetector.Name;
                            }

                            Console.WriteLine($"ParseXGOSTMatreshkaMessageUDP:\n\tsensors {Convert.ToHexString(rec.sensors)} " +
                                              $"\n\tEnterPassagesCount {enterPassagesCount}" +
                                              $"\n\tExitPassagesCount {exitPassagesCount}" +
                                              $"\n\tEnterAlarmCount {enterAlarmCount}" +
                                              $"\n\tExitAlarmCount {exitAlarmCount}" +
                                              $"\n\tInfraredPassCounterMode {infraredPassCounterMode}" +
                                              $"\n\tAlarmZoneMode {alarmZoneMode}" +
                                              $"\n\tmetalQuantity {metalQuantity}" +
                                              $"\n\tsensors {Convert.ToHexString(sensors)}" +
                                              $"\n\tmac {Convert.ToHexString(mac)}");

                            if (metDetector != null)
                            {
                                metDetector.AlarmZoneMode = metDetector.DeviceMetalDetector.ZonesCount = metDetector.DeviceMetalDetector.WorkParams.ZonesSensorMode = alarmZoneMode;
                                metDetector.InfraredPassCounterMode = metDetector.DeviceMetalDetector.WorkParams.InfraredPassCounterMode = infraredPassCounterMode;

                                if (sensors.Any(i => i > 0))
                                {
                                    metDetector.DeviceMetalDetector.LastPassage = new MetalDetectorPassage(rec.Mac, rec.sensors, rec.logTime, alarmZoneMode)
                                    {
                                        EnterPassagesCount = enterPassagesCount,
                                        ExitPassagesCount = exitPassagesCount,
                                        EnterAlarmCount = enterAlarmCount,
                                        ExitAlarmCount = exitAlarmCount,
                                    };
                                }
                                else
                                {
                                    metDetector.DeviceMetalDetector.LastPassage = new MetalDetectorPassage(rec.Mac, rec.logTime, alarmZoneMode, enterPassagesCount, enterAlarmCount, exitPassagesCount, exitAlarmCount);
                                }

                                metDetector.DeviceMetalDetector.WorkParams.ForwardPassageCount = enterPassagesCount;
                                metDetector.DeviceMetalDetector.WorkParams.ForwardAlarmsCount = enterAlarmCount;
                                metDetector.DeviceMetalDetector.WorkParams.BackwardPassageCount = exitPassagesCount;
                                metDetector.DeviceMetalDetector.WorkParams.BackwardAlarmsCount = exitAlarmCount;

                                metDetectorOnChanged?.Invoke(metDetector);
                            }
                        }
                    }

                    return rec;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"ParseXGOSTMatreshkaMessageUDP: EX: {e.Message}");
            }

            return rec;
        }

        public static DeviceMetalDetector MakeMetalDeviceFromPacketInfo(MetalDetectPacketInfo rec, MetalDetectorSeries series)
        {
            MetDetector dev = null;

            //if (MyARM.Instance.AddedDevicesTryGetValue(rec.MAC, out var dev, out var onChanged))
            //{
            //    if (dev.dtLastInfFindMD == default)
            //    {
            //        dev.dtLastInfFindMD = DateTime.Now;
            //        dev.FindNetworkStatus = 1;
            //    }
            //    else
            //    {
            //        dev.FindNetworkStatus = 1;
            //        dev.dtLastInfFindMD = DateTime.Now;

            //        if (dev.dtLastInfFindMD.AddSeconds(10) < DateTime.Now)  //Информация с прошлых запросов
            //        {
            //            //Что-то делаем
            //        }
            //        else //Информация уже найдена на предыдущих срабатываниях таймера текущего поиска - просто меняем время
            //        {
            //        }
            //    }

            //    onChanged(dev);

            //    return dev.DeviceMetalDetector;
            //}

            //dev = MyARM.Instance.DevicesFound.FirstOrDefault(x => x.MAC == rec.MAC);

            if (dev == null)
            {
                if (rec.MetDetector == null)
                {
                    dev = new MetDetector
                    {
                        MAC = rec.MAC,
                        IP = rec.deviceFindAnswerNetworkInf.IP,
                        Mask = rec.deviceFindAnswerNetworkInf.Mask,
                        PortTCP = rec.deviceFindAnswerNetworkInf.PortTCP,
                        PortUDP = rec.deviceFindAnswerNetworkInf.PortUDP,
                        Gateway = rec.deviceFindAnswerNetworkInf.IPGateway
                    };
                }
                else
                {
                    dev = rec.MetDetector;
                }

                if (rec.deviceFindAnswerNetworkInf.Model == MDModel.cMZ6MK)
                {
                    dev.ModelId = (short)MetalDetectorModel.PCVx9300_MZ6MK;
                    
                    //if (MyTools.ExistValue(rec.deviceFindAnswerNetworkInf.Version))
                    //{
                    //    dev.Version = rec.deviceFindAnswerNetworkInf.Version;
                    //}
                }

                switch (series)
                {
                    case MetalDetectorSeries.Matryoshka:
                        dev.Name ??= $"{dev.IP}:{dev.MAC}";
                        dev.ModelSeries = MetalDetectorSeries.Matryoshka;
                        dev.DeviceMetalDetector ??= new Device.Matreshka.Matreshka(dev.IP, dev.PortTCP) { MAC = dev.MAC, Gateway = dev.Gateway, Mask = dev.Mask };
                        break;
                    case MetalDetectorSeries.Impulse:
                        dev.Name ??= $"{dev.IP}:{dev.MAC}";
                        dev.ModelSeries = MetalDetectorSeries.BlockPost;
                        dev.DeviceMetalDetector ??= new Device.Impulse.Impulse(dev.IP, dev.PortTCP) { MAC = dev.MAC, Gateway = dev.Gateway, Mask = dev.Mask };

                        break;
                    case MetalDetectorSeries.Unknown:
                    case MetalDetectorSeries.BlockPost:
                    default:
                        dev.Name = $"Неизвестный detector {dev.IP}:{dev.MAC}";
                        dev.ModelSeries = MetalDetectorSeries.BlockPost;
                        break;
                }

                dev.dtLastInfFindMD = DateTime.Now;

                //MyARM.Instance.DevicesFound.Add(dev);
            }
            else
                dev.dtLastInfFindMD = DateTime.Now;

            return dev.DeviceMetalDetector;
        }

    } //class MetalDetectPacketInfo
}
