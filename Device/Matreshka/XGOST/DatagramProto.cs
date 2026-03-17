using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using IRAPROM.MyCore.Device;
using IRAPROM.MyCore.Device.Matreshka.XGOST;

namespace IRAPROM.MyCore.Device.Matreshka.XGOST
{
    public class DatagramProto : IDatagramProto
    {
        private readonly ushort _hardwareAddress;

        public DatagramProto()
        {
            _hardwareAddress = 0xFFFE;
        }

        public DatagramProto(ushort hardwareAddress)
        {
            _hardwareAddress = hardwareAddress;
        }



        public byte[] MakeRequestDatagram(short cmd, byte[] args = null)
        {
            var argsLength = (byte)(args?.Length ?? 0);
            var datagram = new byte[Constants.MetaInfoLength + argsLength];

            Constants.RequestMagicNumber.CopyTo(datagram, 0);

            datagram[3] = (byte)_hardwareAddress;
            datagram[4] = (byte)(_hardwareAddress >> 8);
            
            datagram[Constants.DataLengthOffset] = Constants.PacketMetaInfoLength;
            datagram[Constants.DataLengthOffset + 1] = 0;
            datagram[Constants.DataLengthOffset + 2] = 0;
            datagram[Constants.DataLengthOffset + 3] = 0;

            for (var i = Constants.FrameSequenceOffset; i < Constants.CommandCodeOffset; i++)
            {
                datagram[i] = 0;
            }

            datagram[Constants.CommandCodeOffset] = (byte)cmd;
            datagram[Constants.CommandCodeOffset + 1] = (byte)(cmd>>8);

            datagram[15] = 0x8B;        // PASSWORD
            datagram[16] = 0x69;
            datagram[17] = 0x3C;
            datagram[18] = 0x5A;

            if (args != null)
            {
                for (var i = Constants.DataOffset; i < Constants.DataOffset + argsLength; i++)
                {
                    datagram[i] = args[i - Constants.DataOffset];
                }
            }

            MakeTail(datagram);

            return datagram;
        }

        public byte[] GetResult(short cmd, byte[] response)
        {
            if (response == null) return Array.Empty<byte>();

            if (cmd != 0x21 && !ValidateChecksum(response))                         // TODO: Matreshka BUG Return Ethernet Parameters
            {
                Console.WriteLine($"Matreshka EX: DatagramProto: GetResult: data checksum is not valid!!!");

                return Array.Empty<byte>();
            }

            var error = response[Constants.ResultOffset];

            return ValidateRequestResult(error) ? response.Skip(Constants.ResultOffset + 1).Take(response.Length - 4 - Constants.ResultOffset).ToArray() : Array.Empty<byte>();
        }

        public bool ValidateRequestResult(byte result, byte commandCode = 0)
        {
            if (result == 0)
            {
                return true;
            }

            switch (result)
            {
                case 0x01:
                    Console.WriteLine($"Matreshka: DatagramProto: CheckResult: \"Parameter error\"!");
                    break;
                case 0x02:
                    Console.WriteLine($"Matreshka: DatagramProto: CheckResult: \"Timeout\"!");
                    break;
                case 0x03:
                    Console.WriteLine($"Matreshka: DatagramProto: CheckResult: \"File does not exist\"!");
                    break;
                case 0x04:
                    Console.WriteLine($"Matreshka: DatagramProto: CheckResult: \"Packet CRC error\"!");
                    break;
                default: 
                    Console.WriteLine($"Matreshka: DatagramProto: CheckResult: Unknown error!");
                    break;
            }

            return false;
        }

        public short GetCodeFromDatagram(byte[] request)
        {
            return BitConverter.ToInt16(request, Constants.CommandCodeOffset);
        }

        public byte[] MakeZonesSensitivityDatagram(short coilsCount, short[] sensorsSensitivity)
        {
            var datagram = new byte[coilsCount * 4 + Constants.MetaInfoLength];

            Constants.RequestMagicNumber.CopyTo(datagram, 0);

            datagram[3] = 1;                                // Serial port hardware address ?
            datagram[4] = (byte)(Constants.PacketMetaInfoLength + coilsCount * 4);      // Frame packet length (4 bytes)
            datagram[12] = (byte)Constants.SetZonesSensitivity.code;
            
            for (var i = 0; i < coilsCount * 2; i++)
            {
                datagram[14 + 2 * i] = (byte)(sensorsSensitivity[i] & 0xFF);
                datagram[14 + 2 * i + 1] = (byte)(sensorsSensitivity[i] >> 8);
            }

            MakeTail(datagram);

            return datagram;
        }

        private void MakeTail(byte[] datagram)
        {
            var data = new byte[datagram.Length - Constants.FrameSequenceOffset - 4];       // - 4 tail

            for (var i = 0; i < data.Length; i++)
            {
                data[i] = datagram[i + 8];
            }

            var checksum = GetChecksum(data, data.Length);

            datagram[datagram.Length - 4] = (byte)checksum;
            datagram[datagram.Length - 3] = (byte)(checksum >> 8);
            datagram[datagram.Length - 2] = 0xD;
            datagram[datagram.Length - 1] = 0xA;
        }

        private ushort GetChecksum(byte[] buf, int length)
        {
            byte c, treat, bcrc;

            ushort WCRC = 0, i, j;

            for (i = 0; i < length; i++)
            {
                c = buf[i];

                for (j = 0; j < 8; j++)
                {
                    treat = (byte)(c & 0x80);

                    c <<= 1;

                    bcrc = (byte)(WCRC >> 8 & 0x80);

                    WCRC <<= 1;

                    if (treat != bcrc)
                    {
                        WCRC ^= 0x1021;
                    }
                }
            }

            return WCRC;
        }

        private bool ValidateChecksum(byte[] response)
        {
            var frame = response.Skip(8).Take(response.Length - 4 - 8).ToArray();
            var checksum = GetChecksum(frame, frame.Length);

            return checksum == (response[response.Length - 3] << 8) + response[response.Length - 4];
        }
    }
}
