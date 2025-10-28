using IRAPROM.MyCore.MyNetwork;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.Matreshka
{
    public class Constants : FamilyInfo
    {
        public static byte[] RequestMagicNumber = { 0x40, 0x23, 0x24 }; // @#$
        public static byte[] ResponseMagicNumber = { 0x41, 0x50, 0x3E }; // AY>
        public static byte[] RequestMagicNumberMonopanel = { 0x5C, 0x15, 0xAE };
        public static byte[] FindDatagram = new byte[] { 0x40, 0x23, 0x24, 0x00, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x30, 0x30, 0x0D, 0x0A };
        public static int MetaInfoLength = 19;
        
        public static (short code, int responseLenght, string name) GetNetworkParams = (0x0021, MetaInfoLength + 16, "GetNetworkParams");
        public static (short code, int responseLenght, string name) GetBaseSensitivity = (0x0022, MetaInfoLength + 1, "GetBaseSensitivity");
        public static (short code, int responseLenght, string name) GetZonesSensitivity3 = (0x0023, MetaInfoLength + 12, "GetZonesSensitivity3");
        public static (short code, int responseLenght, string name) GetZonesSensitivity6 = (0x0023, MetaInfoLength + 24, "GetZonesSensitivity6");
        public static (short code, int responseLenght, string name) GetZonesSensitivity11 = (0x0023, MetaInfoLength + 44, "GetZonesSensitivity11");
        public static (short code, int responseLenght, string name) GetWorkFrequency = (0x0024, MetaInfoLength + 1, "GetWorkFrequency");
        public static (short code, int responseLenght, string name) GetZonesWorkMode = (0x0025, MetaInfoLength + 3, "GetZonesWorkMode");
        public static (short code, int responseLenght, string name) GetZonesWorkModeV33 = (0x0025, MetaInfoLength + 4, "GetZonesWorkMode");
        public static (short code, int responseLenght, string name) GetAlarmParams = (0x0026, MetaInfoLength + 3, "GetAlarmParams");
        public static (short code, int responseLenght, string name) GetTime = (0x0027, MetaInfoLength + 7, "GetTime");
        public static (short code, int responseLenght, string name) GetSerialNumber = (0x0028, MetaInfoLength + 12, "GetSerialNumber");
        public static (short code, int responseLenght, string name) GetPassageCount = (0x0029, MetaInfoLength + 5, "GetPassageCount");
        public static (short code, int responseLenght, string name) GetWorkProgramScene = (0x002A, MetaInfoLength + 1, "GetWorkProgramScene");
        public static (short code, int responseLenght, string name) GetAlarmLogs = (0x002B, 0, "GetAlarmLogs");

        public static (short code, int responseLenght, string name) SetNetworkParams = (0x0001, MetaInfoLength, "SetNetworkParams");
        public static (short code, int responseLenght, string name) SetBaseSensitivity = (0x0002, MetaInfoLength, "SetBaseSensitivity");
        public static (short code, int responseLenght, string name) SetZonesSensitivity = (0x0003, MetaInfoLength, "SetZonesSensitivity");
        public static (short code, int responseLenght, string name) SetWorkFrequency = (0x0004, MetaInfoLength, "SetWorkFrequency");
        public static (short code, int responseLenght, string name) SetZonesWorkMode = (0x0005, MetaInfoLength, "SetZonesWorkMode");
        public static (short code, int responseLenght, string name) SetAlarmParams = (0x0006, MetaInfoLength, "SetAlarmParams");
        public static (short code, int responseLenght, string name) SetTime = (0x0007, MetaInfoLength, "SetTime");
        public static (short code, int responseLenght, string name) SetSerialNumber = (0x0008, MetaInfoLength, "SetSerialNumber");
        public static (short code, int responseLenght, string name) ClearPassageCount = (0x0009, MetaInfoLength, "ClearPassageCount");
        public static (short code, int responseLenght, string name) SetWorkProgramScene = (0x000A, MetaInfoLength, "SetWorkProgramScene");

        public static Dictionary<string, (short ModelId, List<short> AvailableZonesCount, string Name)> Models = new Dictionary<string, (short ModelId, List<short> AvailableZonesCount, string Name)>()
            {
                { PCV900Name, (0x0028, new List<short>{ 3, 6, 9 }, PCV900Name) }, 
                { PCVx900Name, (0x0032, new List<short>{ 3, 6, 9 }, PCVx900Name) }, 
                { PCV1800Name, (0x0029, new List<short>{ 6, 12, 18 }, PCV1800Name) }, 
                { PCVx1800Name, (0x0033, new List<short>{ 6, 12, 18 }, PCVx1800Name) }, 
                { PCV3300Name, (0x003E, new List <short>{ 11, 22, 33 }, PCV3300Name) }, 
                { MV6Name, (0x0064, new List <short>{ 6 }, MV6Name) }, 
                { MVx6Name, (0x0065, new List <short>{ 6 }, MVx6Name) },
                { UnknownName, (0x00ff, new List <short>{ 6 }, UnknownName) },
            };

        public const short PortTCPDefault = 5000;
        public const short PortUDPDefault = 9998;
        public const short PortUDPListenDefault = 9999;

        public override short PortTCP => 5000;
        public override short PortUDP => 9998;
        public override short PortUDPListen => 9999;
        public override List<string> WorkPrograms => _workPrograms;

        private const string PCV900Name = "PC V 900 (9/6/3)";
        private const string PCVx900Name = "PC Vx 900 (9/6/3)";
        private const string PCV1800Name = "PC V 1800 (18/12/6)";
        private const string PCVx1800Name = "PC Vx 1800 (18/12/6)";
        private const string PCV3300Name = "PC V 3300 (33|22|11)";
        private const string MV6Name = "M V 6";
        private const string MVx6Name = "M Vx 6";
        private const string UnknownName = "Unknown";

        public static List<(short, int, string)> GetCommands = new List<(short, int, string)>()
        {
            GetBaseSensitivity, GetWorkFrequency, GetAlarmParams, GetZonesWorkMode, GetPassageCount, GetNetworkParams, GetTime,
            GetSerialNumber, GetWorkProgramScene, GetAlarmLogs, GetZonesSensitivity11, GetZonesSensitivity6, GetZonesSensitivity3,
        };

        public static List<(short, int, string)> SetCommands = new List<(short, int, string)>()
        {
            SetZonesSensitivity, SetBaseSensitivity, SetWorkFrequency, SetAlarmParams, SetZonesWorkMode, SetNetworkParams, SetTime,
            SetSerialNumber, SetWorkProgramScene, ClearPassageCount
        }; 

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
            "Правительство",
            "Аэропорт",
            "Ж/д станция",
            "Ж/д Вокзал",
            "Автостанция",
            "Пристань",
            "Трудовой лагерь",
            "Типография",
            "Фабрика",
            "Завод",
            "Производство",
            "Шахта",
            "Склад",
            "НИИ",
            "Архивы",
            "Музей",
            "Спец. Комната",
            "Стадион",
            "Парк",
            "Центр отдыха",
            "Концерт",
            "Клуб",
            "Бар",
            "Торговый центр",
            "Выставочный центр",
            "Станция метро",
            "Военный городок",
            "Театр, Кинотеатр",
            "Школа",
            "Лаборатория",
            "Галерея искусств",
            "Бункер",
            "Космодром",
            "Ангар",
            "Полигон",
            "Спец. Пункт",
            "Граница",
            "Роддом",
            "Клиника",
            "Спортзал", };

        public enum Model
        {
            Unknown = -1,

            MZ6MK = 20,

            PCV900 = 40,
            PCV1800 = 41,
            PCV3300 = 42,
            PCV4800 = 43,
            PCV6300 = 44,
            PCV9300 = 45,
            PCVx900 = 50,
            PCVx1800 = 51,
            PCVx3300 = 52,
            PCVx4800_MZmb = 53,
            PCVx6300_MZmb = 54,
            PCVx9300 = 55,
            PCV90011 = 60,
            PCV180011 = 61,
            PCV3300M11 = 62,
            PCV480011 = 63,
            PCV630011_MV6mb = 64,
            PCV930011 = 65,
            PCVx90011 = 70,
            PCVx180011 = 71,
            PCVx330011 = 72,
            PCVx480011 = 73,
            PCVx630011 = 74,
            PCVx930011 = 75,
            PCV480016_PCVi1800mb = 80,
            PCV630016 = 81,
            PCV930016 = 82,
            PCVx480016_PCVi3300mb = 90,
            PCVx630016 = 91,
            PCVx930016 = 92,

            MV6 = 100,
            MVx6 = 101,
            MV11_hz = 110,
            MVx11_hz = 111,
            MV16_hz = 120,
            MVx16_hz = 121,
        }
        
        public static string GetModelName(Model id)                                 // Update MetalDetectorModelFromName
        {
            switch (id)
            {
                case Model.PCV1800:
                {
                    return PCV1800Name;
                }
                case Model.PCVx900:
                {
                    return PCVx900Name;
                }
                case Model.PCVx9300:
                {
                    return "PC Vx 9300 (93/62/31) 6 (Монопанель MZ 6 MK)";
                }
                case Model.PCV6300:
                {
                    return "PC V 6300 (63/42/21) 6 (Монопанель)";
                }
                case Model.PCV9300:
                {
                    return "PC V 9300 (93/62/31) 6";
                }
                case Model.PCV480016_PCVi1800mb:
                {
                    return "PC V 4800 (48/32/16) (PCVi1800 (18/12/6))";
                }
                case Model.PCVx480016_PCVi3300mb:
                {
                    return "PC Vx 4800 (48/32/16) (PCVi3300 (33/22/11))";
                }
                case Model.MV6:
                {
                    return MV6Name;
                }
                case Model.MZ6MK:
                {
                    return "M Z 6 MK Монопанель";
                }
                case Model.PCV900:
                {
                    return PCV900Name;
                }
                case Model.PCV3300:
                {
                    return "PC V 3300 (33/22/11) 6";
                }
                case Model.PCV4800:
                {
                    return "PC V 4800 (48/32/16) 6";
                }
                case Model.PCVx1800:
                {
                    return PCVx1800Name;
                }
                case Model.PCVx3300:
                {
                    return "PC Vx 3300 (33/22/11) 6";
                }
                case Model.PCVx4800_MZmb:
                {
                    return "PC Vx 4800 (48/32/16) 6 (MZ Монопанель)";
                }
                case Model.PCVx6300_MZmb:
                {
                    return "PC Vx 6300 (63/42/21) 6 (MZ Монопанель)";
                }
                case Model.PCV90011:
                {
                    return "PC V 900 (9/6/3) 11";
                }
                case Model.PCV180011:
                {
                    return "PC V 1800 (18/12/6) 11";
                }
                case Model.PCV3300M11:
                {
                    return PCV3300Name;
                }
                case Model.PCV480011:
                {
                    return "PC V 4800 (48/32/16) 11";
                }
                case Model.PCV630011_MV6mb:
                {
                    return "PC V 6300 (63/42/21) 11 (MV6 Монопанель)";
                }
                case Model.PCV930011:
                {
                    return "PC V 9300 (93/62/31) 11";
                }
                case Model.PCVx90011:
                {
                    return "PC Vx 900 (9/6/3) 11";
                }
                case Model.PCVx180011:
                {
                    return "PC Vx 1800 (18/12/6) 11";
                }
                case Model.PCVx330011:
                {
                    return "PC Vx 3300 (33/22/11) 11";
                }
                case Model.PCVx480011:
                {
                    return "PC Vx 4800 (18/12/6) 11";
                }
                case Model.PCVx630011:
                {
                    return "PC Vx 6300 (63/42/21) 11";
                }
                case Model.PCVx930011:
                {
                    return "PC Vx 9300 (93/62/31) 11";
                }
                case Model.PCV630016:
                {
                    return "PC V 6300 (63/42/21) 16";
                }
                case Model.PCV930016:
                {
                    return "PC V 9300 (93/62/31) 16";
                }
                case Model.PCVx630016:
                {
                    return "PC Vx 6300 (63/42/21) 16";
                }
                case Model.PCVx930016:
                {
                    return "PC Vx 9300 (93/62/31) 16";
                }
                case Model.MVx6:
                {
                    return MVx6Name;
                }
                case Model.MV11_hz:
                {
                    return "M V 11 new";
                }
                case Model.MVx11_hz:
                {
                    return "M Vx 11 new";
                }
                case Model.MV16_hz:
                {
                    return "M V 16 new";
                }
                case Model.MVx16_hz:
                {
                    return "M Vx 16 new";
                }
                case Model.Unknown:
                default: return "Unknown";
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
                PCV900Name => Models[PCV900Name].ModelId,
                PCVx900Name => Models[PCVx900Name].ModelId,
                PCV1800Name => Models[PCV1800Name].ModelId,
                PCVx1800Name => Models[PCVx1800Name].ModelId,
                PCV3300Name => Models[PCV3300Name].ModelId,
                MV6Name => Models[MV6Name].ModelId,
                MVx6Name => Models[MVx6Name].ModelId,
                _ => -1
            };
        }

        public override Task Find(string ip, IUDPSend sender)
        {
            var taskCompletionSource = new TaskCompletionSource();

            sender.Send(FindDatagram, PortUDP, ip, taskCompletionSource);

            return taskCompletionSource.Task; 
        }

        public override string GetModelName(int id)
        {
            return GetModelName((Model)id);
        }
    }
}
