namespace IRAPROM.MyCore.Device
{
    public interface IDatagramProto
    {
        byte[] MakeRequestDatagram(short cmd, byte[] args = null);
        
        byte[] GetResult(short cmd, byte[] response);

        bool ValidateRequestResult(byte result, byte commandCode = 0);
        
        byte GetCodeFromDatagram(byte[] request);
    }
}
