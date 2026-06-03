using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IRAPROM.MyCore.MyNetwork;
using Newtonsoft.Json;

namespace IRAPROM.MyCore.Device.Impulse
{
    public class Constants : FamilyInfo
    {
        public static int DatagramMetaInfoLength = 5;       // первые 5 байт + CS
        public static int ChecksumLength = 1;
        public static int AfterZonesSensitivityBytesCountSuppose = 13;
        public static int ZonesSensitivityStartIndex = 9;
        public static byte[] FindDatagram = { 0x5B, 0xaa, 0x40 };
        public static byte[] HeaderMagicNumber = { 0x5C, 0x17, 0xAE };
        public static byte[] HeaderMagicNumberOld = { 0x5C, 0x15, 0xAE };

        public static (short deviceCode, short code, int responseLenght, string name) GetWorkParams = (0xA1, 0xA1, 123, "GetWorkParams");
        public static (short deviceCode, short code, int responseLenght, string name) GetPassageCountD = (0xAD, 0xAD, DatagramMetaInfoLength + ChecksumLength, "GetPassageCountD");
        public static (short deviceCode, short code, int responseLenght, string name) GetPassageCountE = (0xAE, 0xAE, DatagramMetaInfoLength + ChecksumLength, "GetPassageCountE");

        public static (short deviceCode, short code, int responseLenght, string name) SetNetworkParams = (0xC1, 0xC1, DatagramMetaInfoLength + ChecksumLength, "SetNetworkParams");
        public static (short deviceCode, short code, int responseLenght, string name) SetWorkParams = (0xA5, 0xA5, 0, "SetWorkParams");                         // PC1800MK 43.0 не посылает ответ
        public static (short deviceCode, short code, int responseLenght, string name) SetWorkProgramScene = (0x14, 0x14, 0, "SetWorkScene");                    // PC1800MK 43.0 не посылает ответ
        public static (short deviceCode, short code, int responseLenght, string name) ClearPassageCount = (0xA7, 0xA7, 0, "ClearPassageCount");                 
        public static (short deviceCode, short code, int responseLenght, string name) CallPassage = (0xAE, 0xAE, DatagramMetaInfoLength + ChecksumLength, "CallPassage");
        public static (short deviceCode, short code, int responseLenght, string name) CallAlarm = (0xAEE, 0xAE, DatagramMetaInfoLength + ChecksumLength, "CallAlarm");

        public override Dictionary<ushort, MetalDetectorAttrs> Models
        {
            get;
        } = new Dictionary<ushort, MetalDetectorAttrs>()
        {
            { 0x0004, new MetalDetectorAttrs(0x0004, PC600MKName, new List<short>{ 6 }, new List<int> {6, 1}, 6) },
            { 0x0002, new MetalDetectorAttrs(0x0002, PC1800MKName, new List<short>{ 18, 12, 6 }, new List < int > { 6, 3 }, 6) },
            { 0x0001, new MetalDetectorAttrs(0x0001, PC4400MKName, new List<short>{ 33, 22, 11 }, new List < int > { 11, 3 }, 11) },
            { 0x0006, new MetalDetectorAttrs(0x0006, PC6300MKName, new List<short>{ 33, 22, 11 }, new List < int > { 11, 3 }, 11) },
            { 0x00ff, new MetalDetectorAttrs(0x00ff, UnknownName, new List <short>{ 6 }, new List < int > { 6, 1 }, 6) },
        };

        public static List<(short, short, int, string)> GetCommands = new List<(short, short, int, string)>()
        {
            GetWorkParams, GetPassageCountD, GetPassageCountE
        };

        public static List<(short, short, int, string)> SetCommands = new List<(short, short, int, string)>()
        {
            SetWorkParams,ClearPassageCount,SetWorkProgramScene
        }; 
        
        public enum Model
        {
            Unknown = 0,

            z400 = 1111,
            x400 = 2222,

            z600 = 11111,
            x600 = 12222,
            z1200 = 21,
            x1200 = 22,

            z1800 = 31,
            x1800 = 32,

            PC600MKZ = 8,   //6-зонник
            PC600MKX = 4,   //6-зонник
            PC1800MKZ = 2,  //18-зонник
            PC1800MKX = 3,   //18-зонник
            PC4400MKZ = 1,  //33-зонник
            PC4400MKX = 5,  //33-зонник
            PC3300M = 10,  //33-зонник
            PC6300MKZ = 6,  //63-зонник
            PC6300MKX = 7,  //63-зонник
        }

        public static string[] ZoneMode1800 = new[] { "18", "12", "6" };
        public static string[] ZoneMode4400 = new[] { "33", "22", "11" };
        public static string[] ZoneMode3300 = new[] { "33", "22", "11" };
        public static string[] ZoneMode600 = new[] { "6" };

        public const short PortTCPDefault = 5012;
        public const short PortUDPDefault = 5015;
        public const short PortUDPListenDefault = 5016;
        private short _portUDPListenAdditional;

        public override ushort PortTCP => 5012;
        public override ushort PortUDP => 5015;
        public override short PortUDPAdditional { get; set; }
        public override short PortUDPListen => 5016;

        public override short PortUDPListenAdditional
        {
            get => _portUDPListenAdditional; 
            set => _portUDPListenAdditional = value;
        }

        [JsonIgnore]
        public override List<string> WorkPrograms => _workPrograms;

        private static readonly List<string> _workPrograms = new List<string>() {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "30",
            "31а",
            "32",
            "33",
            "34" };

        private const string PC600MKName = "PC 600MK (6)";
        private const string PC1800MKName = "PC 1800MK (18/12/6)";
        private const string PC4400MKName = "PC 4400MK (33/22/11)";
        private const string PC6300MKName = "PC 6300MK (63/42/21)";
        private const string PC3300MName = "PC-3300M";
        private const string UnknownName = "UnknownImpulse";

        public static string GetModelName(Model val)                 // Update MetalDetectorModelFromName
        {
            switch (val)
            {
                case Model.PC600MKZ:
                {
                    return PC600MKName;
                }
                case Model.z400:
                {
                    return "PC Z 400 MK";
                }
                case Model.x400:
                {
                    return "PC X 400 MK";
                }
                case Model.z600:
                {
                    return "PC Z 600 MK";
                }
                case Model.x600:
                {
                    return "PC X 600 MK";
                }
                case Model.z1200:
                {
                    return "PC Z 1200 MK";
                }
                case Model.x1200:
                {
                    return "PC X 1200 MK";
                }
                case Model.z1800:
                {
                    return "PC Z 1800 MK";
                }
                case Model.x1800:
                {
                    return "PC X 1800 MK";
                }
                case Model.PC600MKX:
                {
                    return PC600MKName;
                }
                case Model.PC1800MKZ:
                {
                    return PC1800MKName; //"PC 1800MK (18/12/6)";
                }
                case Model.PC1800MKX:
                {
                    return PC1800MKName;
                }
                case Model.PC4400MKZ:
                {
                    return PC4400MKName; //"PC 4400MK (33/22/11) (Z)";
                }
                case Model.PC4400MKX:
                {
                    return PC4400MKName;
                }
                case Model.PC6300MKZ:
                {
                    return "PC 6300MK (63/42/21) (Z)";
                }
                case Model.PC6300MKX:
                {
                    return "PC 6300MK (63/42/21) (X)";
                }
                case Model.PC3300M:
                {
                    return PC3300MName;
                }
                default:
                    return "Unknown";
            }
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
            throw new System.NotImplementedException(); // TODO sometime in future
        }

        public static bool CheckImpulseHeader(byte[] arr)
        {
            if ((arr[0] == Device.Impulse.Constants.HeaderMagicNumber[0]) && (arr[1] == Device.Impulse.Constants.HeaderMagicNumber[1]) && (arr[2] == Device.Impulse.Constants.HeaderMagicNumber[2]))
                return true;

            if ((arr[0] == Device.Impulse.Constants.HeaderMagicNumberOld[0]) && (arr[1] == Device.Impulse.Constants.HeaderMagicNumberOld[1]) && (arr[2] == Device.Impulse.Constants.HeaderMagicNumberOld[2]))
                return true;

            return false;
        }

        public override Dictionary<int, string> InfraModesList { get; } = new Dictionary<int, string>() { { 1, "Неактивный" }, { 2, "Статистика" }, { 3, "Вход-Выход" }, { 4, "Калькулятор" } };
    }
}
