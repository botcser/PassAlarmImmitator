using IRAPROM.MyCore.MyNetwork;
using Newtonsoft.Json;

namespace IRAPROM.MyCore.Device.Matreshka.XGOST
{
    public class Constants : FamilyInfo
    {
        public static readonly short[] FindAnswerCodes = { 0x1040 };
        public static byte[] RequestMagicNumber = { 0x40, 0x23, 0x24 }; // @#$
        public static byte[] ResponseMagicNumber = { 0x41, 0x59, 0x3E }; // AY>
        public static byte[] RequestMagicNumberMonopanel = { 0x5C, 0x15, 0xAE };
        public static byte[] FindDatagram = new byte[] { 0x40, 0x23, 0x24, 0xFF, 0xFF, 0x0E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x8B, 0x69, 0x3C, 0x5A, 0x72, 0xfB, 0x0A };
        
        public const int CommandRequestMetaLength = 22;
        public const int CommandResponseMetaLength = 20;
        public const int FindResponseLength = 56;
        public static int CommandOffset = 13;

        public static (short deviceCode, short code, int responseLenght, string name) GetNetworkParams = (0x0021, 0x0021, CommandRequestMetaLength + 16, "GetNetworkParams");
        public static (short deviceCode, short code, int responseLenght, string name) GetBaseSensitivity = (0x0022, 0x0022, CommandRequestMetaLength + 1, "GetBaseSensitivity");
        public static (short deviceCode, short code, int responseLenght, string name) GetZonesSensitivity3 = (0x023, 0x00233, CommandRequestMetaLength + 12, "GetZonesSensitivity3");
        public static (short deviceCode, short code, int responseLenght, string name) GetZonesSensitivity6 = (0x023, 0x00236, CommandRequestMetaLength + 24, "GetZonesSensitivity6");
        public static (short deviceCode, short code, int responseLenght, string name) GetZonesSensitivity11 = (0x023, 0x00239, CommandRequestMetaLength + 44, "GetZonesSensitivity11");
        public static (short deviceCode, short code, int responseLenght, string name) GetWorkFrequency = (0x0024, 0x0024, CommandRequestMetaLength + 1, "GetWorkFrequency");
        public static (short deviceCode, short code, int responseLenght, string name) GetZonesWorkMode = (0x0025, 0x0025, CommandRequestMetaLength + 3, "GetZonesWorkMode");
        public static (short deviceCode, short code, int responseLenght, string name) GetZonesWorkModeV33 = (0x0025, 0x00255, CommandRequestMetaLength + 4, "GetZonesWorkMode");
        public static (short deviceCode, short code, int responseLenght, string name) GetAlarmParams = (0x0026, 0x0026, CommandRequestMetaLength + 3, "GetAlarmParams");
        public static (short deviceCode, short code, int responseLenght, string name) GetTime = (0x0027, 0x0027, CommandRequestMetaLength + 7, "GetTime");
        public static (short deviceCode, short code, int responseLenght, string name) GetSerialNumber = (0x0028, 0x0028, CommandRequestMetaLength + 12, "GetSerialNumber");
        public static (short deviceCode, short code, int responseLenght, string name) GetPassageCount = (0x0029, 0x0029, CommandRequestMetaLength + 5, "GetPassageCount");
        public static (short deviceCode, short code, int responseLenght, string name) GetWorkProgramScene = (0x002A, 0x002A, CommandRequestMetaLength + 1, "GetWorkProgramScene");
        public static (short deviceCode, short code, int responseLenght, string name) GetAlarmLogs = (0x002B, 0x002B, 0, "GetAlarmLogs");

        public static (short deviceCode, short code, int responseLenght, string name) SetNetworkParams = (0x0001, 0x0001, CommandRequestMetaLength, "SetNetworkParams");
        public static (short deviceCode, short code, int responseLenght, string name) SetBaseSensitivity = (0x0002, 0x0002, CommandRequestMetaLength, "SetBaseSensitivity");
        public static (short deviceCode, short code, int responseLenght, string name) SetZonesSensitivity = (0x0003, 0x0003, CommandRequestMetaLength, "SetZonesSensitivity");
        public static (short deviceCode, short code, int responseLenght, string name) SetWorkFrequency = (0x0004, 0x0004, CommandRequestMetaLength, "SetWorkFrequency");
        public static (short deviceCode, short code, int responseLenght, string name) SetZonesWorkMode = (0x0005, 0x0005, CommandRequestMetaLength, "SetZonesWorkMode");
        public static (short deviceCode, short code, int responseLenght, string name) SetAlarmParams = (0x0006, 0x0006, CommandRequestMetaLength, "SetAlarmParams");
        public static (short deviceCode, short code, int responseLenght, string name) SetTime = (0x0007, 0x0007, CommandRequestMetaLength, "SetTime");
        public static (short deviceCode, short code, int responseLenght, string name) SetSerialNumber = (0x0008, 0x0008, CommandRequestMetaLength, "SetSerialNumber");
        public static (short deviceCode, short code, int responseLenght, string name) SetWorkProgramScene = (0x000A, 0x000A, CommandRequestMetaLength, "SetWorkProgramScene");
        public static (short deviceCode, short code, int responseLenght, string name) ClearPassageCount = (0x0009, 0x0009, CommandRequestMetaLength, "ClearPassageCount");
        public static (short deviceCode, short code, int responseLenght, string name) CallPassage = (0x41, 0x41, CommandRequestMetaLength, "CallPassage");
        public static (short deviceCode, short code, int responseLenght, string name) CallAlarm = (0x42, 0x42, CommandRequestMetaLength, "CallAlarm");
        
        public static Dictionary<string, (short ModelId, List<short> AvailableZonesCount, string Name, List<int> GridCellDefinitions, int RealCoilsCount)> Models = new Dictionary<string, (short ModelId, List<short> AvailableZonesCount, string Name, List<int>, int RealCoilsCount)>()
            {
                { PCV3300Name, (0x002A, new List <short>{ 11, 22, 33 }, PCV3300Name, new List<int> {11, 3}, 11 ) },
                { PCZ3300MKName, (0x0020, new List<short>{ 11, 22, 33 }, PCZ3300MKName, new List<int> {11, 3}, 6 )},
                { PCV900Name, (0x0028, new List<short>{ 3, 6, 9 }, PCV900Name, new List<int> {3, 3}, 6 )}, 
                { PCVx900Name, (0x0032, new List<short>{ 3, 6, 9 }, PCVx900Name, new List<int> {3, 3}, 6 ) }, 
                { PCV1800Name, (0x0029, new List<short>{ 6, 12, 18 }, PCV1800Name, new List<int> {6, 3}, 6 ) }, 
                { PCVx1800Name, (0x0033, new List<short>{ 6, 12, 18 }, PCVx1800Name, new List<int> {6, 3}, 6 ) }, 
                { MV6Name, (0x0064, new List <short>{ 6, 6, 6 }, MV6Name, new List<int> {6, 3}, 6 ) },                    // Монопанели передают режим ЗО = 2
                { MVx6Name, (0x0065, new List <short>{ 6, 6, 6 }, MVx6Name, new List<int> {6, 3}, 6 ) },
                { UnknownName, (0x00FE, new List <short>{ 6, 6, 6 }, UnknownName, new List<int> {6, 1}, 6 ) },
            };

        public const short PortTCPDefault = 5000;
        public const short PortUDPDefault = 1021;
        public const short PortUDPListenDefault = 1021;
        private short _portUDPListenAdditional = 0;
        private short _portUDPAdditional = 0;

        public override short PortTCP => 5000;
        public override short PortUDP => 1021;
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


        private const string PCZ3300MKName = "PC Z 3300 M K";
        private const string PCV900Name = "PC V 900 (9/6/3)";
        private const string PCVx900Name = "PC Vx 900 (9/6/3)";
        private const string PCV1800Name = "PC V 1800 (18/12/6)";
        private const string PCVx1800Name = "PC Vx 1800 (18/12/6)";
        private const string PCV3300Name = "PC V 3300 (33/22/11)";
        private const string MV6Name = "M V 6";
        private const string MVx6Name = "M Vx 6";
        private const string UnknownName = "Unknown Matreshka";

        public static List<(short, short, int, string)> GetCommands = new List<(short, short, int, string)>()
        {
            GetBaseSensitivity, GetWorkFrequency, GetAlarmParams, GetZonesWorkMode, GetPassageCount, GetNetworkParams, GetTime,
            GetSerialNumber, GetWorkProgramScene, GetAlarmLogs, GetZonesSensitivity11, GetZonesSensitivity6, GetZonesSensitivity3,
        };

        public static List<(short, short, int, string)> SetCommands = new List<(short, short, int, string)>()
        {
            SetZonesSensitivity, SetBaseSensitivity, SetWorkFrequency, SetAlarmParams, SetZonesWorkMode, SetNetworkParams, SetTime,
            SetSerialNumber, SetWorkProgramScene, ClearPassageCount, CallPassage, CallAlarm
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

            MZ6MK = 20,

            PCV900 = 40,
            PCV1800 = 41,
            PCZ3300MK = 0x20,
            PCV3300 = 0x2A,
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
                    return "PC V 3300 (33/22/11)";
                }
                case Model.PCZ3300MK:
                {
                    return "PC Z 3300 M K";
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
                case Model.UnknownMatreshka:
                default: return "Unknown Matreshka";
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
                PCZ3300MKName => Models[PCZ3300MKName].ModelId,
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

            sender.Send(FindDatagram, PortUDP, ip);

            if (PortUDPAdditional != 0) sender.Send(FindDatagram, PortUDPAdditional, ip);

            taskCompletionSource.SetResult();

            return taskCompletionSource.Task; 
        }

        public override string GetModelName(int id)
        {
            return GetModelName((Model)id);
        }
    }
}
