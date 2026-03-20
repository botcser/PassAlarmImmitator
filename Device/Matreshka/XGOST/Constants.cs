using IRAPROM.MyCore.Model;
using IRAPROM.MyCore.MyNetwork;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IRAPROM.MyCore.Device.Matreshka.XGOST
{
    public class Constants : FamilyInfo
    {
        public static readonly short[] FindAnswerCodes = { 0x1040 };
        public static byte[] RequestMagicNumber = { 0x40, 0x23, 0x24 }; // @#$
        public static byte[] ResponseMagicNumber = { 0x41, 0x59, 0x3E }; // AY>
        public static byte[] RequestMagicNumberMonopanel = { 0x5C, 0x15, 0xAE };
        public static byte[] FindDatagram = new byte[] { 0x40, 0x23, 0x24, 0xFF, 0xFF, 0x0E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x8B, 0x69, 0x3C, 0x5A, 0xcb, 0xd7, 0x0D, 0x0A };

        public const int CommandResponseLength = 56;
        public const int ResultOffset = 15;
        public const int DataLengthOffset = 5;
        public const int FrameSequenceOffset = 9;
        public const int MetaInfoLength = (3 + 2 + 4) + PacketMetaInfoLength;      //23   // Start frame marker + Hardware address + Frame packet length + PacketMetaInfoLength
        public const int PacketMetaInfoLength = 4 + 2 + 4 + 2 + 2;                      // Frame number + Command + Password + CRC checksum + End frame marker
        public const int CommandCodeOffset = 13;
        public const int DataOffset = 19;
        public const int ResponseMetaInfoLength = (3 + 2 + 4) + 4 + 2 + 1 + 2 + 2;


        public static (short deviceCode, short code, int responseLenght, string name) GetPassword = (0x0087, 0x0087, MetaInfoLength + 7, "GetPassword");
        public static (short deviceCode, short code, int responseLenght, string name) GetFirmwareVersion = (0x0089, 0x0089, MetaInfoLength + 12, "GetFirmwareVersion");
        public static (short deviceCode, short code, int responseLenght, string name) GetSerialNumber = (0x008A, 0x008A, MetaInfoLength + 16, "GetSerialNumber");
        public static (short deviceCode, short code, int responseLenght, string name) GetTime = (0x0088, 0x0088, MetaInfoLength + 6, "GetTime");
        public static (short deviceCode, short code, int responseLenght, string name) GetPassageCount = (0x008B, 0x008B, MetaInfoLength + 16, "GetPassageCount");

        public static (short deviceCode, short code, int responseLenght, string name) GetNetworkParams = (0x0081, 0x0081, MetaInfoLength + 22, "GetNetworkParams");
        public static (short deviceCode, short code, int responseLenght, string name) GetBaseSensitivity = (0x0082, 0x0082, MetaInfoLength + 2, "GetBaseSensitivity");
        public static (short deviceCode, short code, int responseLenght, string name) GetZonesSensitivity = (0x083, 0x0083, MetaInfoLength + 12, "GetZonesSensitivity");
        public static (short deviceCode, short code, int responseLenght, string name) GetZonesSensitivity33 = (0x083, 0x00833, MetaInfoLength + 12, "GetZonesSensitivity33");
        public static (short deviceCode, short code, int responseLenght, string name) GetWorkFrequency = (0x0084, 0x0084, MetaInfoLength + 1, "GetWorkFrequency");
        public static (short deviceCode, short code, int responseLenght, string name) GetZonesWorkMode = (0x0085, 0x0085, MetaInfoLength + 3, "GetZonesWorkMode");
        public static (short deviceCode, short code, int responseLenght, string name) GetZonesWorkModeV33 = (0x0025, 0x00255, MetaInfoLength + 4, "GetZonesWorkMode");
        public static (short deviceCode, short code, int responseLenght, string name) GetAlarmParams = (0x0086, 0x0086, MetaInfoLength + 3, "GetAlarmParams");
        public static (short deviceCode, short code, int responseLenght, string name) GetWorkProgramScene = (0x002A, 0x002A, MetaInfoLength + 1, "GetWorkProgramScene");

        public static (short deviceCode, short code, int responseLenght, string name) SetNetworkParams = (0x0001, 0x0001, ResponseMetaInfoLength, "SetNetworkParams");
        public static (short deviceCode, short code, int responseLenght, string name) SetBaseSensitivity = (0x0002, 0x0002, ResponseMetaInfoLength, "SetBaseSensitivity");
        public static (short deviceCode, short code, int responseLenght, string name) SetZonesSensitivity = (0x0003, 0x0003, ResponseMetaInfoLength, "SetZonesSensitivity");
        public static (short deviceCode, short code, int responseLenght, string name) SetWorkFrequency = (0x0004, 0x0004, ResponseMetaInfoLength, "SetWorkFrequency");
        public static (short deviceCode, short code, int responseLenght, string name) SetWorkProgramScene = (0x0005, 0x0005, ResponseMetaInfoLength, "SetWorkProgramScene");
        public static (short deviceCode, short code, int responseLenght, string name) SetAlarmParams = (0x0006, 0x0006, ResponseMetaInfoLength, "SetAlarmParams");
        public static (short deviceCode, short code, int responseLenght, string name) SetTime = (0x0008, 0x0008, ResponseMetaInfoLength, "SetTime");
        public static (short deviceCode, short code, int responseLenght, string name) SetSerialNumber = (0x0008, 0x0008, ResponseMetaInfoLength, "SetSerialNumber");
        public static (short deviceCode, short code, int responseLenght, string name) SetWorkProgramSceneHZ = (0x000A, 0x000A, ResponseMetaInfoLength, "SetWorkProgramSceneHZ"); //???
        public static (short deviceCode, short code, int responseLenght, string name) SetPassword = (0x0007, 0x0007, ResponseMetaInfoLength, "SetPassword");
        public static (short deviceCode, short code, int responseLenght, string name) ClearPassageCount = (0x0040, 0x0040, ResponseMetaInfoLength, "ClearPassageCount");
        public static (short deviceCode, short code, int responseLenght, string name) ResetSettings = (0x41, 0x41, ResponseMetaInfoLength, "ResetSettings");
        public static (short deviceCode, short code, int responseLenght, string name) SimulatePass = (0x42, 0x42, ResponseMetaInfoLength, "SimulatePass");
        public static (short deviceCode, short code, int responseLenght, string name) ResetDevice = (0x43, 0x43, ResponseMetaInfoLength, "ResetDevice");

        public static Dictionary<string, (short ModelId, List<short> AvailableZonesCount, string Name, List<int> GridCellDefinitions, int RealCoilsCount)> Models = new Dictionary<string, (short ModelId, List<short> AvailableZonesCount, string Name, List<int>, int RealCoilsCount)>()
            {
                { PCX600PROName, (0x006E, new List <short>{ 3, 6, 9 }, PCX600PROName, new List<int> {3, 1}, 6 ) },
                { PCX1100PROName, (0x006F, new List <short>{ 11, 22, 33 }, PCX1100PROName, new List<int> {11, 3}, 11 ) },
                { PCGOST900Name, (0x0028, new List <short>{ 3, 6, 9 }, PCGOST900Name, new List<int> {3, 3}, 6 ) },
                { PCGOST1800Name, (0x0029, new List <short>{ 6, 12, 18 }, PCGOST1800Name, new List<int> {6, 3}, 6 ) },
                { PCGOST3300Name, (0x003E, new List <short>{ 11, 22, 33 }, PCGOST3300Name, new List<int> {11, 3}, 11 ) },
                { PCGOST6300Name, (0x0040, new List <short>{ 21, 42, 63 }, PCGOST6300Name, new List<int> {33, 3}, 11 ) },
                { PCGOSTx900Name, (0x0032, new List <short>{ 3, 6, 9 }, PCGOSTx900Name, new List<int> {3, 3}, 6 ) },
                { PCGOSTx1800Name, (0x0033, new List <short>{ 6, 12, 18 }, PCGOSTx1800Name, new List<int> {6, 3}, 6 ) },
                { PCGOSTx3300Name, (0x0048, new List <short>{ 11, 22, 33 }, PCGOSTx3300Name, new List<int> {11, 3}, 11 ) },
                { PCGOSTx6300Name, (0x004A, new List <short>{ 21, 42, 63 }, PCGOSTx6300Name, new List<int> {33, 3}, 11 ) },
                { MGOST6Name, (0x0064, new List <short>{ 3, 6, 9 }, MGOST6Name, new List<int> {3, 1}, 6 ) },
            };

        public const short PortTCPDefault = 5000;
        public const short PortUDPDefault = 1021;
        public const short PortUDPListenDefault = 1021;
        private short _portUDPListenAdditional = 0;
        private short _portUDPAdditional = 0;

        public override ushort PortTCP => 5000;
        public override ushort PortUDP => 1021;
        public override short PortUDPAdditional
        {
            get => _portUDPAdditional;
            set
            {
                if (value == 0) return;

                _portUDPAdditional = value;
            }
        }
        public override short PortUDPListen => 1021;
        public override short PortUDPListenAdditional
        {
            get => _portUDPListenAdditional;
            set
            {
                if (value == 0) return;

                _portUDPListenAdditional = value;
            }
        }

        [JsonIgnore]
        public override List<string> WorkPrograms => _workPrograms;


        private const string PCX600PROName = "PC X 600 PRO";
        private const string PCX1100PROName = "PC X 1100 PRO";
        private const string PCGOST900Name = "PC V 900 GOST";
        private const string PCGOST1800Name = "PC V 1800 GOST";
        private const string PCGOST3300Name = "PC V 3300 GOST";
        private const string PCGOST6300Name = "PC V 6300 GOST";
        private const string PCGOSTx900Name = "PC Vx 900 GOST";
        private const string PCGOSTx1800Name = "PC Vx 1800 GOST";
        private const string PCGOSTx3300Name = "PC Vx 3300 GOST";
        private const string PCGOSTx6300Name = "PC Vx 6300 GOST";
        private const string MGOST6Name = "M V 6 GOST";
        private const string UnknownName = "Unknown GOST Matreshka";

        public static List<(short, short, int, string)> GetCommands = new List<(short, short, int, string)>()
        {
            GetBaseSensitivity, GetWorkFrequency, GetAlarmParams, GetZonesWorkMode, GetPassageCount, GetNetworkParams, GetTime,
            GetSerialNumber, GetWorkProgramScene, GetPassword, GetZonesSensitivity, GetFirmwareVersion, GetSerialNumber,
        };

        public static List<(short, short, int, string)> SetCommands = new List<(short, short, int, string)>()
        {
            SetZonesSensitivity, SetBaseSensitivity, SetWorkFrequency, SetAlarmParams, SetNetworkParams, SetTime, SetPassword,
            SetSerialNumber, SetWorkProgramScene, ClearPassageCount, ResetDevice, ResetSettings, SimulatePass
        }; 

        private static readonly List<string> _workPrograms = new List<string>() {
            "1 МЧС",
            "2 Склад",
            "3 Ювелирная",
            "4 Тех. Помещение",
            "5 Спец бюро",
            "6 Офисы",
            "7 Комната отдыха",
            "8 Клубы",
            "9 Библиотека",
            "10 Радио",
            "11 Телевидение",
            "12 Метеостанция",
            "13 Пост",
            "14 КПП 1",
            "15 Военная база",
            "16 Посольство",
            "17 Электростанции",
            "18 Гостиница",
            "19 Бассейны",
            "20 Бюро пропусков",
            "21 Блок-пост",
            "22 КПП 2",
            "23 Диспансер",
            "24 Комната Экзамена",
            "25 Суды",
            "26 Автокомбинат",
            "27 Банк",
            "28 Хранилище",
            "29 СИЗО",
            "30 Тюрьма",
            "31 Прокуратура",
            "32 Таможня",
            "33 Правительство",
            "34 Аэропорт",
            "35 Ж/д станция",
            "36 Ж/д Вокзал",
            "37 Автостанция",
            "38 Пристань",
            "39 Трудовой лагерь",
            "40 Типография",
            "41 Фабрика",
            "42 Завод",
            "43 Производство",
            "44 Шахта",
            "45 Склад",
            "46 НИИ",
            "47 Архивы",
            "48 Музей",
            "49 Спец. Комната",
            "50 Стадион",
            "51 Парк",
            "52 Центр отдыха",
            "53 Концерт",
            "54 Клуб",
            "55 Бар",
            "56 Торговый центр",
            "57 Выставочный центр",
            "58 Станция метро",
            "59 Военный городок",
            "60 Театр, Кинотеатр",
            "61 Школа",
            "62 Лаборатория",
            "63 Галерея искусств",
            "64 Бункер",
            "65 Космодром",
            "66 Ангар",
            "67 Полигон",
            "68 Спец. Пункт",
            "69 Граница",
            "70 Роддом",
            "71 Клиника",
            "72 Спортзал", };

        public enum Model
        {
            UnknownMatreshka = 0xFE,

            PCX600PRO = 0x6E,
            PCX1100PRO = 0x6F,

            PCGOST900 = 0x28,
            PCGOST1800 = 0x29,
            PCGOST3300 = 0x3E,
            PCGOST6300 = 0x40,

            PCGOSTx900 = 0x32,
            PCGOSTx1800 = 0x33,
            PCGOSTx3300 = 0x48,
            PCGOSTx6300 = 0x4A,

            MGOST6 = 0x64,
        }

        public static string GetModelName(Model id)                                 // Update MetalDetectorModelFromName
        {
            switch (id)
            {
                case Model.PCX600PRO:
                {
                    return PCX600PROName;
                }
                case Model.PCX1100PRO:
                {
                    return PCX1100PROName;
                }
                case Model.PCGOST900:
                {
                    return PCGOST900Name;
                }
                case Model.PCGOST1800:
                {
                    return PCGOST1800Name;
                }
                case Model.PCGOST3300:
                {
                    return PCGOST3300Name;
                }
                case Model.PCGOST6300:
                {
                    return PCGOST6300Name;
                }
                case Model.PCGOSTx900:
                {
                    return PCGOSTx900Name;
                }
                case Model.PCGOSTx1800:
                {
                    return PCGOSTx1800Name;
                }
                case Model.PCGOSTx3300:
                {
                    return PCGOSTx3300Name;
                }
                case Model.PCGOSTx6300:
                {
                    return PCGOSTx6300Name;
                }
                case Model.MGOST6:
                {
                    return MGOST6Name;
                }
                case Model.UnknownMatreshka:
                default: return "Unknown GOST Matreshka";
            }
        }
        
        public static List<string> GetAllModelsNames()
        {
            return Models.Keys.ToList();
        }

        public override List<string> GetAllModels()
        {
            return Models.Keys.ToList();
        }

        public override int GetModelId(string name)                                 // Update MetalDetectorModelFromName
        {
            return name switch
            {
                PCX600PROName => Models[PCX600PROName].ModelId,
                PCX1100PROName => Models[PCX1100PROName].ModelId,
                PCGOST900Name => Models[PCGOST900Name].ModelId,
                PCGOST1800Name => Models[PCGOST1800Name].ModelId,
                PCGOST3300Name => Models[PCGOST3300Name].ModelId,
                PCGOST6300Name => Models[PCGOST6300Name].ModelId,
                PCGOSTx900Name => Models[PCGOSTx900Name].ModelId,
                PCGOSTx1800Name => Models[PCGOSTx1800Name].ModelId,
                PCGOSTx3300Name => Models[PCGOSTx3300Name].ModelId,
                PCGOSTx6300Name => Models[PCGOSTx6300Name].ModelId,
                MGOST6Name => Models[MGOST6Name].ModelId,
                _ => -1
            };
        }

        public override Task Find(string ip, IUDPSend sender)
        {
            var taskCompletionSource = new TaskCompletionSource();

            sender.Send(FindDatagram, PortUDP, ip);

            if (PortUDPAdditional != 0) sender.Send(FindDatagram, PortUDPAdditional, ip);

            taskCompletionSource.SetResult();

            return taskCompletionSource.Task; 
        }

        public override DeviceMetalDetector ParseFindCommandResponse(byte[] bytes, out ushort commandCode)
        {
            XPROGOST result;

            using (var ms = new MemoryStream(bytes))
            {
                using (var br = new BinaryReader(ms))
                {
                    var magicNumberHead = br.ReadBytes(3); //4
                    var hardwareAddress = BitConverter.ToUInt16(br.ReadBytes(2));
                    var frameDataLength = br.ReadBytes(4);
                    var frameSequenceNumber = br.ReadBytes(4);

                    commandCode = br.ReadUInt16();

                    var commandResult = br.ReadByte();

                    var ip = new IPAddress((uint)IPAddress.HostToNetworkOrder(int.Parse(Convert.ToHexString(br.ReadBytes(4)), NumberStyles.HexNumber))).ToString();
                    var mask = new IPAddress((uint)IPAddress.HostToNetworkOrder(int.Parse(Convert.ToHexString(br.ReadBytes(4)), NumberStyles.HexNumber))).ToString();
                    var gateway = new IPAddress((uint)IPAddress.HostToNetworkOrder(int.Parse(Convert.ToHexString(br.ReadBytes(4)), NumberStyles.HexNumber))).ToString();
                    var portTcp = BitConverter.ToUInt16(br.ReadBytes(2));
                    var portUdp = BitConverter.ToUInt16(br.ReadBytes(2));
                    var mac = Convert.ToHexString(br.ReadBytes(6));

                    var deviceMetalDetector = new XPROGOST(ip, portTcp, hardwareAddress) { Mask = mask, Gateway = gateway, PortUDP = portUdp, MAC = mac,
                        ProductModelName = Encoding.UTF8.GetString(br.ReadBytes(10))
                    };

                    var modelId = br.ReadBytes(4).Select(i => (char)i).ToList();

                    deviceMetalDetector.ModelId = (ushort)(Convert.FromHexString($"{modelId[0]}{modelId[1]}")[0]/* + Convert.FromHexString($"{modelId[2]}{modelId[3]}")[0] * 0x100*/);
                    deviceMetalDetector.Model = (Model)deviceMetalDetector.ModelId;

                    result = deviceMetalDetector;
                }
            }

            return result;
        }

        public override string GetModelName(int id)
        {
            return GetModelName((Model)id);
        }
    }
}
