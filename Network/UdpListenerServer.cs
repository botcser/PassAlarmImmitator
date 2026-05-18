using Extensions;
using IRAPROM.MyCore.Auxiliary;
using IRAPROM.MyCore.DBModel;
using IRAPROM.MyCore.Device;
using IRAPROM.MyCore.Device.Matreshka;
using IRAPROM.MyCore.Model;
using IRAPROM.MyCore.Model.MD;
//using IRAPROM.MyCore.MyEnum;
//using IRAPROM.MyCore.MyNetwork.Observer;
//using MyCore.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
//using PassAlarmSimulator.Validator;

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
            Console.WriteLine($"UDPServer: CreateUdpClient: Listening port = {_port}");
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
        }

        private void ResendToSimulatorUDP(byte[] bytes, string ip, int port)
        {
            UDPSender.Instance.Send(bytes, port, ip);
        }

        private static bool IsSentByServer(IPEndPoint remoteIpEndPoint)
        {
            _localAddresses ??= Dns.GetHostEntry(Dns.GetHostName()).AddressList.ToList();
            _localAddresses.Add(IPAddress.Parse("127.0.0.1"));

            return _localAddresses.Any(i => remoteIpEndPoint.AddressFamily == i.AddressFamily && remoteIpEndPoint.Address.Equals(i))
                   && remoteIpEndPoint.Port != App.Loader_UDPPortRetransmission;
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
                        Console.WriteLine("MatreshkaResponseXPROGOST: EX: Unknow device or device answer parsing error!");
                    }
                    else
                    {
                        Console.WriteLine($"MatreshkaResponseXPROGOST: FoundDevices: {device.IP}");
                        //Validator.FoundDevices.Add(device);
                        //MyARM.Instance.AddedDevicesAddForValidatorOnly(device.MAC, rec.MetDetector);
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