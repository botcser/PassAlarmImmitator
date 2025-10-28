using CommandTransmitter.Device;
using Device;
using PassAlarmSimulator.Network;

namespace Extensions
{
    internal class WebProxyClient :IDisposable
    {

        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource _internalCancellationTokenSource;
        private readonly NetworkCommandsTransmitter _networkCommandsTransmitter;
        private readonly TimeSpan _networkOperationsTimeout = TimeSpan.FromSeconds(9);
        private readonly TimeSpan _workOperationsTimeout = TimeSpan.FromSeconds(1);

        private string Url = 1 == 0 ? UrlDebug : UrlRelease;
        private const string UrlDebug = "localhost:56126";
        //private const string UrlRelease = "commandcenter.somee.com";
        private const string UrlRelease = "commandcenter.runasp.net";

        private DateTime _lastFileWrite;

        public WebProxyClient(CancellationTokenSource cancellationTokenSource, NetworkCommandsTransmitter networkCommandsTransmitter)
        {
            _cancellationTokenSource = cancellationTokenSource;
            _networkCommandsTransmitter = networkCommandsTransmitter; 
            _internalCancellationTokenSource = new CancellationTokenSource();
        }

        public async Task<bool> Run()
        {
            try
            {
                await File.WriteAllTextAsync("c:\\Users\\Proger\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\alive.txt", DateTime.UtcNow.Ticks.ToString());

                do
                {
                    _networkCommandsTransmitter.Url = Url;

                    if (!_networkCommandsTransmitter.IsRunning && !_networkCommandsTransmitter.IsAlive)
                    {
                        Task.Run(() => _networkCommandsTransmitter.Run(), _internalCancellationTokenSource.Token);

                        await Task.Delay(_networkOperationsTimeout);
                    }

                    if (!CommandExecutor.IsBusy && _networkCommandsTransmitter.HasRequest)
                    {
                        _networkCommandsTransmitter.Response = CommandExecutor.ExecuteCommonCommand(_networkCommandsTransmitter.Request);
                    }

                    if (_networkCommandsTransmitter.HasResponse)
                    {
                        _networkCommandsTransmitter.SendResponse(_networkCommandsTransmitter.Response);
                    }

                    if (_networkCommandsTransmitter.IsAlive)
                    {
                        await Task.Delay(_workOperationsTimeout);
                    }
                    else
                    {
                        await Task.Delay(_networkOperationsTimeout);
                    }

                    if (_networkCommandsTransmitter.IsRunning && _networkCommandsTransmitter.IsAlive && (DateTime.UtcNow - _lastFileWrite).TotalSeconds > 5)
                     {
                        await File.WriteAllTextAsync("c:\\Users\\Proger\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\alive.txt", DateTime.UtcNow.Ticks.ToString());
                        _lastFileWrite = DateTime.UtcNow;
                    }
                } while (!_cancellationTokenSource.IsCancellationRequested);
            }
            catch (Exception e)
            {
                Console.WriteLine($"EX: WebProxyClient: {e.Message}!");
                return false;
            }
            finally
            {
                await Task.Delay(100);
            }

            return true;
        }

        public void Dispose()
        {
            _internalCancellationTokenSource?.Dispose();
            _networkCommandsTransmitter?.Dispose();
        }
    }
}
