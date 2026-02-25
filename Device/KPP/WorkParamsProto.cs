using IRAPROM.MyCore.Model.WP;

namespace IRAPROM.MyCore.Device.KPP
{
    internal class WorkParamsProto : IWorkParamsProto
    {
        public WorkParamsProto()
        {
            
        }

        public WorkParams GetWorkParams()
        {
            return null;
        }

        public void SetWorkProgramScene(WorkParams workParams)
        {
            throw new NotImplementedException();
        }

        public bool SetWorkParams(WorkParams workParams)
        {
            throw new NotImplementedException();
        }
        
        public void ScanCommands(byte startCode, byte endCode)
        {
            throw new NotImplementedException();
        }

        public void CallPassage()
        {
            throw new NotImplementedException();
        }

        public void CallAlarm()
        {
            throw new NotImplementedException();
        }

        public void ClearPassageCount()
        {
            throw new NotImplementedException();
        }
    }
}
