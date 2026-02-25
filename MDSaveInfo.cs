using IRAPROM.MyCore.Auxiliary;
using IRAPROM.MyCore.DBModel;
using System;

namespace IRAPROM.MyCore.Model
{
    public class MDSaveInfo
    {
        public MetDetector MetDetector;
        public DateTime logTime;

        public byte[] mac = new byte[6];
        public string MAC => Convert.ToHexString(mac);

        public decimal? Temperature = null;
        public string Explosives = "";
        public short? Radiation = null;

        public virtual void AddInfToDB()
        {

        }

        public virtual void SaveEventToXML()
        {


        }

    }
}
