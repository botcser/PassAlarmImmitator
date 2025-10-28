using System;
using System.Collections.Generic;

namespace IRAPROM.MyCore.Model.MD
{
    public enum MetalDetectorModel
    {
        Unknown = -1,

        z400 = 1111,
        x400 = 2222,

        z600 = 11111,
        x600 = 12222,
        MZ6MK = 20,
        z1200 = 21,
        x1200 = 22,

        z1800 = 31,
        x1800 = 32,

        //МАТРЕШКА
        PCV900 = 40,
        PCV1800 = 41,
        PCV33006 = 42,
        PCV4800 = 43,
        PCV6300 = 44,
        PCV9300 = 45,
        PCVx900 = 50,
        PCVx1800 = 51,
        PCVx3300 = 52,
        PCVx4800_MZmb = 53,
        PCVx6300_MZmb = 54,
        PCVx9300_MZ6MK = 55,
        PCV90011 = 60,
        PCV180011 = 61,
        PCV3300 = 62,
        PCV480011 = 63,
        PCV630011_MV6mb  = 64,
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

        /*
        Corresponding code of MV 6 MK ------------- 0x0C
        of PC V 10 0 MK ------------ 0x0D
        of PC V 30 0 MK ------------ 0x0E
        of PC V 4 00 MK ------------ 0x0F
        of PC V 6 00 MK ------------ 0x10
        of PC V 18 00 MK ------------ 0x11
        of PC V 3300 MK ------------ 0x12
        of PC V 4800 MK ------------ 0x13
        of PC V 6300 MK ------------ 0x14
        of PC V 9300 MK ------------ 0x15
        */


        //ИМПУЛЬС
        PC600MKZ = 0,  //6-зонник
        PC600MKX = 1,  //6-зонник
        PC1800MKZ = 2,  //18-зонник
        PC4400MK = 3,  //18-зонник
        PC4400MKZ = 4,  //33-зонник
        PC4400MKX = 5,  //33-зонник
        PC6300MKZ = 6,  //63-зонник
        PC6300MKX = 7,  //63-зонник
    }


    public static class MDModel
    {
        public const string cMZ6MK = "MZ6MK";//Передается новой монопанелью в ответе на поиск металлодетекторов



        public static string GetItemName(MetalDetectorModel val)
        {
            switch (val)
            {
                case MetalDetectorModel.PC600MKZ:
                {
                    return "PC 600MK (Z)";
                }
                case MetalDetectorModel.z400:
                {
                    return "PC Z 400 MK";
                }
                case MetalDetectorModel.x400:
                {
                    return "PC X 400 MK";
                }
                case MetalDetectorModel.z600:
                {
                    return "PC Z 600 MK";
                }
                case MetalDetectorModel.x600:
                {
                    return "PC X 600 MK";
                }
                case MetalDetectorModel.z1200:
                {
                    return "PC Z 1200 MK";
                }
                case MetalDetectorModel.x1200:
                {
                    return "PC X 1200 MK";
                }
                case MetalDetectorModel.z1800:
                {
                    return "PC Z 1800 MK";
                }
                case MetalDetectorModel.x1800:
                {
                    return "PC X 1800 MK";
                }
                case MetalDetectorModel.PCV1800:
                {
                    return "PC V 1800 (18/12/6)";
                }
                case MetalDetectorModel.PCVx900:
                {
                    return "PC Vx 900 (18/12/6)";
                }
                case MetalDetectorModel.PCVx9300_MZ6MK:
                {
                    return "PC Vx 9300 (93/62/31) 6 (Монопанель MZ 6 MK)";
                }
                case MetalDetectorModel.PCV6300:
                {
                    return "PC V 6300 (63/42/21) 6 (Монопанель)";
                }
                case MetalDetectorModel.PCV9300:
                {
                    return "PC V 9300 (93/62/31) 6";
                }
                case MetalDetectorModel.PCV480016_PCVi1800mb:
                {
                    return "PC V 4800 (48/32/16) (PCVi1800 (18/12/6))";
                }
                case MetalDetectorModel.PCVx480016_PCVi3300mb:
                {
                    return "PC Vx 4800 (48/32/16) (PCVi3300 (33/22/11))";
                }
                case MetalDetectorModel.MV6:
                {
                    return "M V 6 Монопанель";
                }
                case MetalDetectorModel.PC600MKX:
                {
                    return "PC 600MK (X)";
                }
                case MetalDetectorModel.PC1800MKZ:
                {
                    return "PC 1800MK (Z)";
                }
                case MetalDetectorModel.PC4400MK:
                {
                    return "PC 1800MK (X)";
                }
                case MetalDetectorModel.MZ6MK:
                    return "M Z 6 MK Монопанель";
                case MetalDetectorModel.PCV900:
                    return "PC V 900 (9/6/3)";
                case MetalDetectorModel.PCV33006:
                    return "PC V 3300 (33/22/11) 6";
                case MetalDetectorModel.PCV4800:
                    return "PC V 4800 (48/32/16) 6";
                case MetalDetectorModel.PCVx1800:
                    return "PC Vx 1800 (18/12/6)";
                case MetalDetectorModel.PCVx3300:
                    return "PC Vx 3300 (33/22/11) 6";
                case MetalDetectorModel.PCVx4800_MZmb:
                    return "PC Vx 4800 (48/32/16) 6 (MZ Монопанель)";
                case MetalDetectorModel.PCVx6300_MZmb:
                    return "PC Vx 6300 (63/42/21) 6 (MZ Монопанель)";
                case MetalDetectorModel.PCV90011:
                    return "PC V 900 (9/6/3) 11";
                case MetalDetectorModel.PCV180011:
                    return "PC V 1800 (18/12/6) 11";
                case MetalDetectorModel.PCV3300:
                    return "PC V 3300 (33/22/11)";
                case MetalDetectorModel.PCV480011:
                    return "PC V 4800 (48/32/16) 11";
                case MetalDetectorModel.PCV630011_MV6mb:
                    return "PC V 6300 (63/42/21) 11 (MV6 Монопанель)";
                case MetalDetectorModel.PCV930011:
                    return "PC V 9300 (93/62/31) 11";
                case MetalDetectorModel.PCVx90011:
                    return "PC Vx 900 (9/6/3) 11";
                case MetalDetectorModel.PCVx180011:
                    return "PC Vx 1800 (18/12/6) 11";
                case MetalDetectorModel.PCVx330011:
                    return "PC Vx 3300 (33/22/11) 11";
                case MetalDetectorModel.PCVx480011:
                    return "PC Vx 4800 (18/12/6) 11";
                case MetalDetectorModel.PCVx630011:
                    return "PC Vx 6300 (63/42/21) 11";
                case MetalDetectorModel.PCVx930011:
                    return "PC Vx 9300 (93/62/31) 11";
                case MetalDetectorModel.PCV630016:
                    return "PC V 6300 (63/42/21) 16";
                case MetalDetectorModel.PCV930016:
                    return "PC V 9300 (93/62/31) 16";
                case MetalDetectorModel.PCVx630016:
                    return "PC Vx 6300 (63/42/21) 16";
                case MetalDetectorModel.PCVx930016:
                    return "PC Vx 9300 (93/62/31) 16";
                case MetalDetectorModel.MVx6:
                    return "M Vx 6";
                case MetalDetectorModel.MV11_hz:
                    return "M V 11 new";
                case MetalDetectorModel.MVx11_hz:
                    return "M Vx 11 new";
                case MetalDetectorModel.MV16_hz:
                    return "M V 16 new";
                case MetalDetectorModel.MVx16_hz:
                    return "M Vx 16 new";
                case MetalDetectorModel.PC4400MKZ:
                    return "PC 4400MK (33/22/11) (Z)";
                case MetalDetectorModel.PC4400MKX:
                    return "PC 4400MK (33/22/11) (X)";
                case MetalDetectorModel.PC6300MKZ:
                    return "PC 6300MK (63/42/21) (Z)";
                case MetalDetectorModel.PC6300MKX:
                    return "PC 6300MK (63/42/21) (X)";
                case MetalDetectorModel.Unknown:
                default: return "Unknown";
            }
        }

        public static bool IsMonopanel(MetalDetectorModel val)
        {
            switch (val)
            {
                case MetalDetectorModel.PCVx900:
                case MetalDetectorModel.PCVx9300_MZ6MK:
                case MetalDetectorModel.PCV6300:
                    return true;

                default:
                    return false;
            }

        }
        public static bool IsMonopanel(short sVal)
        {
            var val = (MetalDetectorModel)sVal;
            bool isDefined = Enum.IsDefined(typeof(MetalDetectorModel), val);
            if (!isDefined) return false;

            return IsMonopanel(val);
        }


        public static short MaxZonesCount(MetalDetectorModel val)
        {
            switch (val)
            {
                case MetalDetectorModel.z400: return 4;
                case MetalDetectorModel.x400: return 4;
                case MetalDetectorModel.z600: return 6;
                case MetalDetectorModel.x600: return 6;
                case MetalDetectorModel.z1200: return 12;
                case MetalDetectorModel.x1200: return 12;
                case MetalDetectorModel.z1800: return 18;
                case MetalDetectorModel.x1800: return 18;


                case MetalDetectorModel.PC600MKX: return 6; //Импульсник
                case MetalDetectorModel.PC1800MKZ: return 18; //Импульсник
                case MetalDetectorModel.PC4400MK: return 18;//33;

                case MetalDetectorModel.PCVx900: return 6;
                case MetalDetectorModel.PCV1800: return 18;//6;
                case MetalDetectorModel.PCV480016_PCVi1800mb: return 18;
                case MetalDetectorModel.PCVx480016_PCVi3300mb: return 18;//33;
                case MetalDetectorModel.PCV6300: return 63;//6;
                case MetalDetectorModel.PCV9300: return 93;//9;  
                case MetalDetectorModel.PCVx9300_MZ6MK: return 93;//6;

                case MetalDetectorModel.MV6: return 9;//48;


                case MetalDetectorModel.MZ6MK:
                case MetalDetectorModel.MVx6:
                case MetalDetectorModel.PC600MKZ:
                    return 6;
                case MetalDetectorModel.PCV900:
                case MetalDetectorModel.PCV90011:
                case MetalDetectorModel.PCVx90011:
                    return 9;
                case MetalDetectorModel.PCV180011:
                case MetalDetectorModel.PCVx1800:
                case MetalDetectorModel.PCVx180011:
                    return 18;
                case MetalDetectorModel.PCV33006:
                case MetalDetectorModel.PCV3300:
                case MetalDetectorModel.PCVx330011:
                case MetalDetectorModel.PCVx3300:
                case MetalDetectorModel.MV11_hz:
                case MetalDetectorModel.MVx11_hz:
                case MetalDetectorModel.PC4400MKZ:
                case MetalDetectorModel.PC4400MKX:
                    return 33;
                case MetalDetectorModel.PCV4800:
                case MetalDetectorModel.PCV480011:
                case MetalDetectorModel.PCVx4800_MZmb:
                case MetalDetectorModel.PCVx480011:
                case MetalDetectorModel.MV16_hz:
                case MetalDetectorModel.MVx16_hz:
                    return 48;
                case MetalDetectorModel.PCV630011_MV6mb:
                case MetalDetectorModel.PCV630016:
                case MetalDetectorModel.PCVx630011:
                case MetalDetectorModel.PCVx630016:
                case MetalDetectorModel.PCVx6300_MZmb:
                case MetalDetectorModel.PC6300MKZ:
                case MetalDetectorModel.PC6300MKX:
                    return 63;
                case MetalDetectorModel.PCV930011:
                case MetalDetectorModel.PCV930016:
                case MetalDetectorModel.PCVx930011:
                case MetalDetectorModel.PCVx930016:
                    return 93;

                case MetalDetectorModel.Unknown:
                default: return 0;
            }
        }
        public static short ZonesCount(short sVal)
        {
            var val = (MetalDetectorModel)sVal;
            bool isDefined = Enum.IsDefined(typeof(MetalDetectorModel), val);
            if (!isDefined) return 0;

            return MaxZonesCount(val);
        }



        public static string GetItemName(short modelId)
        {
            var val = (MetalDetectorModel)modelId;
            var isDefined = Enum.IsDefined(typeof(MetalDetectorModel), val);

            return !isDefined ? "???" : GetItemName(val);
        }

        public static MetalDetectorModel MetalDetectorModelFromName(string name)
        {
            switch (name)
            {
                case "PC 600MK (Z)":
                {
                    return (MetalDetectorModel)Device.Impulse.Constants.Model.PC600MKZ;
                }
                case "PC Z 400 MK":
                {
                    return (MetalDetectorModel)Device.Impulse.Constants.Model.z400;
                }
                case "PC X 400 MK":
                {
                    return (MetalDetectorModel)Device.Impulse.Constants.Model.x400;
                }
                case "PC Z 600 MK":
                {
                    return (MetalDetectorModel)Device.Impulse.Constants.Model.z600;
                }
                case "PC X 600 MK":
                {
                    return (MetalDetectorModel)Device.Impulse.Constants.Model.x600;
                }
                case "PC Z 1200 MK":
                {
                    return (MetalDetectorModel)Device.Impulse.Constants.Model.z1200;
                }
                case "PC X 1200 MK":
                {
                    return (MetalDetectorModel)Device.Impulse.Constants.Model.x1200;
                }
                case "PC Z 1800 MK":
                {   
                    return (MetalDetectorModel)Device.Impulse.Constants.Model.z1800;
                }
                case "PC X 1800 MK":
                {
                    return (MetalDetectorModel)Device.Impulse.Constants.Model.x1800;
                }
                case "PC 600MK (X)":
                {
                    return (MetalDetectorModel)Device.Impulse.Constants.Model.PC600MKX;
                }
                case "PC 1800MK (18|12|6)":
                {
                    return (MetalDetectorModel)Device.Impulse.Constants.Model.PC1800MKZ;
                }
                case "PC 1800MK (X)":
                {
                    return (MetalDetectorModel)Device.Impulse.Constants.Model.PC1800MKX;
                }
                case "PC 4400MK (33|22|11)":
                {
                    return (MetalDetectorModel)Device.Impulse.Constants.Model.PC4400MKZ;
                }
                case "PC 4400MK (33/22/11) (X)":
                {
                    return (MetalDetectorModel)Device.Impulse.Constants.Model.PC4400MKX;
                }
                case "PC 6300MK (63/42/21) (Z)":
                {
                    return (MetalDetectorModel)Device.Impulse.Constants.Model.PC6300MKZ;
                }
                case "PC 6300MK (63/42/21) (X)":
                {
                    return (MetalDetectorModel)Device.Impulse.Constants.Model.PC6300MKX;
                }
                case "PC V 1800 (18/12/6)":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCV1800;
                }
                case "PC Vx 900 (18/12/6) (Монопанель)":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCVx900;
                }
                case "PC Vx 9300 (93/62/31) 6 (Монопанель MZ 6 MK)":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCVx9300;
                }
                case "PC V 6300 (63/42/21) 6 (Монопанель)":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCV6300;
                }
                case "PC V 9300 (93/62/31) 6":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCV9300;
                }
                case "PC V 4800 (48/32/16) (PCVi1800 (18/12/6))":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCV480016_PCVi1800mb;
                }
                case "PC Vx 4800 (48/32/16) (PCVi3300 (33/22/11))":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCVx480016_PCVi3300mb;
                }
                case "M V 6":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.MV6;
                }
                case "M Z 6 MK Монопанель":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.MZ6MK;
                }
                case "PC V 900 (9/6/3)":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCV900;
                }
                case "PC V 3300 (33/22/11) 6":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCV3300;
                }
                case "PC V 4800 (48/32/16) 6":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCV4800;
                }
                case "PC Vx 1800 (18/12/6)":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCVx1800;
                }
                case "PC Vx 3300 (33/22/11) 6":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCVx3300;
                }
                case "PC Vx 4800 (48/32/16) 6 (MZ Монопанель)":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCVx4800_MZmb;
                }
                case "PC Vx 6300 (63/42/21) 6 (MZ Монопанель)":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCVx6300_MZmb;
                }
                case "PC V 900 (9/6/3) 11":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCV90011;
                }
                case "PC V 1800 (18/12/6) 11":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCV180011;
                }
                case "PC V 3300 (33/22/11)":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCV3300;
                }
                case "PC V 4800 (48/32/16) 11":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCV480011;
                }
                case "PC V 6300 (63/42/21) 11 (MV6 Монопанель)":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCV630011_MV6mb;
                }
                case "PC V 9300 (93/62/31) 11":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCV930011;
                }
                case "PC Vx 900 (9/6/3) 11":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCVx90011;
                }
                case "PC Vx 1800 (18/12/6) 11":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCVx180011;
                }
                case "PC Vx 3300 (33/22/11) 11":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCVx330011;
                }
                case "PC Vx 4800 (18/12/6) 11":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCVx480011;
                }
                case "PC Vx 6300 (63/42/21) 11":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCVx630011;
                }
                case "PC Vx 9300 (93/62/31) 11":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCVx930011;
                }
                case "PC V 6300 (63/42/21) 16":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCV630016;
                }
                case "PC V 9300 (93/62/31) 16":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCV930016;
                }
                case "PC Vx 6300 (63/42/21) 16":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCVx630016;
                }
                case "PC Vx 9300(93 / 62 / 31) 16":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.PCVx930016;
                }
                case "M Vx 6":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.MVx6;
                }
                case "M V 11 new":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.MV11_hz;
                }
                case "M Vx 11 new":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.MVx11_hz;
                }
                case "M V 16 new":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.MV16_hz;
                }
                case "M Vx 16 new":
                {
                    return (MetalDetectorModel)Device.Matreshka.Constants.Model.MVx16_hz;
                }
                default:
                {
                    return MetalDetectorModel.Unknown;
                }
            }
        }
    }
}
