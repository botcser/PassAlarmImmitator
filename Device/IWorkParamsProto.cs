//using Ira.Prom.Video.Server.Proto;
using IRAPROM.MyCore.Model.WP;

namespace IRAPROM.MyCore.Device
{
    public interface IWorkParamsProto
    {
        WorkParams GetWorkParams();

        bool SetWorkingMode(WorkParams workParams);

        bool SetWorkParams(WorkParams workParams);

        void SetNetworkParams(WorkParams workParams);

        void ScanCommands(byte startCode, byte endCode);

        void CallPassage();

        void CallAlarm();

        void ClearPassageCount();
    }
}