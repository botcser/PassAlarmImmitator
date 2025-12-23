using IRAPROM.MyCore.MyNetwork;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Device.Impulse
{
    public class Constants : FamilyInfo
    {
        public static int DatagramMetaInfoLength = 5;       // первые 5 байт + CS
        public static int ChecksumLength = 1;
        public static int AfterZonesSensitivityBytesCountSuppose = 13;
        public static int ZonesSensitivityStartIndex = 9;
        public static byte[] FindDatagram = { 0x5B, 0xaa, 0x40 };

        public static (short code, int responseLenght, string name) GetWorkParams = (0xA1, 123, "GetWorkParams");
        public static (short code, int responseLenght, string name) GetPassageCountD = (0xAD, DatagramMetaInfoLength + ChecksumLength, "GetPassageCountD");
        public static (short code, int responseLenght, string name) GetPassageCountE = (0xAE, DatagramMetaInfoLength + ChecksumLength, "GetPassageCountE");

        public static (short code, int responseLenght, string name) SetNetworkParams = (0xC1, DatagramMetaInfoLength + ChecksumLength, "SetNetworkParams");
        public static (short code, int responseLenght, string name) SetWorkParams = (0xA5, DatagramMetaInfoLength + ChecksumLength, "SetWorkParams");
        public static (short code, int responseLenght, string name) SetWorkProgramScene = (0x14, DatagramMetaInfoLength + ChecksumLength, "SetWorkScene");
        public static (short code, int responseLenght, string name) ClearPassageCount = (0xA7, DatagramMetaInfoLength + ChecksumLength, "ClearPassageCount");

        public static Dictionary<string, (short ModelId, List<short> AvailableZonesCount, string Name, List<int> GridCellDefinitions, int RealCoilsCount)> Models = new Dictionary<string, (short ModelId, List<short> AvailableZonesCount, string Name, List<int>, int RealCoilsCount)>()
        {
            { PC600MKName, (0x0004, new List<short>{ 6 }, PC600MKName, new List<int> {3, 2}, 6) },
            { PC1800MKName, (0x0002, new List<short>{ 18, 12, 6 }, PC1800MKName, new List < int > { 6, 3 }, 6) },
            { PC4400MKName, (0x0001, new List<short>{ 33, 22, 11 }, PC4400MKName, new List < int > { 11, 3 }, 11) },
            { PC3300MName, (0x0006, new List<short>{ 33, 22, 11 }, PC3300MName, new List < int > { 11, 3 }, 11) },
            { UnknownName, (0x00ff, new List <short>{ 6 }, UnknownName, new List < int > { 6, 1 }, 6) },
        };      
        
        public static List<(short, int, string)> GetCommands = new List<(short, int, string)>()
        {
            GetWorkParams, GetPassageCountD, GetPassageCountE
        };

        public static List<(short, int, string)> SetCommands = new List<(short, int, string)>()
        {
            SetWorkParams,ClearPassageCount,SetWorkProgramScene
        }; 
        
        public enum Model
        {
            Unknown = -1,

            z400 = 1111,
            x400 = 2222,

            z600 = 11111,
            x600 = 12222,
            z1200 = 21,
            x1200 = 22,

            z1800 = 31,
            x1800 = 32,

            PC600MKZ = 0,   //6-зонник
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

        public override short PortTCP => 5012;
        public override short PortUDP => 5015;
        public override short PortUDPListen => 5016;
        [JsonIgnore]
        public override List<string> WorkPrograms => _workPrograms;

        private static readonly List<string> _workPrograms = new List<string>() {
            "МЧС",
            "Склад",
            "Ювелирная",
            "Тех. Помещение",
            "Спец бюро",
            "Офисы",
            "Комната отдыха",
            "Клубы",
            "Библиотека",
            "Радио",
            "Телевидение",
            "Метеостанция",
            "Пост",
            "КПП 1",
            "Военная база",
            "Посольство",
            "Электростанции",
            "Гостиница",
            "Бассейны",
            "Бюро пропусков",
            "Блок-пост",
            "КПП 2",
            "Диспансер",
            "Комната Экзамена",
            "Суды",
            "Автокомбинат",
            "Банк",
            "Хранилище",
            "СИЗО",
            "Тюрьма",
            "Прокуратура",
            "Таможня",
            "33",
            "34" };
        
        private const string PC600MKName = "PC 600MK (6)";
        private const string PC1800MKName = "PC 1800MK (18/12/6)";
        private const string PC4400MKName = "PC 4400MK (33/22/11)";
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
        
        public static List<string> GetAllModelsNames()
        {
            return Models.Keys.ToList();
        }

        public override List<string> GetAllModels()
        {
            return Models.Keys.ToList();
        }

        public override Task Find(string ip, IUDPSend sender)
        {
            var taskCompletionSource = new TaskCompletionSource();

            sender.Send(FindDatagram, PortUDP, ip, taskCompletionSource);

            return taskCompletionSource.Task;
        }

        public override int GetModelId(string val)                                 // Update MetalDetectorModelFromName
        {
            return val switch
            {
                PC600MKName => Models[PC600MKName].ModelId,
                PC1800MKName => Models[PC1800MKName].ModelId,
                PC4400MKName => Models[PC4400MKName].ModelId,
                PC3300MName => Models[PC3300MName].ModelId,
                _ => -1
            };
        }
        
        public override string GetModelName(int id)
        {
            return GetModelName((Model)id);
        }
    }
}
