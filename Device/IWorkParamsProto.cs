using IRAPROM.MyCore.Model.WP;

namespace IRAPROM.MyCore.Device
{
    public interface IWorkParamsProto
    {
        WorkParams GetWorkParams();

        void SetWorkProgramScene(WorkParams workParams);

        bool SetWorkParams(WorkParams workParams);

        void ScanCommands(byte startCode, byte endCode);

        void CallPassage();
        
        void CallAlarm();

        void ClearPassageCount();
    }
}
