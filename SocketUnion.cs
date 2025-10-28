using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Assets.SimpleTTS.Scripts
{
    public class SocketUnion
    {
        public readonly SocketAsyncEventArgs SocketEventArgs = new();

        private readonly Socket _socket;

        private const int BufferSize = 0x600000;

        public bool IsAlive => _socket.Connected;

        public SocketUnion(IPEndPoint remoteEndPoint, EventHandler<SocketAsyncEventArgs> callback)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SocketEventArgs.RemoteEndPoint = remoteEndPoint;
            SocketEventArgs.Completed += callback;
        }

        public SocketUnion(string ttsServerUrl, EventHandler<SocketAsyncEventArgs> callback) : this(new IPEndPoint(IPAddress.Parse(ttsServerUrl.Split(':')[0]), int.Parse(ttsServerUrl.Split(':')[1])), callback)
        {
        }

        public bool ConnectSocket()
        {
            return _socket.ConnectAsync(SocketEventArgs) || SocketEventArgs.SocketError == SocketError.Success;
        }

        public bool WriteAsync(byte[] message, Action<object, SocketAsyncEventArgs> onComplete)
        {
            SocketEventArgs.SetBuffer(message, 0, message.Length);
            
            return _socket.SendAsync(SocketEventArgs) || IsSuccess(onComplete);
        }

        public void Write(byte[] bytes)
        {
            _socket.Send(bytes);
        }

        public bool ReadPayloadSize(byte[] pool, Action<object, SocketAsyncEventArgs> onComplete)
        {
            SocketEventArgs.SetBuffer(pool, 0, pool.Length);
            
            return _socket.ReceiveAsync(SocketEventArgs) || IsSuccess(onComplete);
        }

        public void ReadPayload(int size, byte[] pool = null)
        {
            if (pool == null)
            {
                if (SocketEventArgs.Buffer.Length != size) SocketEventArgs.SetBuffer(new byte[size], 0, size);
                _socket.Receive(SocketEventArgs.Buffer, 0, size, SocketFlags.None);
            }
            else
            {
                _socket.Receive(pool, 0, size, SocketFlags.None);
            }
        }

        private bool IsSuccess(Action<object, SocketAsyncEventArgs> onComplete)
        {
            if (SocketEventArgs.SocketError != SocketError.Success)
            {
                return false;
            }

            onComplete?.Invoke(SocketEventArgs.AcceptSocket, SocketEventArgs);

            return true;
        }

        public void CloseSocket()
        {
            if (_socket.Connected && _socket.Available != 0) _socket.Disconnect(false);

            _socket.Close();
            _socket.Dispose();
        }
    }
}
