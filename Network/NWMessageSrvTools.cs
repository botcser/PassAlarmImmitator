using IRAPROM.MyCore.DBModel;
using IRAPROM.MyCore.Device.Impulse;
using IRAPROM.MyCore.Device.Matreshka;
using IRAPROM.MyCore.Model.MD;
using IRAPROM.MyCore.MyNetwork;
using PassAlarmSimulator.Validator;

namespace IRAPROM.MyCore.Model
{

    public static class NWMessageSrvTools
    {
        public static void MakeMetalDeviceFromOldPacketInfo(DeviceFindAnswerNetworkInf rec, MetalDetectorSeries mdlType)
        {
            var dev = new MetDetector();
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

                dev.Name = $"{MetalDetectorSeriesLib.GetModel(dev.ModelId)}";

                Validator.FoundDevices.Add(dev.DeviceMetalDetector);
        }
    }

}
