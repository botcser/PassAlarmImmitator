using System.Collections.Generic;

namespace Device.Matreshka
{
    public class Matreshka : DeviceMetalDetector
    {
        public override string SeriesName => "Матрешка";
        public override string ModelName => Constants.GetModelName(Model);
        public Constants.Model Model => (Constants.Model)WorkParams.ModelId;
        public override List<short> AvailableZonesCount => WorkParams == null ? null : Constants.Models[ModelName].AvailableZonesCount;
        public override short PortTCP { get => _portTCP == 0 ? FamilyInfo.PortTCP : _portTCP; set {} }
        public override short PortUDP { get => _portUDP == 0 ? FamilyInfo.PortUDP : _portUDP; set {} }

        public override int ZonesCount
        {
            get => WorkParams == null ? 0 : Constants.Models[ModelName].AvailableZonesCount[WorkParams.ZonesSensorMode];
            set
            { 
                if (WorkParams == null || value >= Constants.Models[ModelName].AvailableZonesCount.Count) return;           // omg stupid mapper
                
                WorkParams.ZonesSensorMode = (byte)value;

                switch (Model)
                {
                    case Constants.Model.MV6:
                    case Constants.Model.MVx6:
                    case Constants.Model.MV11_hz:
                    case Constants.Model.MVx11_hz:
                    case Constants.Model.MV16_hz:
                    case Constants.Model.MVx16_hz:
                        WorkParams.ZonesSensorMode = (byte)Constants.Models[ModelName].AvailableZonesCount[WorkParams.ZonesSensorMode];
                        break;
                    default:
                        WorkParams.ZoneMode = Constants.Models[ModelName].AvailableZonesCount[WorkParams.ZonesSensorMode].ToString();
                        break;
                }
            }
        }
        
        public Matreshka() : base(new WorkParamsProto(new DatagramProto(), Constants.GetCommands, Constants.SetCommands), new Constants())
        {

        }

#if USE_COMMAND_CENTER
        public Matreshka(string ip, short port) : base(new WorkParamsProto(new NetworkProtoHttp(ip, Constants.PortTCPDefault), new DatagramProto(), Constants.GetCommands, Constants.SetCommands), new Constants())
        {
            IP = ip;
            PortTCP = port;
        }
#else
        public Matreshka(string ip, short port) : base(new WorkParamsProto(new NetworkProtoCommonDual(ip, Constants.PortTCPDefault), new DatagramProto(), Constants.GetCommands, Constants.SetCommands), new Constants())
        {
            IP = ip;
            PortTCP = port;
        }
#endif



        public void ScanCommands()
        {
            WorkParamsProto.ScanCommands(0x28, 0x28);
            WorkParamsProto.ScanCommands(0x2C, 0xFF);
        }
    }
}
