using IRAPROM.MyCore.Model.WP;

namespace Device
{
    public interface IWorkParamsProto
    {
        WorkParams GetWorkParams();

        bool SetWorkParams(WorkParams workParams);

        bool SelfTest(WorkParams workParams);
        
        void ScanCommands(byte startCode, byte endCode);
    }
}
