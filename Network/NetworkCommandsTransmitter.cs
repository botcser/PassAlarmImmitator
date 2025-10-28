using Assets.SimpleTTS.Scripts;
using Casualbunker.Server.Common;
using Device;
using Extensions;

namespace PassAlarmSimulator.Network
{
    internal class NetworkCommandsTransmitter : IDisposable
    {
        public bool IsRunning;

        private const string CommandCenterEndPoint = "api/CommandCenter/ConnectWorker/password=qwerty123$";

        public string Url
        {
            set
            {
                if (value == _url) return;

                _url = value;

                if (_websocketConnector != null) _websocketConnector.Url = $"ws://{value}/{CommandCenterEndPoint}";
            }
            get => _url;
        }

        public bool IsAlive => _websocketConnector?.IsAlive ?? false;
        public bool HasRequest => _requests.Count > 0;
        public bool HasResponse => _responses.Count > 0;

        private string _url = "localhost:56126";

        public Command Request
        {
            get
            {
                if (_requests.Count == 0) return null;

                var request = _requests[0];

                _requests.Remove(request);

                return request;
            }
        }

        public Command Response
        {
            set => _responses.Add(value);
            get
            {
                if (_responses.Count == 0) return null;

                var response = _responses[0];

                _responses.Remove(response);

                return response;
            }
        }

        public readonly CancellationTokenSource CancellationSourceToken;

        private readonly TimeSpan _connectionTimeout = TimeSpan.FromSeconds(6);
        private readonly TimeSpan _operationTimeout = TimeSpan.FromSeconds(1);
        private readonly List<Command> _requests = new List<Command>();
        private readonly List<Command> _responses = new List<Command>();

        private SocketUnion _socket;
        private WebsocketConnector _websocketConnector;
        private CancellationTokenSource _internalCancellationSourceToken;
        private bool _isListening;


        public NetworkCommandsTransmitter(CancellationTokenSource cancellationSourceToken)
        {
            CancellationSourceToken = cancellationSourceToken;
            CancellationSourceToken.Token.Register(Dispose);
        }

        public async Task Run()
        {
            try
            {
                IsRunning = true;

                _websocketConnector = new WebsocketConnector(CancellationSourceToken, $"ws://{Url}/{CommandCenterEndPoint}");

                do
                {
                    if (!_websocketConnector.IsAlive)
                    {
                        _websocketConnector.Url = $"ws://{Url}/{CommandCenterEndPoint}";
                        await _websocketConnector.Connect();
                    }

                    if (!_websocketConnector.IsAlive)
                    {
                        _websocketConnector.Url = $"wss://{Url}/{CommandCenterEndPoint}";
                        await _websocketConnector.Connect();
                    }

                    if (_websocketConnector.IsAlive && !_isListening)
                    {
                        _internalCancellationSourceToken?.Cancel();
                        _internalCancellationSourceToken = new CancellationTokenSource();
                        Task.Run(Listen);
                    }

                    await Task.Delay(_connectionTimeout);
                } while (!CancellationSourceToken.IsCancellationRequested);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX: NetworkCommandsTransmitter: {ex.Message}: {Url}");
            }

            IsRunning = false;
        }

        private async Task Listen()
        {
            _isListening = true;

            do
            {
                try
                {
                    var request = await _websocketConnector.ReadPackage(_internalCancellationSourceToken);
                    
                    if (!request.IsNullOrEmpty())
                    {
                        _requests.Add(JsonGeneric.FromJson<Command>(request));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            } while (!_internalCancellationSourceToken.IsCancellationRequested);

            _isListening = false;
        }
        
        public void SendResponse(Command response)
        {
            try
            {
                if (_websocketConnector.IsAlive)
                {
                    Task.Run(() => _websocketConnector.Send(JsonGeneric.ToJson(response)));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"EX: SendResponse: {e.Message}!");
            }
        }

        public void Dispose()
        {
            if (_internalCancellationSourceToken != null)
            {
                if (!_internalCancellationSourceToken.IsCancellationRequested)
                {
                    _internalCancellationSourceToken.Cancel();
                    _internalCancellationSourceToken?.Dispose();
                }
            }

            if (!CancellationSourceToken.IsCancellationRequested)
            {
                CancellationSourceToken?.Dispose();
            }

            _websocketConnector?.Dispose();
            IsRunning = false;
        }

        private void DebugReportCommand(Command command)
        {
            Console.WriteLine($"\n");

            Console.WriteLine($"\"request: {Convert.ToHexString(command.DatagramRequest)}");
            Console.WriteLine($"\"response: {Convert.ToHexString(command.GetResponse(0))}");
            Console.WriteLine($"\"port: {command.ResponsePorts[0]}");

            Console.WriteLine($"\n");
        }
    }
}
