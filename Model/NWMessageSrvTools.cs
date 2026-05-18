using Casualbunker.Server.Common;
using IRAPROM.MyCore.Auxiliary;
using IRAPROM.MyCore.DBModel;
using IRAPROM.MyCore.Device;
using IRAPROM.MyCore.Device.Impulse;
using IRAPROM.MyCore.Device.Matreshka;
//using IRAPROM.MyCore.Model.Lic;
using IRAPROM.MyCore.Model.MD;
using IRAPROM.MyCore.Model.WP;
using IRAPROM.MyCore.MyNetwork;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Extensions;
//using static IRAPROM.MyCore.Model.NWMessage;

namespace IRAPROM.MyCore.Model
{

    public static class NWMessageSrvTools
    {
        public static void MakeMetalDeviceFromOldPacketInfo(DeviceFindAnswerNetworkInf rec, MetalDetectorSeries mdlType)
        {
            if (MyARM.Instance.AddedDevicesTryGetValue(rec.MAC, out var dev, out var onChanged))
            {
                if (dev.dtLastInfFindMD == default)
                {
                    dev.dtLastInfFindMD = DateTime.Now;
                    dev.FindNetworkStatus = 1;
                }
                else
                {
                    dev.FindNetworkStatus = 1;
                    dev.dtLastInfFindMD = DateTime.Now;

                    if (dev.dtLastInfFindMD.AddSeconds(10) < DateTime.Now)  //Информация с прошлых запросов
                    {
                        //Что-то делаем
                    }
                    else //Информация уже найдена на предыдущих срабатываниях таймера текущего поиска - просто меняем время
                    {
                    }
                }

                onChanged(dev);

                return;
            }

            dev = MyARM.Instance.DevicesFound.FirstOrDefault(x => x.MAC == rec.MAC);

            if (dev == null)
            {
                dev = new MetDetector();
                dev.MAC = rec.MAC;
                dev.IP = rec.IP;
                dev.Mask = rec.Mask;
                dev.PortTCP = rec.PortTCP;
                dev.PortUDP = rec.PortUDP;
                dev.Gateway = rec.IPGateway;
                dev.dtLastInfFindMD = DateTime.Now;

                switch (mdlType)
                {
                    case MetalDetectorSeries.Impulse:
                        dev.ModelSeries = MetalDetectorSeries.Impulse;
                        dev.DeviceMetalDetector = new Impulse(rec.IP, rec.PortTCP) { MAC = dev.MAC, Mask = rec.Mask, Gateway = rec.IPGateway };
                        break;

                    case MetalDetectorSeries.Matryoshka:
                        dev.ModelSeries = MetalDetectorSeries.Matryoshka;
                        dev.DeviceMetalDetector = new Matreshka(rec.IP, rec.PortTCP) { MAC = dev.MAC, Mask = rec.Mask, Gateway = rec.IPGateway };
                        break;

                    default:
                        dev.ModelId = -1;
                        dev.ModelSeries = MetalDetectorSeries.Unknown;
                        break;
                }

                dev.Name = $"{dev.DeviceMetalDetector.ModelName} {dev.DeviceMetalDetector.SeriesName}";

                //Validator.FoundDevices.Add(dev.DeviceMetalDetector);
                MyARM.Instance.DevicesFound.Add(dev);
            }
            else
                dev.dtLastInfFindMD = DateTime.Now;
        }
    }

}
