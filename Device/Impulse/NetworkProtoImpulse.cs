using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace Device.Impulse
{
    public class NetworkProtoImpulse : NetworkProtoCommonDual
    {
        public NetworkProtoImpulse(string ip, int portTCP, int timeOut = 15000) : base(ip, portTCP, timeOut)
        {
        }

        public override byte[] SendAndGet(byte[] outputBytes, int getCount)
        {
            if (!Send(outputBytes))
            {
                return null;
            }

            var _try = 5;
            var inputBytes = new byte[2048];

            while (_try > 0)
            {
                _try--;

                try
                {
                    var resultBuffer = Get(2);

                    if (resultBuffer == null || resultBuffer.Length == 0)
                    {
                        continue;
                    }

                    if (resultBuffer[0] == 0x5C)
                    {
                        var data = Get(resultBuffer[1]);

                        resultBuffer.CopyTo(inputBytes, 0);
                        data.CopyTo(inputBytes, 2);

                        inputBytes = inputBytes.Take(2 + resultBuffer[1]).ToArray();

                        break;
                    }
                    else
                    {
                        _try++;
                        continue;
                    }
                }
                catch (TimeoutException e)
                {
                    Console.WriteLine($"Response: timeout!\n");
                }
                catch (EndOfStreamException e)
                {
                    Console.WriteLine($"EX: SendAndGet: Impulse package signature not found: {e.Message}!\n");
                }
                catch (IOException e)
                {
                    Console.WriteLine($"Response: timeout!\n");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"EX: SendAndGet: {e.Message}!\n");
                }

                Thread.Sleep(150);
            }

#if DEBUG
            Console.WriteLine($"Response: {BitConverter.ToString(inputBytes)}\n");
#endif

            return inputBytes;
        }
    }
}
