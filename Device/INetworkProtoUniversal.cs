namespace Device
{
    public interface INetworkProtoUniversal : INetworkProto
    {
        byte[] SendAndGet(byte[] outputBytes);
    }
}
