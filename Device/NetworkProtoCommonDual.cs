using Extensions;
using IRAPROM.MyCore.Model;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Device
{
    public class NetworkProtoCommonDual : INetworkProtoDual, IDisposable
    {
        [JsonProperty]
        public string Ip { get; set; }

        internal TcpClient Socket;
        internal NetworkStream Stream;
        
        [JsonProperty]
        private readonly int _port;
        [JsonProperty]
        private readonly int _timeOut = 15000;

        [JsonIgnore]
        internal IPEndPoint IPEndPoint => new IPEndPoint(IPAddress.Parse(Ip), _port);

        public NetworkProtoCommonDual()
        {
            
        }

        public NetworkProtoCommonDual(string ip, int portTCP, int timeOut = 0)
        {
            Ip = ip;
            _port = portTCP;
            _timeOut = timeOut == 0 ? _timeOut : timeOut;
            Socket = new TcpClient();
            Socket.SendTimeout = Socket.ReceiveTimeout = _timeOut;
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
                Socket.SendTimeout = Socket.ReceiveTimeout = _timeOut;

#if DEBUGG
                Console.WriteLine($"new socket {Socket.Client.Handle} connecting {Ip}:{_port} (timeout {5000}ms)...");
#endif
                
                if (!Socket.BeginConnect(Ip, _port, null, null).AsyncWaitHandle.WaitOne(5000, false))
                {
                    Socket.Close();
#if DEBUG
                    Console.WriteLine($"Connection attempt timed out after {5000}ms.");
#endif
                    return false;
                }

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
            var socketHandle = Socket.Client.Handle;

            Socket.Close();
#if DEBUGG
            Console.WriteLine($"close socket {socketHandle} {Ip}:{_port}.");
#endif
        }

        public bool Send(byte[] bytes)
        {
#if DEBUG
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
                    Console.WriteLine($"socket {Socket.Client.Handle} writing {Ip}:{_port} (timeout {Socket.SendTimeout}ms)...");
#endif
                    Stream.Write(bytes, 0, bytes.Length);
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
            Console.WriteLine($"socket {Socket.Client.Handle} reading {Ip}:{_port} (timeout {Socket.ReceiveTimeout}ms)...");
#endif

            var nRead = Stream.Read(bytes, 0, count);

#if DEBUG
            Console.WriteLine($"Response: {BitConverter.ToString(bytes, 0, nRead)}\n");
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
