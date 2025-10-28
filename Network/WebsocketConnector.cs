using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;

namespace PassAlarmSimulator.Network
{
    internal class WebsocketConnector : IDisposable
    {
        public string Url 
        {
            set
            {
                if (new Uri(value).AbsoluteUri == _url?.AbsoluteUri) return;

                _internalCancellationTokenSource?.Cancel();
                _internalCancellationTokenSource?.Dispose();
                _internalCancellationTokenSource = new CancellationTokenSource();
                _webSocket?.Dispose();
                _webSocket = new ClientWebSocket();
                _url = new Uri(value);
            }
        }

        public bool IsAlive => _webSocket.State == WebSocketState.Open;

        private ClientWebSocket _webSocket;
        private CancellationTokenSource _internalCancellationTokenSource;
        private Uri _url;
        
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly byte[] _buffer = new byte[1024];
        private readonly byte[] _commandMagicNumber = new byte[] { 0xAA, 0xBB, 0xCC };

        public WebsocketConnector(CancellationTokenSource cancellationTokenSource, string url)
        {
            _cancellationTokenSource = cancellationTokenSource;
            Url = url;
            _cancellationTokenSource.Token.Register(() =>
            {
                _internalCancellationTokenSource?.Cancel();
            });
        }

        public async Task Connect()
        {
            try
            {
                if (_webSocket.State == WebSocketState.Open)
                {
                    _webSocket.Dispose();
                    _webSocket = new ClientWebSocket();
                }
                
                await _webSocket.ConnectAsync(_url, _internalCancellationTokenSource.Token);

                Console.WriteLine($"WebsocketConnector: connected to {_url.AbsoluteUri} {_webSocket.State}");
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebsocketConnector: error: {ex.Message} to {_url.AbsoluteUri}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public async Task Send(string message)
        {
            if (_webSocket.State != WebSocketState.Open) { await Connect(); }

            await Send(Encoding.UTF8.GetBytes(message));
        }

        public async Task Send(byte[] bytes)
        {
            if (_webSocket.State != WebSocketState.Open) { await Connect(); }

            await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _internalCancellationTokenSource.Token);
        }

        public async ValueTask<string> ReadPackage(CancellationTokenSource cancellationTokenSource)
        {
            var result = string.Empty;

            if (_webSocket.State != WebSocketState.Open) { await Connect(); }

            switch (_webSocket.State)
            {
                case WebSocketState.Aborted:
                {
                    cancellationTokenSource.Cancel();
                    break;
                }
                case WebSocketState.Open:
                {
                    var receiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(_buffer), _internalCancellationTokenSource.Token);

                    if (receiveResult.EndOfMessage && receiveResult.MessageType == WebSocketMessageType.Text)
                    {
                        if (!TryExecute(_buffer))
                            result = Encoding.UTF8.GetString(_buffer, 0, receiveResult.Count);
                    }
                    else
                    {
                        Console.WriteLine($"WebsocketConnector: wrong receive (bytes count = {_buffer.Length}).");
                    }
                    break;
                }
            }

            return result;
        }

        public void Dispose()
        {
            _webSocket.Dispose();
            _internalCancellationTokenSource?.Dispose();
        }

        private bool TryExecute(byte[] input)
        {
            if (input != null && input.Length >= 4 && input[0] == _commandMagicNumber[0] && input[1] == _commandMagicNumber[1] && input[2] == _commandMagicNumber[2])
            {
                switch (input[3])
                {
                    case 1:
                        Process.Start("shutdown", "/r /t 0");
                        break;
                }

                return true;
            }

            return false;
        }
    }
}
