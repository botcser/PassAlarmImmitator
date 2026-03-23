using Newtonsoft.Json;

namespace IRAPROM.MyCore.Device
{
    public interface INetworkProto
    {
        public string Ip { get; set; }

        public int PortTCP { get; set; }

        bool Connect();

        void Disconnect();

        bool Send(byte[] bytes);

        byte[] Get(int count);
    }
}
