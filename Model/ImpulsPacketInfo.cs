using System;
using System.IO;
using IRAPROM.MyCore.Auxiliary;
using IRAPROM.MyCore.Device.Impulse;

namespace IRAPROM.MyCore.Model
{
    public class ImpulsPacketInfo : MDSaveInfo
    {
        public uint NormalPassNum = 0;
        public uint NormalReturnNum = 0;
        public uint AlarmPassNum = 0;
        public uint AlarmReturnNum = 0;


        public byte sensorMode = 0;
        public string SensorModeName => $"Режим/Зоны обнаруж = {sensorMode}";

        //public int[] sensors = new int[18];
        //public byte[] sensorsProcessed = new byte[18];

        public byte rowCount = 11;
        public byte zonesCount = 0;

        public int[] Left = new int[24];
        public int[] Right = new int[24];
        public int[] Center = new int[24];

        //public byte[] sensors = new byte[72]; 
        public byte[] sensors = new byte[33];
        
        public bool ExistAlarm()
        {
            for (int i = 0; i < sensors.Length; i++) 
            { 
                if (sensors[i] > 0)
                    return true;
            }
            return false;
            //int sum = sensors.Sum();
            //return (sum > 0);
        }

        public static ImpulsPacketInfo PacketToInfo(byte[] arr)
        {
            if (arr.Length != 31)
                return null;

            if (!Constants.CheckImpulseHeader(arr))
                return null;

            var rec = new ImpulsPacketInfo();
            rec.logTime = DateTime.Now;

            //Считываем проходы
            using (var ms = new MemoryStream(arr))
            {
                using (var br = new BinaryReader(ms))
                {
                    var artmp = br.ReadBytes(5); //0-4

                    var passBytes = br.ReadBytes(3); //5-7 
                    rec.NormalPassNum = (uint)(((passBytes[0] >> 4) * 10 + (passBytes[0] & 0x0f)) * 10000
                                               + ((passBytes[1] >> 4) * 10 + (passBytes[1] & 0x0f)) * 100
                                               + (passBytes[2] >> 4) * 10 + (passBytes[2] & 0x0f));


                    //artmp = br.ReadBytes(3); 
                    passBytes = br.ReadBytes(3); //8-10
                    //if (md.IdModel == MDModel.enItems.PCV1800_thermoMB)
                    rec.NormalReturnNum = (uint)(((passBytes[0] >> 4) * 10 + (passBytes[0] & 0x0f)) * 10000
                                                 + ((passBytes[1] >> 4) * 10 + (passBytes[1] & 0x0f)) * 100
                                                 + (passBytes[2] >> 4) * 10 + (passBytes[2] & 0x0f));



                    passBytes = br.ReadBytes(3); //11-13
                    rec.AlarmPassNum = (uint)(((passBytes[0] >> 4) * 10 + (passBytes[0] & 0x0f)) * 10000 + ((passBytes[1] >> 4) * 10 + (passBytes[1] & 0x0f)) * 100 + (passBytes[2] >> 4) * 10 + (passBytes[2] & 0x0f));

                    //artmp = br.ReadBytes(3); //14-16
                    passBytes = br.ReadBytes(3); 
                    rec.AlarmReturnNum = (uint)(((passBytes[0] >> 4) * 10 + (passBytes[0] & 0x0f)) * 10000 + ((passBytes[1] >> 4) * 10 + (passBytes[1] & 0x0f)) * 100 + (passBytes[2] >> 4) * 10 + (passBytes[2] & 0x0f));

                    //Левый ряд
                    var sen = br.ReadBytes(3); //17-19
                    for ( int i = 0; i < 3; i++ )
                    {
                        byte btSens = sen[i];
                        for (int j = 0; j < 8; j++)
                        {
                            rec.Left[8*i + j] = (btSens >> j) & 0x01;

                        }
                    }
                    //Правый ряд
                    sen = br.ReadBytes(3); //20-22
                    for (int i = 0; i < 3; i++)
                    {
                        byte btSens = sen[i];
                        for (int j = 0; j < 8; j++)
                        {
                            rec.Right[8 * i + j] = (btSens >> j) & 0x01;

                        }
                    }

                    for (int i = 0; i < rec.Left.Length; i++)
                    {

                        if (rec.Left[i] == 1 && rec.Right[i] == 1)
                            rec.Center[i] = 1;
                    }


                    /*
                    for (int j = 0; j < 24; j++)
                    {
                        rec.sensors[0 + j] = (byte)rec.Left[j];
                    }
                    for (int j = 0; j < 24; j++)
                    {
                        rec.sensors[24 + j] = (byte)rec.Right[j];
                    }
                    for (int j = 0; j < 24; j++)
                    {
                        rec.sensors[48 + j] = (byte)rec.Center[j];
                    }
                    */
                    for (int j = 0; j < 11; j++)
                    {
                        rec.sensors[0 + j] = (byte)rec.Left[j];
                    }
                    for (int j = 0; j < 11; j++)
                    {
                        //rec.sensors[11 + j] = (byte)rec.Right[j];
                        rec.sensors[11 + j] = (byte)rec.Center[j];
                    }
                    for (int j = 0; j < 11; j++)
                    {
                        //rec.sensors[22 + j] = (byte)rec.Center[j];
                        rec.sensors[22 + j] = (byte)rec.Right[j];
                    }

                    rec.zonesCount = br.ReadByte(); //23
                    switch (rec.zonesCount)
                    {
                        case 11:
                        case 22:
                        case 33:
                            rec.rowCount = 11;
                            break;

                        default:
                            rec.rowCount = 6;
                            break;
                    }

                    var bTemp = br.ReadByte(); //24 - непонятный

                    rec.mac = br.ReadBytes(6); //25-30
                }
            }
            return rec;
        }


    }
}
