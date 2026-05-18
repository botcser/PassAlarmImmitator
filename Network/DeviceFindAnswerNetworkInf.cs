using System;
using System.IO;
using System.Net;
using IRAPROM.MyCore.Auxiliary;

namespace IRAPROM.MyCore.MyNetwork
{
    [Serializable]

    public class DeviceFindAnswerNetworkInf
    {
        public byte[] arIP { get; set; } = new byte[4];
        public byte[] arMask { get; set; } = new byte[4];
        public byte[] arIPGateway { get; set; } = new byte[4];

        public string IP { get; set; } = "";
        public string Mask { get; set; } = "";
        public string IPGateway { get; set; } = "";

        public ushort PortTCP { get; set; } = 0;
        public ushort PortUDP { get; set; } = 0;

        public byte[] mac { get; set; } = new byte[6];
        public string MAC => Convert.ToHexString(mac);

        public string Model { get; set; } = ""; //Для новых монопанелей
        public string Version { get; set; } = ""; //Для новых монопанелей



        static bool CheckLength(byte[] arr)
        {
            if (arr.Length == (23 + 4))
                return true;

            if (arr.Length == 41)
                return true;

            return false;
        }
        public static DeviceFindAnswerNetworkInf GetRecFromPacket(byte[] arr)
        {
            if (!CheckLength(arr))
                return null;

            var rec = new DeviceFindAnswerNetworkInf();

            using (var ms = new MemoryStream(arr))
            {
                using (var br = new BinaryReader(ms))
                {
                    byte b = br.ReadByte(); //Пропуск 1 байта


                    rec.arIP = br.ReadBytes(4);
                    rec.IP = $"{rec.arIP[0]}.{rec.arIP[1]}.{rec.arIP[2]}.{rec.arIP[3]}";

                    rec.arMask = br.ReadBytes(4);
                    rec.Mask = $"{rec.arMask[0]}.{rec.arMask[1]}.{rec.arMask[2]}.{rec.arMask[3]}";

                    rec.arIPGateway = br.ReadBytes(4);
                    rec.IPGateway = $"{rec.arIPGateway[0]}.{rec.arIPGateway[1]}.{rec.arIPGateway[2]}.{rec.arIPGateway[3]}";

                    rec.PortTCP = br.ReadUInt16();
                    rec.PortUDP = br.ReadUInt16();

                    rec.mac = br.ReadBytes(6);


                    //Для новых монопанелей
                    if (arr.Length == 41)
                    {
                        byte[] arModel = br.ReadBytes(5);
                        foreach (byte item in arModel)
                        {
                            rec.Model += Convert.ToChar(item);
                        }
                        b = br.ReadByte(); //Пропуск 1 байта

                        byte[] arVer = br.ReadBytes(7);
                        foreach (byte item in arVer)
                        {
                            rec.Version += Convert.ToChar(item);
                        }
                    }
                }
            }

            return rec;
        }

        public static DeviceFindAnswerNetworkInf ParseOldImpulseResponse(byte[] arr)
        {
            if (arr.Length != (22))
                return null;

            var rec = new DeviceFindAnswerNetworkInf();

            using (var ms = new MemoryStream(arr))
            {
                using (var br = new BinaryReader(ms))
                {
                    rec.arIP = br.ReadBytes(4);
                    rec.IP = $"{rec.arIP[0]}.{rec.arIP[1]}.{rec.arIP[2]}.{rec.arIP[3]}";
                    if (!IPAddress.TryParse(rec.IP, out IPAddress address))
                        return null;

                    rec.arMask = br.ReadBytes(4);
                    rec.Mask = $"{rec.arMask[0]}.{rec.arMask[1]}.{rec.arMask[2]}.{rec.arMask[3]}";

                    rec.arIPGateway = br.ReadBytes(4);
                    rec.IPGateway = $"{rec.arIPGateway[0]}.{rec.arIPGateway[1]}.{rec.arIPGateway[2]}.{rec.arIPGateway[3]}";

                    rec.PortTCP = br.ReadUInt16();
                    rec.PortUDP = br.ReadUInt16();

                    rec.mac = br.ReadBytes(6);

                }
            }

            return rec;

        }

    }
}
