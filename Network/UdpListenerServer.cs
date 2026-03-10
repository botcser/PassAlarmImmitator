using IRAPROM.MyCore.Auxiliary;
using IRAPROM.MyCore.DBModel;
using IRAPROM.MyCore.Device;
using IRAPROM.MyCore.Model;
using IRAPROM.MyCore.Model.MD;
using PassAlarmSimulator.Validator;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using IRAPROM.MyCore.Device.Matreshka;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            _udpClient.EnableBroadcast = true;

            Console.WriteLine($"Validator: listening ports: {_port}...");
        }

        public void StartListening()
        {
#if USE_COMMAND_CENTER
            return;
#endif

            var error = false;
//            Task.Run(() =>
//            {
//                do
//                {
//                    try
//                    {
//                        var remoteIPEndPoint = new IPEndPoint(IPAddress.Any, _port);
//#if DEBUGG
//                        Console.WriteLine($"Listening from {remoteIPEndPoint.Address}:{remoteIPEndPoint.Port}!\n");
//#endif
//                        var bytes = _udpClient.Receive(ref remoteIPEndPoint);


//#if DEBUGG
//                        Console.WriteLine($"Received: response from {remoteIPEndPoint.Address}:{remoteIPEndPoint.Port}!\n" +
//                                          $"\tresponse bytes: {BitConverter.ToString(bytes)}\n");
//#endif

//                        if (IsSentByServer(remoteIPEndPoint)) continue;

//                        ParseResponse(bytes, remoteIPEndPoint);
//                    }
//                    catch (SocketException ex)
//                    {
//                        Console.WriteLine($"StartListening EX: specific SocketException {ex.Message}");
//                        error = true;
//                    }
//                    catch (IOException ex)
//                    {
//                        if (ex.InnerException is SocketException innerEx && innerEx.ErrorCode == 10060)
//                        {
//                            Console.WriteLine($"Receive timeout occurred {ex.Message}.");
//                        }
//                        else
//                        {
//                            Console.WriteLine($"Receive unknown IOExceptions {ex.Message}.");
//                        }
//                        error = true;
//                    }
//                    catch (Exception ex)
//                    {
//                        Console.WriteLine($"StartListening EX: {ex.Message}");
//                        error = true;
//                    }
//                } while (!error);
//            });
//           return;

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
                DebugResponse("MatreshkaResponse");
            }
            else if (ImpulseNewPackage(response))           // TODO: MB! TEST!
            {
                DebugResponse("ImpulseNewResponse");
            }
            //---------- проходы ----------------
            else if (ImpulseOrCommonResponse(response, rec))
            {
                DebugResponse("ImpulseOrCommonResponse");
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
                    Console.WriteLine($"Received: old Response: {series}\n" +
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



            void DebugResponse(string sourceName)
            {
#if DEBUGG
                Console.WriteLine($"Received: {sourceName}:\n" +
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
#if DEBUGG
                Console.WriteLine($"Received: something on port {_port}...");
#endif
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
            
#if DEBUGG
            Console.WriteLine($"Received: response from port {remoteIpEndPoint.Port}!\n" +
                              $"\tresponse bytes: {BitConverter.ToString(bytes)}\n");
#endif

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

//#if REMOTE_DEBUG
//            Console.WriteLine($"_localAddresses :\n");
//            _localAddresses.ForEach(i =>
//            {
//                Console.WriteLine($"\t{i}!\n");
//            });

//            Console.WriteLine($"\tremoteIpEndPoint.AddressFamily {remoteIpEndPoint.AddressFamily}!\n");
//            Console.WriteLine($"\tremoteIpEndPoint.Address {remoteIpEndPoint.Address}!\n");
//#endif

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
            
            var message = $"rec.command = {rec.command}\n";

            try
            {
                if (Constants.FindAnswerCodes.Contains(rec.command))
                {
                    message += $"FindAnswer -> MAC:{rec.MAC}  -  IP: {rec.deviceFindAnswerNetworkInf.IP}" +
                               $"  Mask: {rec.deviceFindAnswerNetworkInf.Mask}" +
                               $"  TCP-port: {rec.deviceFindAnswerNetworkInf.PortTCP}" +
                               $"  UDP-port: {rec.deviceFindAnswerNetworkInf.PortUDP}" +
                               $"  Gateway: {rec.deviceFindAnswerNetworkInf.IPGateway}";

                    message += $"  Модель: {rec.deviceFindAnswerNetworkInf.Model}  Версия: {rec.deviceFindAnswerNetworkInf.Version}";

                    DeviceMetalDetector device;

                    if (Constants.PortUDPDefault == ip.Port || DeviceMetalDetector.FamilyInfoVariants[0].PortUDPAdditional == ip.Port)
                    {
                        device = MetalDetectPacketInfo.MakeMetalDeviceFromPacketInfo(rec, MetalDetectorSeries.Matryoshka);
                    }
                    else
                    {
                        switch (+ip.Port)
                        {
                            case 9998:
                                device = MetalDetectPacketInfo.MakeMetalDeviceFromPacketInfo(rec, MetalDetectorSeries.Matryoshka);
                                break;
                            case 1021:
                                device = MetalDetectPacketInfo.MakeMetalDeviceFromPacketInfo(rec, MetalDetectorSeries.Matryoshka);
                                break;

                            default:
                                device = MetalDetectPacketInfo.MakeMetalDeviceFromPacketInfo(rec, MetalDetectorSeries.BlockPost);
                                break;
                        }
                    }

                    if (device == null)
                    {
                        Console.WriteLine("MatreshkaResponse: EX: Unknow device or device answer parsing error!");
                    }
                    else
                    {
                        Validator.FoundDevices.Add(device);
                    }
                }
                else
                {
                    switch (rec.command)
                    {
                        case MDCommands.METDET_CMD_NORMAL_GET_PASSAGES:
                        {
                            Console.WriteLine($"METDET_CMD_NORMAL_GET_PASSAGES: {BitConverter.ToString(bytes)}");

                            message += $"WorkInf -> MAC:{rec.MAC}  -  {rec.NormalInfName}";

                            break;
                        }
                        case MDCommands.METDET_CMD_ALARM:
                        {
                            Console.WriteLine($"METDET_CMD_ALARM: {BitConverter.ToString(bytes)}");

                            message += $"ALARM -> MAC:{rec.MAC} ({rec.SensorModeName}) - ({rec.SensorsStrMsg.Trim()})";

                            break;
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            if (App.Loader_UDPPortRetransmission > 0)
            {
                if (MetalDetectPacketInfo.CheckMatreshkaHeader(bytes))
                {
                    switch (rec.command)
                    {
                        case MDCommands.METDET_CMD_ALARM:
                        case MDCommands.METDET_CMD_NORMAL_GET_PASSAGES:
                        case MDCommands.METDET_CMD_FINDANSWER:
                            UDPSender.Instance.Send(bytes, App.Loader_UDPPortRetransmission);
                            break;
                    }
                }
            }

            return true;
        }
    }
}