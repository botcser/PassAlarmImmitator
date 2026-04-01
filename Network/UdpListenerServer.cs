using Extensions;
using IRAPROM.MyCore.Auxiliary;
using IRAPROM.MyCore.DBModel;
using IRAPROM.MyCore.Device;
using IRAPROM.MyCore.Device.Matreshka;
using IRAPROM.MyCore.Model;
using IRAPROM.MyCore.Model.MD;
using PassAlarmSimulator.Validator;
//using IRAPROM.MyCore.MyEnum;
//using IRAPROM.MyCore.MyNetwork.Observer;
//using MyCore.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

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
        }

        public void StartListening()
        {
#if USE_COMMAND_CENTER
            return;
#endif

#if DEBUG
            Console.WriteLine($"UDPServer: CreateUdpClient: Listening port = {_port}\n");
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
            //if (App.LoaderLogMode == enLoaderLogMode.All)
            //{
            //    var str = MyTools.ConvertByteArrayToHexString(response);

            //    File.AppendAllText(App.LoaderFilePathSaveMsg, str + Environment.NewLine);
            //}

            if (MatreshkaResponseXPROGOST(response, remoteIpEndPoint, out var rec))
            {
                DebugResponse("MatreshkaResponseXPROGOST");
            }
            else if(MatreshkaResponse(response, remoteIpEndPoint, out rec))
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

#if USE_DEVICE_SIMULATOR
            if (_port == Device.Matreshka.XGOST.Constants.PortUDPDefault && bytes.Length == Device.Matreshka.XGOST.Constants.FindDatagram.Length)
            {
                ResendToSimulatorUDP(bytes, remoteIpEndPoint.Address.ToString(), _port + 1);
                return;
            }
#else
            if (IsSentByServer(remoteIpEndPoint)) return;
#endif
            Console.WriteLine($"Received: UDP Response: {remoteIpEndPoint.Address.ToString()}\n" +
                              $"\tresponse bytes: {BitConverter.ToString(bytes)}\n");

            ParseResponse(bytes, remoteIpEndPoint);
        }

        private void ResendToSimulatorUDP(byte[] bytes, string ip, int port)
        {
            UDPSender.Instance.Send(bytes, port, ip);
        }
        
        private static bool ImpulseOrCommonResponse(byte[] bytes, MetalDetectPacketInfo rec)
        {
            var rec2 = MonopanelPacketInfo.ParseImpulseMessageUDP(bytes);

            if (rec2 == null) return false;
            
            //if (MyARM.Instance.AddedDevicesTryGetValue(rec2.MAC, out var md, out var onChanged))
            //{
            //    var dopInf = md.GetDopInf();

            //    if (dopInf?.Temperature != null)
            //    {
            //        rec2.Temperature = dopInf.Temperature;
            //    }
            //}

            //if (!App.FreeVersion)
            //{
            //    MDSaveInfoManager.Export(rec2);
            //}

            //if (App.xmlUnloadingRegim == (short)XMLUnloadingRegim.enItems.SaveEachEventInSeparateFile && rec2.ExistAlarm())
            //{
            //    rec2.SaveEventToXML(MDCommands.METDET_CMD_ALARM);
            //}

            var message = $"WorkInf -> MAC:{rec2.MAC}  -  {rec2.NormalInfName}";

            if (rec2.ExistAlarm())
            {
                message += $"         ALARM -> ({rec2.SensorModeName}) - ({rec2.SensorsStrMsg.Trim()})";
            }

            //if (App.LoaderLogMode == enLoaderLogMode.Work)
            //{
            //    var str = MyTools.ConvertByteArrayToHexString(bytes);

            //    File.AppendAllText(App.LoaderFilePathSaveMsg, str + Environment.NewLine);
            //}

            if (App.Loader_UDPPortRetransmission > 0)
            {
                if (Device.Impulse.Constants.CheckImpulseHeader(bytes) || (bytes.Length == 22))
                {
                    UDPSender.Instance.Send(bytes, App.Loader_UDPPortRetransmission);
                }
            }

            //Tracker.Instance.OnGetLogEvent(new LogServer()
            //{
            //    Message = message
            //}, new TrackerEventArg()
            //{
            //    Mac = rec?.MAC ?? string.Empty
            //});

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

            if (MyARM.Instance.AddedDevicesTryGetValue(rec.MAC, out var metDetector, out var onChanged))
            {
                rec.MetDetector = metDetector;

                if (rec.ExistAlarm())
                {
                    metDetector.DeviceMetalDetector.LastPassage = new MetalDetectorPassage(rec.MAC, rec.sensors, rec.logTime, rec.sensorMode);
                }
                else
                {
                    metDetector.DeviceMetalDetector.LastPassage = new MetalDetectorPassage(rec.MAC, rec.logTime, rec.sensorMode, rec.NormalPassNum, rec.AlarmPassNum, rec.NormalReturnNum, rec.AlarmReturnNum);
                }

                onChanged(metDetector);
            }

            UpdateDbInfo(rec);

            if (App.Loader_UDPPortRetransmission > 0)
            {
                UDPSender.Instance.Send(bytes, App.Loader_UDPPortRetransmission);
            }

            return true;
        }

        private static bool MatreshkaResponseXPROGOST(byte[] bytes, IPEndPoint ip, out MetalDetectPacketInfo rec)
        {
            rec = null;

            if (bytes.Length < 0x14)
            {
                return false;
            }

            rec = MetalDetectPacketInfo.ParseXGOSTMatreshkaMessageUDP(bytes, ip);
            
            if (rec == null || rec.ProductModel.IsNullOrEmpty())
            {
                return false;
            }

            rec.deviceFindAnswerNetworkInf = new DeviceFindAnswerNetworkInf()
            {
                IP = rec.Ip,
                Mask = rec.Mask,
                IPGateway = rec.Gateway,
                mac = rec.mac,
                PortTCP = rec.TCPPort,
                PortUDP = rec.UDPPort,
                Model = rec.ProductModel
            };

            MetalDetectPacketInfo.MakeMetalDeviceFromPacketInfo(rec, MetalDetectorSeries.Matryoshka);

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

                    var device = MetalDetectPacketInfo.MakeMetalDeviceFromPacketInfo(rec, MetalDetectorSeries.Matryoshka);

                    if (device == null)
                    {
                        Console.WriteLine("MatreshkaResponse: EX: Unknow device or device answer parsing error!");
                    }
                    else
                    {
                        Console.WriteLine($"MatreshkaResponse: FoundDevices: {device.IP}");
                        Validator.FoundDevices.Add(device);
                        MyARM.Instance.AddedDevicesAddForValidatorOnly(device.MAC, rec.MetDetector);
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

        private static bool MatreshkaResponse(byte[] bytes, IPEndPoint ip, out MetalDetectPacketInfo rec)
        {
            rec = MetalDetectPacketInfo.ParseMatreshkaMessageUDP(bytes);

            if (rec == null || rec.Ip.IsNullOrEmpty())
            {
                return false;
            }

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
                        Console.WriteLine($"MatreshkaResponse: FoundDevices: {device.IP}");
                        Validator.FoundDevices.Add(device);
                        MyARM.Instance.AddedDevicesAddForValidatorOnly(device.MAC, rec.MetDetector);
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
            finally
            {
                UpdateDbInfo(rec);
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

            //Tracker.Instance.OnGetLogEvent(new LogServer()
            //{
            //    Message = message
            //}, new TrackerEventArg()
            //{
            //    Mac = rec.MAC
            //});

            return true;
        }

        private static void UpdateDbInfo(MDSaveInfo rec)
        {
            rec.AddInfToDB();

            //MetDetector.UpdateMainInf(rec.MetDetector);

            //switch (App.xmlUnloadingRegim)
            //{
            //    case (short)XMLUnloadingRegim.enItems.SaveEachEventInSeparateFile:
            //        rec.SaveEventToXML();
            //        //MDSaveInfoManager.WriteSrasu(rec);
            //        break;
            //    default:
            //        break;
            //}
        }
    }
}