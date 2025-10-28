namespace Device
{
    public interface INetworkProto
    {
        public string Ip { get; set; }

        bool Connect();

        void Disconnect();

        bool Send(byte[] bytes);

        byte[] Get(int count);
    }
}
