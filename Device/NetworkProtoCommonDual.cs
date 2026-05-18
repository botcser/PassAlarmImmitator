using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Extensions;
using Newtonsoft.Json;

namespace IRAPROM.MyCore.Device
{
    public class NetworkProtoCommonDual : INetworkProtoDual, IDisposable
    {
        [JsonProperty]
        public string Ip { get; set; }
        [JsonProperty]
        public int PortTCP { get; set; }

        internal TcpClient Socket;
        internal NetworkStream Stream;
        
        [JsonProperty]
        private readonly int _port;
        [JsonProperty]
        public int Timeout { get; set; } = 3000;

        [JsonIgnore]
        internal IPEndPoint IPEndPoint => new IPEndPoint(IPAddress.Parse(Ip), _port);

        public NetworkProtoCommonDual()
        {
            
        }

        public NetworkProtoCommonDual(string ip, int portTCP, int timeOut = 0)
        {
            Ip = ip;
            _port = portTCP;
            Timeout = timeOut == 0 ? Timeout : timeOut;
            Socket = new TcpClient();
            Socket.SendTimeout = Socket.ReceiveTimeout = Timeout;
        }

        public bool Connect()
        {
            try
            {
                if (Socket != null)
                {
                    if (Socket.IsAlive()) return true;

                    if (Socket.Client == null || Socket.Client.IsBound)
                    {
#if DEBUGG
                        Console.WriteLine($"disposing socket {(Socket.Client == null ? "closed" : Socket.Client.Handle.ToString())} {Ip}:{_port}...");
#endif
                        Socket.Dispose();
                    }
                } 

                Socket = new TcpClient();
                Socket.SendTimeout = Socket.ReceiveTimeout = Timeout;

#if DEBUGG
                Console.Write($"Connecting {Ip}:{_port}...");
#endif
                
                if (!Socket.BeginConnect(Ip, _port, null, null).AsyncWaitHandle.WaitOne(5000, false))
                {
                    Socket.Close();
#if DEBUG
                    Console.WriteLine($"Connection attempt timed out after {5000}ms.");
#endif
                    return false;
                }

#if DEBUGG
                Console.WriteLine("success");
#endif

                if (!Socket.Connected) return false;

                Stream = Socket.GetStream();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX: NetworkProtoCommonDual: Connect: {IPEndPoint.Address}:{IPEndPoint.Port} - {ex.Message}");

                return false;
            }
        }

        public void Disconnect()
        {
            if (Socket.Client != null)
            {
                var socketHandle = Socket.Client.Handle;

#if DEBUGG
                Console.WriteLine($"close socket {socketHandle} {Ip}:{_port}.");
#endif
            }

            Socket.Close();
        }

        public bool Send(byte[] bytes)
        {
#if DEBUGG
            Console.WriteLine($"\nNetworkProtoCommonDual: Send: {IPEndPoint.Address}:{IPEndPoint.Port} :\n" +
                              $"Request:  {BitConverter.ToString(bytes)}...");
#endif

            if (!Connect()) return false;

            lock (Stream)
            {
                if (Stream == null) return false;

                try
                {
#if DEBUGG
                    Console.Write($"Writing {Ip}:{_port})...");
#endif

                    Stream.Write(bytes, 0, bytes.Length);

#if DEBUGG
                    Console.Write("success\n");
#endif
                }
                catch (Exception e)
                {
                    Console.WriteLine($"NetworkProtoCommonDual: Send: write operation fail: {e.Message}!");

                    return false;
                }
            }

            return true;
        }

        public byte[] Get(int count)
        {
            if (!Connect()) return null;

            if (Stream == null || !Stream.CanRead) return null;
            
            var bytes = new byte[count];

#if DEBUGG
            Console.Write($"Reading {Ip}:{_port} {count} bytes...");
#endif

            var nRead = Stream.Read(bytes, 0, count);

#if DEBUGG
            Console.Write("success\n");
#endif

            return nRead != count ? bytes.Take(nRead).ToArray() : bytes;
        }

        public virtual byte[] SendAndGet(byte[] outputBytes, int getCount)
        {
            if (!Send(outputBytes)) return null;

            try
            {
                return Get(getCount);
            }
            catch (TimeoutException e)
            {
                Console.WriteLine($"Response: timeout!\n");
            }
            catch (IOException e)
            {
                Console.WriteLine($"Response: timeout!\n");
            }
            catch (Exception e)
            {
                Console.WriteLine($"EX: SendAndGet:{e.Message}!\n");
            }

            return Array.Empty<byte>();
        }

        public virtual byte[] SendAndGetMany(byte[] outputBytes, int getCount)
        {
            if (!Send(outputBytes)) return null;

            var _try = 5;
            var readCounterSum = 0;
            var readCounter = 0;
            var inputBytes = new byte[getCount];

            while (_try > 0)
            {
                _try--;

                try
                {
                    var resultBuffer = Get(getCount - readCounter);

                    resultBuffer.CopyTo(inputBytes, readCounter);
                    readCounter = resultBuffer.Length;

                    readCounterSum += readCounter;

                    if (readCounterSum >= getCount)
                    {
                        break;
                    }

                    inputBytes = readCounterSum >= getCount ? inputBytes : inputBytes.Take(readCounterSum).ToArray();

                }
                catch (TimeoutException e)
                {
                    Console.WriteLine($"Response: timeout!\n");
                }
                catch (IOException e)
                {
                    Console.WriteLine($"Response: timeout!\n");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"EX: SendAndGet:{e.Message}!\n");
                }

                Thread.Sleep(150);
            }

#if DEBUG
            Console.WriteLine($"Response: {BitConverter.ToString(inputBytes)}\n");
#endif

            return inputBytes;
        }

        public void Dispose()
        {
            Stream?.Dispose();
            Socket?.Close();
            Socket?.Dispose();
        }
    }
}
