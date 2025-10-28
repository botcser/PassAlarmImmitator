using Newtonsoft.Json;
using System.Collections.Generic;

namespace Device.Impulse
{
    public class Impulse : DeviceMetalDetector
    {
        public override string SeriesName => "Импульс";
        public override string ModelName => Constants.GetModelName(Model);
        public Constants.Model Model => (Constants.Model)WorkParams.ModelId; 
        public override List<short> AvailableZonesCount => WorkParams == null ? null : Constants.Models[ModelName].AvailableZonesCount;
        
        [JsonIgnore]
        public override int ZonesCount
        {
            get => WorkParams == null ? 0 : Constants.Models[ModelName].AvailableZonesCount[WorkParams.ZonesSensorMode];
            set
            {
                if (WorkParams == null || value >= Constants.Models[ModelName].AvailableZonesCount.Count) return;       // omg stupid mapper

                WorkParams.ZonesSensorMode = (byte)value;
                WorkParams.ZoneMode = Constants.Models[ModelName].AvailableZonesCount[WorkParams.ZonesSensorMode].ToString();
            }
        }

        public Impulse() : base(new WorkParamsProto(new DatagramProto(), Constants.GetCommands, Constants.SetCommands), new Constants())
        {

        }
#if USE_COMMAND_CENTER            
        public Impulse(string ip, short portTCP) : base(new WorkParamsProto(new NetworkProtoHttp(ip, Constants.PortTCPDefault), new DatagramProto(), Constants.GetCommands, Constants.SetCommands), new Constants())
        {
            IP = ip;
            PortTCP = portTCP;
        }
#else
        public Impulse(string ip, short portTCP) : base(new WorkParamsProto(new NetworkProtoImpulse(ip, Constants.PortTCPDefault), new DatagramProto(), Constants.GetCommands, Constants.SetCommands), new Constants())
        {
            IP = ip;
            PortTCP = portTCP;
        }
#endif
    }
}
