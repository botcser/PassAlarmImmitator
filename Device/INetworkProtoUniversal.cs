namespace IRAPROM.MyCore.Device
{
    public interface INetworkProtoUniversal : INetworkProto
    {
        byte[] SendAndGet(byte[] outputBytes);
    }
}
