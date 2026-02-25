using IRAPROM.MyCore.DBModel;
using IRAPROM.MyCore.Model.MD;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using IRAPROM.MyCore.Auxiliary;
using IRAPROM.MyCore.Device;
using IRAPROM.MyCore.Model;
using PassAlarmSimulator.Validator;

namespace IRAPROM.MyCore.MyNetwork
{
    public class UdpListenerServer
    {
        private static List<IPAddress> _localAddresses;

        private readonly ObservableCollection<string> _lgMsg;
        private readonly UdpClient _udpClient;
        private readonly int _port;
        private Timer _timeoutTimer;

        public UdpListenerServer(int port, ObservableCollection<string> lgMessages)
        {
            _lgMsg = lgMessages ?? new ObservableCollection<string>();
            _port = port;
            _udpClient = new UdpClient(port);
        }
        
        public void StartListening()
        {
#if USE_COMMAND_CENTER
            return;
#endif

            try
            {
                //_timeoutTimer = new Timer(BeginReceiveTimeout, null, 5000, 5000);
                _udpClient.BeginReceive(RequestCallback, new object());
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"StartListening EX: specific SocketException {ex.Message}");
            }
            catch (IOException ex)
            {
                if (ex.InnerException is SocketException innerEx && innerEx.ErrorCode == 10060)
                {
                    Console.WriteLine($"Receive timeout occurred {ex.Message}.");
                }
                else
                {
                    Console.WriteLine($"Receive unknown IOExceptions {ex.Message}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"StartListening EX: {ex.Message}");
            }
        }

        public static void ParseResponse(byte[] response, IPEndPoint remoteIpEndPoint)
        {
            //---------- ответы поиска ---------------
            if (MatreshkaResponse(response, remoteIpEndPoint, out var rec))
            {
                DebugResponse();
            }
            else if (ImpulseNewPackage(response))           // TODO: MB! TEST!
            {
                DebugResponse();
            }
            //---------- проходы ----------------
            else if (ImpulseOrCommonResponse(response, rec))
            {
                DebugResponse();
            }
            else if (response.Length == 22)
            {
                var responseMonopanel = DeviceFindAnswerNetworkInf.GetRecFromPacketMonopanel(response);

                if (responseMonopanel != null)
                {
                    var series = "";

                    switch (remoteIpEndPoint.Port)
                    {
                        case 5015:
                            series = MetalDetectorSeries.Impulse.ToString();
                            NWMessageSrvTools.MakeMetalDeviceFromOldPacketInfo(responseMonopanel, MetalDetectorSeries.Impulse);
                            break;

                        case 9998:
                            series = MetalDetectorSeries.Matryoshka.ToString();
                            NWMessageSrvTools.MakeMetalDeviceFromOldPacketInfo(responseMonopanel, MetalDetectorSeries.Matryoshka);
                            break;

                        default:
                            series = MetalDetectorSeries.Unknown.ToString();
                            NWMessageSrvTools.MakeMetalDeviceFromOldPacketInfo(responseMonopanel, MetalDetectorSeries.Unknown);
                            break;
                    }

#if DEBUGG
                    Console.WriteLine($"Received: Response: {series}\n" +
                                      $"\tresponse bytes: {BitConverter.ToString(response)}\n" +
                                      $"\tIdModel = {responseMonopanel.Model},\n" +
                                      $"\tMAC = {responseMonopanel.MAC},\n" +
                                      $"\tIP = {responseMonopanel.IP},\n" +
                                      $"\tIPGateway = {responseMonopanel.IPGateway},\n" +
                                      $"\tPortUDP = {responseMonopanel.PortUDP},\n" +
                                      $"\tPortTCP = {responseMonopanel.PortTCP},\n" +
                                      $"\tVersion = {responseMonopanel.Version}");
#endif

                    return;
                }
            }

#if DEBUGG
            Console.WriteLine($"Received: response from port {remoteIpEndPoint.Port}!\n" +
                              $"\tresponse bytes: {BitConverter.ToString(response)}\n");
#endif


            void DebugResponse()
            {
#if DEBUGG
                Console.WriteLine($"Received: MatreshkaResponse:\n" +
                                  $"\tresponse bytes: {BitConverter.ToString(response)}\n" +
                                  $"\tIdModel = {rec?.IdModel},\n" +
                                  $"\tMAC = {rec?.MAC},\n" +
                                  $"\tIP = {rec?.deviceFindAnswerNetworkInf?.IP},\n" +
                                  $"\tIPGateway = {rec?.deviceFindAnswerNetworkInf?.IPGateway},\n" +
                                  $"\tPortUDP = {rec?.deviceFindAnswerNetworkInf?.PortUDP},\n" +
                                  $"\tPortTCP = {rec?.deviceFindAnswerNetworkInf?.PortTCP},\n" +
                                  $"\tVersion = {rec?.deviceFindAnswerNetworkInf?.Version}");
#endif
            }
        }
        
        private void RequestCallback(IAsyncResult result)
        {
            try
            {
                Receive(result);
            }
            catch (Exception e)
            {
                _lgMsg?.Add(e.Message);
            }
            finally
            {
                StartListening();
            }
        }

        public void Receive(IAsyncResult result)
        {
            var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            var bytes = _udpClient.EndReceive(result, ref remoteIpEndPoint);

#if !USE_DEVICE_SIMULATOR            
            if (IsSentByServer(remoteIpEndPoint)) return;
#endif
            ParseResponse(bytes, remoteIpEndPoint);
        }

        private static bool ImpulseOrCommonResponse(byte[] bytes, MetalDetectPacketInfo rec)
        {
            var rec2 = MonopanelPacketInfo.ParseImpulseMessageUDP(bytes);

            if (rec2 == null) return false;
            
            var message = $"WorkInf -> MAC:{rec2.MAC}  -  {rec2.NormalInfName}";

            if (rec2.ExistAlarm())
            {
                message += $"         ALARM -> ({rec2.SensorModeName}) - ({rec2.SensorsStrMsg.Trim()})";
            }
           
            if (App.Loader_UDPPortRetransmission > 0)
            {
                if (MetalDetectPacketInfo.CheckMatreshkaHeader(bytes) || (bytes.Length == 22))
                {
                    UDPSender.Instance.Send(bytes, App.Loader_UDPPortRetransmission);
                }
            }

            return true;
        }

        private static bool IsSentByServer(IPEndPoint remoteIpEndPoint)
        {
            _localAddresses ??= Dns.GetHostEntry(Dns.GetHostName()).AddressList.ToList();
            _localAddresses.Add(IPAddress.Parse("127.0.0.1"));

            return _localAddresses.Any(i => remoteIpEndPoint.AddressFamily == i.AddressFamily && remoteIpEndPoint.Address.Equals(i))
                   && remoteIpEndPoint.Port != App.Loader_UDPPortRetransmission;
        }

        private static bool ImpulseNewPackage(byte[] bytes)
        {
            if (bytes.Length != 31)
            {
                return false;
            }

            var rec = ImpulsPacketInfo.PacketToInfo(bytes);

            if (rec == null)
            {
                return false;
            }

            if (App.Loader_UDPPortRetransmission > 0)
            {
                UDPSender.Instance.Send(bytes, App.Loader_UDPPortRetransmission);
            }

            return true;
        }

        private static bool MatreshkaResponse(byte[] bytes, IPEndPoint ip, out MetalDetectPacketInfo rec)
        {
            rec = MetalDetectPacketInfo.ParseMatreshkaMessageUDP(bytes);

            if (rec == null) return false;
            
            var message = "";

            try
            {
                switch (rec.command)
                {
                    case MDCommands.METDET_CMD_NORMAL_GET_PASSAGES:
                    {
                        Console.WriteLine($"METDET_CMD_NORMAL_GET_PASSAGES: {BitConverter.ToString(bytes)}");

                        message = $"WorkInf -> MAC:{rec.MAC}  -  {rec.NormalInfName}";

                        break;
                    }
                    case MDCommands.METDET_CMD_ALARM:
                    {
                        Console.WriteLine($"METDET_CMD_ALARM: {BitConverter.ToString(bytes)}");

                        message = $"ALARM -> MAC:{rec.MAC} ({rec.SensorModeName}) - ({rec.SensorsStrMsg.Trim()})";

                        break;
                    }
                    case MDCommands.METDET_CMD_FINDANSWER:
                    {
                        message = $"FindAnswer -> MAC:{rec.MAC}  -  IP: {rec.deviceFindAnswerNetworkInf.IP}" +
                                  $"  Mask: {rec.deviceFindAnswerNetworkInf.Mask}" +
                                  $"  TCP-port: {rec.deviceFindAnswerNetworkInf.PortTCP}" +
                                  $"  UDP-port: {rec.deviceFindAnswerNetworkInf.PortUDP}" +
                                  $"  Gateway: {rec.deviceFindAnswerNetworkInf.IPGateway}";

                        message += $"  Модель: {rec.deviceFindAnswerNetworkInf.Model}  Версия: {rec.deviceFindAnswerNetworkInf.Version}";

                        DeviceMetalDetector device;

                        switch (+ip.Port)
                        {
                            case 9998:
                                device = MetalDetectPacketInfo.MakeMetalDeviceFromPacketInfo(rec, MetalDetectorSeries.Matryoshka);
                                break;

                            default:
                                device = MetalDetectPacketInfo.MakeMetalDeviceFromPacketInfo(rec, MetalDetectorSeries.BlockPost);
                                break;
                        }

                        if (device == null)
                        {
                            Console.WriteLine("MatreshkaResponse: EX: Unknow device or device answer parsing error!");
                        }
                        else
                        {
                            Validator.FoundDevices.Add(device);
                        }

                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            //if (App.Loader_UDPPortRetransmission > 0)
            //{
            //    if (MetalDetectPacketInfo.CheckMatreshkaHeader(bytes))
            //    {
            //        switch (rec.command)
            //        {
            //            case MDCommands.METDET_CMD_ALARM:
            //            case MDCommands.METDET_CMD_NORMAL_GET_PASSAGES:
            //            case MDCommands.METDET_CMD_FINDANSWER:
            //                UDPSender.Instance.Send(bytes, App.Loader_UDPPortRetransmission);
            //                break;
            //        }
            //    }
            //}
            
            return true;
        }
    }
}