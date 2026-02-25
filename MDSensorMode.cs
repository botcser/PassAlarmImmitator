using System;
using System.Collections.Generic;

namespace IRAPROM.MyCore.Model.MD
{
    public static class MDSensorMode
    {
        public enum enItems
        {

            Zones_0 = 0,
            Zones_1 = 1,
            Zones_2 = 2,


            //Zones_4 = 3,
            //Zones_2 = 4,
        }


        public enum enItemsFact
        {
            Zones_None,

            Zones_2,
            Zones_3,
            Zones_4,
            Zones_6,
            Zones_9,
            Zones_11,
            Zones_12,
            Zones_18,
            Zones_22,
            Zones_32,
            Zones_33,
            Zones_48,



        }

        /*
                public static short ZonesCount(enItems val)
                {
                    switch (val)
                    {
                        case enItems.Zones_6: return 6;
                        case enItems.Zones_12: return 12;
                        case enItems.Zones_18: return 18;

                        case enItems.Zones_4: return 4;
                        case enItems.Zones_2: return 2;

                        default: return 0;
                    }
                }
                public static short ZonesCount(short sVal)
                {
                    var val = (enItems)sVal;
                    bool isDefined = Enum.IsDefined(typeof(enItems), val);
                    if (!isDefined) return 0;

                    return ZonesCount(val);
                }
          */

        public static enItemsFact GetItemFact(MetalDetectorModel model, enItems val)
        {
            var cnt = MDModel.MaxZonesCount(model);
            switch (cnt)
            {
                case 4:
                    switch (val)
                    {
                        case enItems.Zones_0:
                            return enItemsFact.Zones_2;
                        case enItems.Zones_1:
                            return enItemsFact.Zones_4;
                        default:
                            return enItemsFact.Zones_None;
                    }

                case 6:
                    switch (val)
                    {
                        case enItems.Zones_0:
                            return enItemsFact.Zones_6;
                        default:
                            return enItemsFact.Zones_None;
                    }

                case 9:
                    switch (val)
                    {
                        case enItems.Zones_0:
                            return enItemsFact.Zones_3;
                        case enItems.Zones_1:
                            return enItemsFact.Zones_6;
                        case enItems.Zones_2:
                            return enItemsFact.Zones_9;
                        default:
                            return enItemsFact.Zones_None;
                    }

                case 12:
                    switch (val)
                    {
                        case enItems.Zones_0:
                            return enItemsFact.Zones_6;
                        case enItems.Zones_1:
                            return enItemsFact.Zones_12;
                        default:
                            return enItemsFact.Zones_None;
                    }

                case 18:
                    switch (val)
                    {
                        case enItems.Zones_0:
                            return enItemsFact.Zones_6;
                        case enItems.Zones_1:
                            return enItemsFact.Zones_12;
                        case enItems.Zones_2:
                            return enItemsFact.Zones_18;
                        default:
                            return enItemsFact.Zones_None;
                    }

                case 33:
                    switch (val)
                    {
                        case enItems.Zones_0:
                            return enItemsFact.Zones_11;
                        case enItems.Zones_1:
                            return enItemsFact.Zones_22;
                        case enItems.Zones_2:
                            return enItemsFact.Zones_33;
                        default:
                            return enItemsFact.Zones_None;
                    }

                case 48:
                    switch (val)
                    {
                        case enItems.Zones_0:
                            return enItemsFact.Zones_18;
                        case enItems.Zones_1:
                            return enItemsFact.Zones_32;
                        case enItems.Zones_2:
                            return enItemsFact.Zones_48;
                        default:
                            return enItemsFact.Zones_None;
                    }





                default:
                    return enItemsFact.Zones_None;
            }


        }
        public static enItemsFact GetItemFact(short smodel, short szn)
        {
            var model = (MetalDetectorModel)smodel;
            var zn = (enItems)szn;

            bool isDefined = Enum.IsDefined(typeof(MetalDetectorModel), model);
            if (!isDefined) return enItemsFact.Zones_None;

            isDefined = Enum.IsDefined(typeof(enItems), zn);
            if (!isDefined) return enItemsFact.Zones_None;

            return GetItemFact(model, zn);
        }

        public static string GetItemNameFact(enItemsFact val)
        {
            switch (val)
            {
                case enItemsFact.Zones_2: return $"2 зоны";
                case enItemsFact.Zones_3: return $"3 зоны";
                case enItemsFact.Zones_4: return $"4 зоны";

                case enItemsFact.Zones_6: return $"6 зон";
                case enItemsFact.Zones_9: return $"9 зон";
                case enItemsFact.Zones_11: return $"11 зон";
                case enItemsFact.Zones_12: return $"12 зон";
                case enItemsFact.Zones_18: return $"18 зон";
                case enItemsFact.Zones_22: return $"22 зон";
                case enItemsFact.Zones_32: return $"32 зон";
                case enItemsFact.Zones_33: return $"33 зон";
                case enItemsFact.Zones_48: return $"48 зон";

                default: return "???";
            }
        }

        public static string GetItemNameFact(MetalDetectorModel model, enItems val)
        {
            var itm = GetItemFact(model, val);
            return GetItemNameFact(itm);
        }

        public static string GetItemNameFact(short smodel, short szn)
        {
            var model = (MetalDetectorModel)smodel;
            var zn = (enItems)szn;

            bool isDefined = Enum.IsDefined(typeof(MetalDetectorModel), model);
            if (!isDefined) return "???";

            isDefined = Enum.IsDefined(typeof(enItems), zn);
            if (!isDefined) return "???";

            return GetItemNameFact(model, zn);
        }

        public static enItems ZonesRegimMax(MetalDetectorModel model)
        {
            var cnt = MDModel.MaxZonesCount(model);
            switch (cnt)
            {
                case 18:
                    return enItems.Zones_2;

                case 12:
                    return enItems.Zones_1;

                case 9:
                    return enItems.Zones_2;

                case 6:
                    return enItems.Zones_0;

                case 4:
                    return enItems.Zones_1;

                default:
                    return enItems.Zones_0;
            }
        }

        public static enItems ZonesRegimMax(short smodel)
        {
            var model = (MetalDetectorModel)smodel;

            bool isDefined = Enum.IsDefined(typeof(MetalDetectorModel), model);
            if (!isDefined)
                return enItems.Zones_0;

            return ZonesRegimMax(model);

        }


        public static string GetItemName(MetalDetectorModel model, enItems val)
        {
            var itfct = GetItemFact(model, val);
            return GetItemNameFact(itfct);
        }

        public static string GetItemName(short smodel, short szn)
        {
            var model = (MetalDetectorModel)smodel;
            var zn = (enItems)szn;

            bool isDefined = Enum.IsDefined(typeof(MetalDetectorModel), model);
            if (!isDefined) return "???";

            isDefined = Enum.IsDefined(typeof(enItems), zn);
            if (!isDefined) return "???";

            return GetItemName(model, zn);
        }
        
        public static List<enItems> GetItemsEnum(MetalDetectorModel model)
        {
            var lst = new List<enItems>();
            var cnt = MDModel.MaxZonesCount(model);
            switch (cnt)
            {
                case 48:
                case 33:
                case 18:
                case 9:
                    lst.Add(enItems.Zones_0);
                    lst.Add(enItems.Zones_1);
                    lst.Add(enItems.Zones_2);
                    break;

                case 12:
                    lst.Add(enItems.Zones_0);
                    lst.Add(enItems.Zones_1);
                    break;

                case 6:
                    lst.Add(enItems.Zones_0);
                    break;

                case 4:
                    lst.Add(enItems.Zones_0);
                    lst.Add(enItems.Zones_1);
                    break;

                default:
                    break;
            }

            return lst;
        }

        public static List<enItems> GetItemsEnum(short sVal)
        {
            var val = (MetalDetectorModel)sVal;
            bool isDefined = Enum.IsDefined(typeof(MetalDetectorModel), val);
            if (!isDefined) return null;

            return GetItemsEnum(val);

        }

        /*
        public static List<IdName> GetItems()
        {
            var lst = new List<IdName>();

            lst.Add(GetIdName(enItems.Zones_6));
            lst.Add(GetIdName(enItems.Zones_12));
            lst.Add(GetIdName(enItems.Zones_18));
            return lst;
        }
        */


        /*
        //Получаем мой режим по режиму устройства
        public static  byte GetMySensorMode(MDModel.enItems model, byte mode)
        {
            var cnt = MDModel.ZonesCount(model);
            switch (cnt)
            {
                case 18:
                case 12:
                case 6:
                    return mode;

                
                case 4:
                    switch (mode)
                    {
                        case 0:
                            return (byte)enItems.Zones_2;

                        case 1:
                            return (byte)enItems.Zones_4;

                        default:
                            return mode;
                    }
                    

                default:
                    return mode;
            }

        }
        

        public static byte GetMySensorMode(short smodel, byte mode)
        {
            var mdl = (MDModel.enItems)smodel;
            bool isDefined = Enum.IsDefined(typeof(MDModel.enItems), mdl);
            if (!isDefined) return mode;

            return GetMySensorMode(mdl, mode);
        }
        */

        /*
        public static byte GetDeviceSensorMode(byte mode)
        {
            var val =  (enItems)((short)mode);
            bool isDefined = Enum.IsDefined(typeof(enItems), val);
            if (!isDefined) return mode;

            switch (val)
            {
                case enItems.Zones_6:
                case enItems.Zones_12:
                case enItems.Zones_18:
                    return mode;
                
                case enItems.Zones_4:
                    return 1;
                    
                case enItems.Zones_2:
                    return 0;
                
                default:
                    return mode;
            }


        }
        */
        
    }
}
