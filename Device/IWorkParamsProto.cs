using IRAPROM.MyCore.Model.WP;

namespace Device
{
    public interface IWorkParamsProto
    {
        WorkParams GetWorkParams();

        void SetWorkProgramScene(WorkParams workParams);

        bool SetWorkParams(WorkParams workParams);

        bool SelfTest(WorkParams workParams);
        
        void ScanCommands(byte startCode, byte endCode);

        void CallPassage();
        
        void CallAlarm();

        void ClearPassageCount();
    }
}
