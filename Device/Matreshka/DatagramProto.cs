using System;
using System.Linq;

namespace Device.Matreshka
{
    public class DatagramProto : IDatagramProto
    {
        private const int MetaInfoLength = 3 + 1 + 4 + PacketMetaInfoLength;    // Start frame marker + Hardware address + Frame packet length + PacketMetaInfoLength
        private const int PacketMetaInfoLength = 4 + 2 + 2 + 2;                 // Frame number + Command + CRC checksum + End frame marker
        private const int CommandCodePosition = 12;

        public byte[] MakeRequestDatagram(short cmd, byte[] args = null)
        {
            var argsLength = (byte)(args?.Length ?? 0);
            var datagram = new byte[18 + argsLength];

            Constants.RequestMagicNumber.CopyTo(datagram, 0);

            datagram[3] = 0;
            datagram[4] = (byte)(PacketMetaInfoLength + argsLength);

            for (var i = 5; i <= 11; i++)
            {
                datagram[i] = 0;
            }

            datagram[12] = (byte)cmd;
            datagram[13] = 0;

            if (args != null)
            {
                for (var i = 14; i < 14 + argsLength; i++)
                {
                    datagram[i] = args[i - 14];
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
                Console.WriteLine($"Matreshka: DatagramProto: GetResult: data checksum is not valid!");
                
                return Array.Empty<byte>();
            }

            var error = response[14];

            return ValidateRequestResult(error) ? response.Skip(15).Take(response.Length - 4 - 15).ToArray() : Array.Empty<byte>();
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

        public byte GetCodeFromDatagram(byte[] request)
        {
            return request[CommandCodePosition];
        }

        public byte[] MakeZonesSensitivityDatagram(short coilsCount, short[] sensorsSensitivity)
        {
            var datagram = new byte[coilsCount * 4 + MetaInfoLength];

            Constants.RequestMagicNumber.CopyTo(datagram, 0);

            datagram[3] = 1;                                // Serial port hardware address ?
            datagram[4] = (byte)(PacketMetaInfoLength + coilsCount * 4);      // Frame packet length (4 bytes)
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
            var data = new byte[datagram.Length - Constants.RequestMagicNumber.Length - 1 - 4 - 4];       // - 1 hw ser port, - 4 length frame - 4 tail
            
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
