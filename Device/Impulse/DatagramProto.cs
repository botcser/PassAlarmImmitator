using System;
using System.Linq;

namespace Device.Impulse
{
    public class DatagramProto : IDatagramProto
    {
        private const int CommandCodePosition = 2;

        public byte[] MakeRequestDatagram(short command, byte[] args = null)
        {
            if (command == Constants.SetNetworkParams.code)             // TODO: такое дерьмище, лучше не придумал, как обделать другую комманду Импульса на установку Сетевых Настроек.
            {
                var datagram = new byte[Constants.DatagramMetaInfoLength - 2 + Constants.ChecksumLength - 1 + (args?.Length ?? 0)];

                MakeHeader(datagram, (byte)command);

                args?.CopyTo(datagram, index: Constants.DatagramMetaInfoLength - 2);
                datagram[1] = 0x00;

                return datagram;
            }
            else
            {
                var datagram = new byte[Constants.DatagramMetaInfoLength + Constants.ChecksumLength + (args?.Length ?? 0)];

                MakeHeader(datagram, (byte)command);

                args?.CopyTo(datagram, index: Constants.DatagramMetaInfoLength);

                MakeTail(datagram);

                return datagram;
            }
        }

        public byte[] GetResult(short cmd, byte[] response)
        {
            if (!ValidateChecksum(response))
            {
                Console.WriteLine($"Impulse: DatagramProto: GetResult: data checksum is not valid!");
                
                return Array.Empty<byte>();
            }

            var responseByte = response[2];

            return ValidateRequestResult(responseByte) ? response.Skip(5).Take(response[1] - 5).ToArray() : Array.Empty<byte>();
        }

        public bool ValidateRequestResult(byte result, byte commandCode = 0)
        {
            return result - commandCode == 1;
        }

        public byte GetCodeFromDatagram(byte[] request)
        {
            return request[CommandCodePosition];
        }

        private static void MakeHeader(byte[] datagram, byte command)
        {
            datagram[0] = 0x5C;
            datagram[1] = (byte)(datagram.Length - 2);
            datagram[2] = command;
            datagram[3] = 0x00;
            datagram[4] = 0x00;
        }
        
        private static void MakeTail(byte[] datagram)
        {
            var checksum = GetChecksum(datagram);
            
            datagram[datagram.Length - 1] = (byte)(checksum);
        }
        
        private static byte GetChecksum(byte[] bytes)
        {
            return bytes.Aggregate<byte, byte>(0, (current, i) => (byte)(current + i));
        }

        private bool ValidateChecksum(byte[] response)
        {
            var bytes = response.Take(response.Length - 2).ToArray();
            var checksum = GetChecksum(bytes);

            return checksum == response[response.Length - 1];
        }
    }
}
