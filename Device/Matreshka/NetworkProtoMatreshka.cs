using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Extensions;
using Newtonsoft.Json;

namespace IRAPROM.MyCore.Device.Matreshka
{
    public class NetworkProtoMatreshka : INetworkProtoDual, IDisposable
    {
        private const int HeaderSize = 8;

        [JsonProperty]
        public string Ip { get; set; }
        [JsonProperty]
        public int PortTCP { get; set; }

        internal TcpClient Socket;
        internal NetworkStream Stream;

        [JsonProperty]
        private readonly int _port;
        [JsonProperty]
        public int Timeout { get; set; } = 5000;

        [JsonIgnore]
        internal IPEndPoint IPEndPoint => new IPEndPoint(IPAddress.Parse(Ip), _port);

        public NetworkProtoMatreshka() { }

        public NetworkProtoMatreshka(string ip, int portTCP, int timeOut = 0)
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
//#if DEBUGG
//                        Console.WriteLine($"disposing socket {(Socket.Client == null ? "closed" : Socket.Client.Handle.ToString())} {Ip}:{_port}...");
//#endif
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
                Console.Write("success\n");
#endif

                if (!Socket.Connected) return false;

                Stream = Socket.GetStream();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX: NetworkProtoMatreshka: Connect: {IPEndPoint.Address}:{IPEndPoint.Port} - {ex.Message}");

                return false;
            }
        }

        public void Disconnect()
        {
            var socketHandle = Socket.Client.Handle;

            Socket.Close();
#if DEBUGG
            Console.WriteLine($"close socket {socketHandle} {Ip}:{_port}.");
#endif
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
                    Console.Write($"Writing {Ip}:{_port}..");
#endif

                    Stream.Write(bytes, 0, bytes.Length);

#if DEBUGG
                    Console.WriteLine("success");
#endif
                }
                catch (Exception e)
                {
                    Console.WriteLine($"NetworkProtoMatreshka: Send: write operation fail: {e.Message}!");

                    return false;
                }
            }

            return true;
        }

        public byte[] Get(int _)
        {
            if (!Connect()) return null;

            if (Stream == null || !Stream.CanRead) return null;

#if DEBUGG
            Console.WriteLine($"Reading {Ip}:{_port} HeaderSize {HeaderSize} bytes...");
#endif

            var headerBytes = new byte[HeaderSize];
            var nRead = Stream.Read(headerBytes, 0, HeaderSize);

            if (nRead != HeaderSize)
            {
#if DEBUGG
                Console.WriteLine($"Reading {Ip}:{_port} Matreshka header first 8 bytes error!!!");
#endif
                return Array.Empty<byte>();
            }

            var frameLength = headerBytes[4];
            var bytes = new byte[frameLength];

            nRead = Stream.Read(bytes, 0, frameLength);

            if (nRead != frameLength)
            {
#if DEBUGG
                Console.WriteLine($"Reading {Ip}:{_port} frameLength bytes error!!!");
#endif
                return Array.Empty<byte>();
            }

#if DEBUGG
            Console.WriteLine($"Response: {BitConverter.ToString(bytes, 0, nRead)}\n");
#endif

            return headerBytes.Concat(bytes).ToArray();
        }

        public virtual byte[] SendAndGet(byte[] outputBytes, int getCount)
        {
            if (!Send(outputBytes)) return null;

            try
            {
                return Get(0);
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
        
        public void Dispose()
        {
            Stream?.Dispose();
            Socket?.Close();
            Socket?.Dispose();
        }
    }
}
