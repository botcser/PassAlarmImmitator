using IRAPROM.MyCore.Model.WP;
using PassAlarmSimulator.Device;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
//using PassAlarmSimulator.Validator;

namespace IRAPROM.MyCore.Device.Matreshka.XGOST
{
    public class WorkParamsProto : CommandExecutor, IWorkParamsProto, ITestsProto
    {
        private readonly int _requestDelay = TimeSpan.FromMilliseconds(150).Milliseconds;

        public WorkParamsProto(INetworkProtoDual networkProto, IDatagramProto datagramProto, List<(short, short, int, string)> getCommands, List<(short, short, int, string)> setCommands) : base(networkProto, datagramProto, getCommands, setCommands)
        {
        }

        public WorkParamsProto(IDatagramProto datagramProto, List<(short, short, int, string)> getCommands, List<(short, short, int, string)> setCommands) : base(datagramProto, getCommands, setCommands)
        {
        }

        public WorkParams GetWorkParams()
        {
            var workParams = new WorkParams
            {
                ModelId = (byte)Constants.Model.UnknownMatreshka
            };

            try
            {
                InitZonesSensitivity(workParams);
                InitNetworkParams(workParams);
                InitBaseSensitivity(workParams);
                InitWorkFrequency(workParams);
                InitAlarmParams(workParams);
                InitZonesWorkMode(workParams);

                InitOperatorPassword(workParams);
                InitFirmwareVersion(workParams);
                InitSerialNumber(workParams);
                InitTime(workParams);
                InitPassageCount(workParams);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                NetworkProto.Disconnect();
            }

            return workParams;
        }

        public bool SetWorkParams(WorkParams workParams)
        {
            SetWorkProgramScene(workParams);
            Thread.Sleep(_requestDelay);
            
            SetWorkProgramScene(workParams);
            Thread.Sleep(_requestDelay);

            SetZonesSensitivity(workParams);
            Thread.Sleep(_requestDelay);
            
            SetBaseSensitivity(workParams);
            Thread.Sleep(_requestDelay);
            
            SetWorkFrequency(workParams);
            Thread.Sleep(_requestDelay);
            
            SetAlarmParams(workParams);
            Thread.Sleep(_requestDelay);

            if (NetworkProto.Ip != workParams.IP || NetworkProto.PortTCP != workParams.PortTCP)
            {
                SetNetworkParams(workParams);
            }

            NetworkProto.Disconnect();

            return true;
        }
        
        public bool StaticTest(WorkParams workParams)
        {
            const byte testValue = 0x02;

            return BaseSensitivityTest(workParams, testValue) && ZonesSensitivityTest(workParams, testValue) && WorkingFreqTest(workParams) &&
                   WorkProgramSceneTest(workParams, testValue) && AlarmParamsTest(workParams, testValue) && OperatorPasswordTest(workParams, 4321) && 
                   ClearPassageTest(workParams) && TimeTest(workParams, new DateTime(2026, 2, 2, 2, 2, 2)) && NetworkTest(workParams)
                   /*&& RestoreSettingsTest(workParams)*/;
        }
        
        public void HandTest(WorkParams workParams)
        {
            byte testValue = 0x09;

            workParams.WorkProgram = testValue;
            SetWorkProgramScene(workParams);
            Thread.Sleep(_requestDelay);

            workParams.SensorsSensitivity = new[]
            {
                (short)testValue, (short)testValue, (short)testValue, (short)testValue, (short)testValue,
                (short)testValue,
                (short)testValue, (short)testValue, (short)testValue, (short)testValue, (short)testValue,
                (short)testValue,
            };
            SetZonesSensitivity(workParams);
            Thread.Sleep(_requestDelay);

            workParams.BaseSensitivity = testValue;
            SetBaseSensitivity(workParams);
            Thread.Sleep(_requestDelay);

            do
            {
                workParams.WorkingFreq = (byte)new Random().Next(51);
            } while (workParams.WorkingFreq % 3 != 0);

            SetWorkFrequency(workParams);
            Thread.Sleep(_requestDelay);

            workParams.AlarmDuration = testValue;
            workParams.AlarmVolume = testValue;
            workParams.AlarmTone = testValue;
            SetAlarmParams(workParams);
            Thread.Sleep(_requestDelay);

            ClearPassageCount();
        }

        public bool DynamicTest(WorkParams workParams, int milliSecondsTimeout, bool alarm)
        {
            if (alarm)
            {
                SimulateAlarm();
            }
            else
            {
                SimulatePass();
            }

            NetworkProto.Disconnect();

            return true;
        }

        private bool ValidatePassages(MetalDetectorPassage alarmPassage, MetalDetectorPassage lastPassage)
        {
            throw new NotImplementedException();
        }

        public void CallPassage()
        {
            ExecuteCommonCommand(new Command(DatagramProto.MakeRequestDatagram(Constants.SimulatePass.deviceCode), Constants.SimulatePass.code, "127.0.0.1", Constants.PortTCPDefault.ToString(), ProtocolType.Tcp));
        }

        public void CallAlarm()
        {
            ExecuteCommonCommand(new Command(DatagramProto.MakeRequestDatagram(Constants.SimulatePass.deviceCode), Constants.SimulatePass.code, "127.0.0.1", Constants.PortTCPDefault.ToString(), ProtocolType.Tcp));
        }

        public void InitZonesSensitivity(WorkParams workParams)
        {
            try
            {
                var response = ExecuteGetCommand(Constants.GetZonesSensitivity.code);

                workParams.SensorsSensitivity = new short[response.Length / 2];

                for (byte i = 0; i < response.Length; i += 2)
                {
                    workParams.SensorsSensitivity[i / 2] = (short)(response[i] + ((short)response[i + 1] << 8));
                }
            }
            catch (Exception _)
            {
                Console.WriteLine($"ERROR: InitZonesSensitivity: unknown coils count of device {workParams.IP}:{workParams.MAC}!");
                return;
            }
        }

        public void InitNetworkParams(WorkParams workParams)
        {
            var response = ExecuteGetCommand(Constants.GetNetworkParams.code);
            //var command = ExecuteCommonCommand(new Command(DatagramProto.MakeRequestDatagram(Constants.SetNetworkParams.deviceCode), Constants.SetNetworkParams.code, workParams.IP, workParams.PortTCP.ToString(), ProtocolType.Tcp));
            //var response = command.Result;

            using (var ms = new MemoryStream(response))
            {
                using (var br = new BinaryReader(ms))
                {
                    var arIP = br.ReadBytes(4);
                    var arMask = br.ReadBytes(4);
                    var arIPGateway = br.ReadBytes(4);

                    workParams.IP = $"{arIP[0]}.{arIP[1]}.{arIP[2]}.{arIP[3]}";
                    workParams.Mask = $"{arMask[0]}.{arMask[1]}.{arMask[2]}.{arMask[3]}";
                    workParams.Gateway = $"{arIPGateway[0]}.{arIPGateway[1]}.{arIPGateway[2]}.{arIPGateway[3]}";
                    workParams.PortTCP = br.ReadInt16();
                    workParams.PortUDP = br.ReadInt16();
                    workParams.MAC = Convert.ToHexString(br.ReadBytes(6));
                }
            }
        }

        public void InitBaseSensitivity(WorkParams workParams)
        {
             workParams.BaseSensitivity = ExecuteGetCommand(Constants.GetBaseSensitivity.code).FirstOrDefault();
        }

        public void InitWorkFrequency(WorkParams workParams)
        {
            workParams.WorkingFreq = ExecuteGetCommand(Constants.GetWorkFrequency.code).FirstOrDefault();
        }

        public void InitAlarmParams(WorkParams workParams)
        {
            var response = ExecuteGetCommand(Constants.GetAlarmParams.code);

            workParams.AlarmDuration = response[0];
            workParams.AlarmVolume = response[1];
            workParams.AlarmTone = response[2];
        }

        public void InitOperatorPassword(WorkParams workParams)
        {
            var bytes = ExecuteGetCommand(Constants.GetPassword.code);

            workParams.Password = BitConverter.ToInt32(bytes, 0);
        }

        public void InitFirmwareVersion(WorkParams workParams)
        {
            var response = ExecuteGetCommand(Constants.GetFirmwareVersion.code);

            workParams.FirmwareVersion = Encoding.ASCII.GetString(response);
        }

        public void InitSerialNumber(WorkParams workParams)
        {
            var response = ExecuteGetCommand(Constants.GetSerialNumber.code);

            workParams.SerialNumber = Convert.ToHexString(response);
        }

        public void InitTime(WorkParams workParams)
        {
            var response = ExecuteGetCommand(Constants.GetTime.code);

            var ye = 2000 + response[0];
            var mo = response[1];
            var da = response[2];

            var ho = response[3];
            var mi = response[4];
            var se = response[5];

            workParams.DateTime = new DateTime(ye, mo, da, ho, mi, se);
        }

        public void InitZonesWorkMode(WorkParams workParams)
        {
            var response = ExecuteGetCommand(Constants.GetZonesWorkMode.code);

            workParams.ZonesSensorMode = response[0];
            workParams.WorkProgram = response[1];
            workParams.InfraredPassCounterMode = response[2];
        }

        public void InitPassageCount(WorkParams workParams)
        {
            var response = ExecuteGetCommand(Constants.GetPassageCount.code);

            workParams.ForwardPassageCount = BitConverter.ToUInt32(response, 0);
            workParams.BackwardPassageCount = BitConverter.ToUInt32(response, 4);
            workParams.ForwardAlarmsCount = BitConverter.ToUInt32(response, 8);
            workParams.BackwardAlarmsCount = BitConverter.ToUInt32(response, 12);
        }

        public void RestoreSettings(WorkParams workParams)
        {
            ExecuteSetCommandRaw(Constants.ResetSettings.code, new byte[] { 4, 0x00 });
            Thread.Sleep(_requestDelay);
            ExecuteSetCommandRaw(Constants.ResetSettings.code, new byte[] { 8, 0x00 });
            Thread.Sleep(_requestDelay);
            ExecuteSetCommandRaw(Constants.ResetSettings.code, new byte[] { 16, 0x00 });
            Thread.Sleep(_requestDelay);
            ExecuteSetCommandRaw(Constants.ResetSettings.code, new byte[] { 32, 0x00 });
            Thread.Sleep(_requestDelay);
            ExecuteSetCommandRaw(Constants.ResetSettings.code, new byte[] { 64, 0x00 });
            Thread.Sleep(_requestDelay);
            ExecuteSetCommandRaw(Constants.ResetSettings.code, new byte[] { 128, 0x00 });
            Thread.Sleep(_requestDelay);
            ExecuteSetCommandRaw(Constants.ResetSettings.code, new byte[] { 0x00, 0x01 });
            Thread.Sleep(_requestDelay);
            ExecuteSetCommandRaw(Constants.ResetSettings.code, new byte[] { 0x00, 0x02 });
            Thread.Sleep(_requestDelay);
            ExecuteSetCommandRaw(Constants.ResetSettings.code, new byte[] { 0x02, 0x00 });

            NetworkProto.Disconnect();
            //ExecuteSetCommandRaw(Constants.ResetSettings.code, new byte[] { 0x00, 4 });
            //Thread.Sleep(_requestDelay);
            //ExecuteSetCommandRaw(Constants.ResetSettings.code, new byte[] { 0x00, 8 });
            //Thread.Sleep(_requestDelay);
            //ExecuteSetCommandRaw(Constants.ResetSettings.code, new byte[] { 0x00, 16 });
            //Thread.Sleep(_requestDelay);
            //ExecuteSetCommandRaw(Constants.ResetSettings.code, new byte[] { 0x00, 32 });
            //Thread.Sleep(_requestDelay);
            //ExecuteSetCommandRaw(Constants.ResetSettings.code, new byte[] { 0x00, 64 });
            //Thread.Sleep(_requestDelay);
            //ExecuteSetCommandRaw(Constants.ResetSettings.code, new byte[] { 0x00, 128 });
            //Thread.Sleep(_requestDelay);
            //ExecuteSetCommandRaw(Constants.ResetSettings.code, new byte[] { 0x01, 0x00 });
            //Thread.Sleep(_requestDelay);
            //ExecuteSetCommandRaw(Constants.ResetSettings.code, new byte[] { 0x02, 0x00 });
            //Thread.Sleep(_requestDelay);
            //ExecuteSetCommandRaw(Constants.ResetSettings.code, new byte[] { 0x00, 0x02 });
        }

        public void RebootDevice(WorkParams workParams)
        {
            ExecuteSetCommandRaw(Constants.RebootDevice.code, new byte[]{});
        }

        public void SetZonesSensitivity(WorkParams workParams)
        {
            ExecuteSetCommandRaw(Constants.SetZonesSensitivity.code, workParams.SensorsSensitivity.SelectMany(BitConverter.GetBytes).ToArray());
        }

        public void SetBaseSensitivity(WorkParams workParams)
        {
            var bytes = BitConverter.GetBytes(workParams.BaseSensitivity);
            var byte0 = bytes[0];
            var byte1 = bytes[1];

            ExecuteSetCommandRaw(Constants.SetBaseSensitivity.code, new [] { byte0, byte1 });
        }

        public void SetWorkFrequency(WorkParams workParams)
        {
            ExecuteSetCommandRaw(Constants.SetWorkFrequency.code, new[] { (byte)workParams.WorkingFreq });
        }

        public void SetAlarmParams(WorkParams workParams)
        {
            ExecuteSetCommandRaw(Constants.SetAlarmParams.code, new[] { (byte)workParams.AlarmDuration, (byte)workParams.AlarmVolume, (byte)workParams.AlarmTone });
        }

        private void SetOperatorPassword(WorkParams workParams)
        {
            var bytes = BitConverter.GetBytes(workParams.Password);
            var byte0 = bytes[0];
            var byte1 = bytes.Length > 1 ? bytes[1] : (byte)0;
            var byte2 = bytes.Length > 2 ? bytes[2] : (byte)0;
            var byte3 = bytes.Length > 3 ? bytes[3] : (byte)0;

            ExecuteSetCommandRaw(Constants.SetPassword.code, new[] { byte0, byte1, byte2, byte3 });
        }

        private void SetTime(WorkParams workParams)
        {
            var ye = (byte)(workParams.DateTime.Year - 2000);
            var mo = (byte)workParams.DateTime.Month;
            var da = (byte)workParams.DateTime.Day;

            var ho = (byte)workParams.DateTime.Hour;
            var mi = (byte)workParams.DateTime.Minute;
            var se = (byte)workParams.DateTime.Second;

            ExecuteSetCommandRaw(Constants.SetTime.code, new[] { ye, mo, da, ho, mi, se });
        }

        public void ClearPassageCount()
        {
            ExecuteSetCommandRaw(Constants.ClearPassageCount.code, new byte[] { 0x0F });
        }

        public void SetNetworkParams(WorkParams workParams)
        {
            var args = IPAddress.Parse(workParams.IP).GetAddressBytes().ToList();

            args.AddRange(workParams.Mask.Split('.').Select(byte.Parse));
            args.AddRange(workParams.Gateway.Split('.').Select(byte.Parse));
            args.AddRange(BitConverter.GetBytes((short)workParams.PortTCP));
            args.AddRange(BitConverter.GetBytes((short)workParams.PortUDP));

            ExecuteSetCommandRaw(Constants.SetNetworkParams.code, args.ToArray());

            NetworkProto.Disconnect();
            NetworkProto.Ip = workParams.IP;
            NetworkProto.PortTCP = workParams.PortTCP;
        }

        public void SetWorkProgramScene(WorkParams workParams)
        {
            ExecuteSetCommandRaw(Constants.SetWorkProgramScene.code, new[] { workParams.ZonesSensorMode, workParams.WorkProgram, workParams.InfraredPassCounterMode });
        }

        public bool SimulatePass()
        {
            ExecuteGetCommand(Constants.SimulatePass.code, new byte[] { 0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00 });

            return true;
        }

        public bool SimulateAlarm()
        {
            ExecuteGetCommand(Constants.SimulatePass.code, new byte[] { 0x01, 0x2A,0x2A,0x00,0x00, 0x2A,0x2A,0x00,0x00 });

            return true;
        }

        private bool WorkProgramSceneTest(WorkParams workParams, byte testValue)
        {
#if DEBUG
            Console.WriteLine($"\nWorkProgramSceneTest: testing \"Set Working Mode\"...");
#endif
            workParams.WorkProgram = testValue;
            workParams.ZonesSensorMode = testValue;
            workParams.InfraredPassCounterMode = testValue;
            SetWorkProgramScene(workParams);
            Thread.Sleep(_requestDelay);
            InitZonesWorkMode(workParams);

            if (workParams.WorkProgram != testValue && workParams.ZonesSensorMode != testValue && workParams.InfraredPassCounterMode != testValue)
            {
#if DEBUG
                Console.WriteLine($"WorkProgramSceneTest: {workParams.IP}:\t WorkProgram test fail!");
#endif
                return false;
            }

            return true;
        }

        private bool ZonesSensitivityTest(WorkParams workParams, byte testValue)
        {
#if DEBUG
            Console.WriteLine($"\nZonesSensitivityTest: testing \"Set Zone Sensitivity\"...");
#endif

            for (var i = 0; i < workParams.SensorsSensitivity.Length; i++)
            {
                workParams.SensorsSensitivity[i] = testValue;
            }

            SetZonesSensitivity(workParams);
            Thread.Sleep(_requestDelay);
            InitZonesSensitivity(workParams);

            if (workParams.SensorsSensitivity[01] != testValue || workParams.SensorsSensitivity[03] != testValue ||
                workParams.SensorsSensitivity[05] != testValue)
            {
#if DEBUG
                Console.WriteLine($"ZonesSensitivityTest: {workParams.IP}:\t ZonesSensitivity test fail!");
#endif
                return false;
            }
            return true;
        }

        private bool BaseSensitivityTest(WorkParams workParams, byte testValue)
        {
#if DEBUG
            Console.WriteLine($"\nBaseSensitivityTest: testing \"Set Security Level\"...");
#endif

            workParams.BaseSensitivity = testValue;
            SetBaseSensitivity(workParams);
            Thread.Sleep(_requestDelay);
            InitBaseSensitivity(workParams);

            if (workParams.BaseSensitivity != testValue)
            {
#if DEBUG
                Console.WriteLine($"BaseSensitivityTest: {workParams.IP}:\t BaseSensitivity test fail!");
#endif

                return false;
            }

            return true;
        }

        private bool OperatorPasswordTest(WorkParams workParams, int testValue)
        {
#if DEBUG
            Console.WriteLine($"\nOperatorPasswordTest: testing \"Set Operator Password\"...");
#endif

            workParams.Password = testValue;
            SetOperatorPassword(workParams);
            Thread.Sleep(_requestDelay);
            InitOperatorPassword(workParams);

            if (workParams.Password != testValue)
            {
#if DEBUG
                Console.WriteLine($"OperatorPasswordTest: {workParams.IP}:\t OperatorPassword test fail!");
#endif

                return false;
            }

            return true;
        }

        private bool RestoreSettingsTest(WorkParams workParams)
        {
#if DEBUG
            Console.WriteLine($"\nRestoreSettingsTest: testing \"Restore Factory Settings\"...");
#endif

            var oldIp = workParams.IP;

            workParams.BaseSensitivity = 33;
            SetBaseSensitivity(workParams);
            Thread.Sleep(_requestDelay);
            workParams.PortTCP = 5001;
            SetNetworkParams(workParams);
            Thread.Sleep(5000);
            
            RestoreSettings(workParams);
            Thread.Sleep(5000);

            NetworkProto.Ip = workParams.IP = "192.168.1.100";
            NetworkProto.PortTCP = workParams.PortTCP = Constants.PortTCPDefault;
            InitBaseSensitivity(workParams);
            InitPassageCount(workParams);

#if DEBUG
            Console.WriteLine($"RestoreSettingsTest: done...");
#endif

            if (workParams.BaseSensitivity == 33)
            {
#if DEBUG
                Console.WriteLine($"RestoreSettingsTest: {workParams.IP}:\t RestoreSettings test fail!");
#endif

                return false;
            }

            var enterPassagesCount = workParams?.ForwardPassageCount ?? 0;
            var enterAlarmCount = workParams?.ForwardAlarmsCount ?? 0;
            var exitPassagesCount = workParams?.BackwardPassageCount ?? 0;
            var exitAlarmCount = workParams?.BackwardAlarmsCount ?? 0;

            Console.WriteLine($"\nRestoreSettingsTest: \n\t EnterPassagesCount {enterPassagesCount}\n\t EnterAlarmCount {enterAlarmCount}" +
                              $"\n\t ExitPassagesCount {exitPassagesCount}\n\t ExitAlarmCount {exitAlarmCount}");

            return true;
        }

        private bool NetworkTest(WorkParams workParams)
        {
#if DEBUG
            Console.WriteLine($"\nNetworkTest: testing \"Set Ethernet Parameters\"...");
#endif

            workParams.BaseSensitivity = 22;
            SetBaseSensitivity(workParams);
            Thread.Sleep(_requestDelay);

            var ipBuff = workParams.IP;
            workParams.IP = IncrementIp(ipBuff);

            workParams.PortTCP = 5001;
            SetNetworkParams(workParams);

#if DEBUG
            Console.WriteLine($"NetworkTest: Thread.Sleep(15000)");
#endif
            Thread.Sleep(15000);

            workParams.BaseSensitivity = 11;

            InitBaseSensitivity(workParams);

            if (workParams.BaseSensitivity != 22)
            {
#if DEBUG
                Console.WriteLine($"NetworkTest: {workParams.IP}:\tNetworkTest test fail!");
#endif
                return false;
            }

            workParams.IP = ipBuff;
            SetNetworkParams(workParams);

            return true;

            string IncrementIp(string ipAddressString)
            {
                if (!IPAddress.TryParse(ipAddressString, out IPAddress address))
                {
                    throw new ArgumentException("Invalid IP address format.", nameof(ipAddressString));
                }

                byte[] bytes = address.GetAddressBytes();

                if (bytes.Length != 4)
                {
                    throw new NotSupportedException("Only IPv4 addresses are supported by this method.");
                }

                if (bytes[3] < 255)
                {
                    bytes[3]++;
                }
                else
                {
                    bytes[3] = 0;
                }

                IPAddress newAddress = new IPAddress(bytes);

                return newAddress.ToString();
            }
        }

        private bool TimeTest(WorkParams workParams, DateTime testValue)
        {
#if DEBUG
            Console.WriteLine($"\nTimeTest: testing \"Set Time Parameters\"...");
#endif

            workParams.DateTime = testValue;
            SetTime(workParams);
            Thread.Sleep(_requestDelay);
            InitTime(workParams);

            if (workParams.DateTime != testValue)
            {
#if DEBUG
                Console.WriteLine($"TimeTest: {workParams.IP}:\t Time test fail!");
#endif

                return false;
            }

            return true;
        }

        private bool WorkingFreqTest(WorkParams workParams)
        {
#if DEBUG
            Console.WriteLine($"\nWorkingFreqTest: testing \"Set Driving Frequency\"...");
#endif

            byte workingFreq;

            do
            {
                workingFreq = (byte)new Random().Next(1, 51);
            } while (workingFreq % 3 != 0);

            workParams.WorkingFreq = workingFreq;
            SetWorkFrequency(workParams);
            Thread.Sleep(_requestDelay);
            InitWorkFrequency(workParams);

            if (workParams.WorkingFreq != workingFreq)
            {
#if DEBUG
                Console.WriteLine($"WorkingFreqTest: {workParams.IP}:\t WorkingFreq test fail!");
#endif
                return false;
            }

            return true;
        }

        private bool AlarmParamsTest(WorkParams workParams, byte testValue)
        {
#if DEBUG
            Console.WriteLine($"\nAlarmParamsTest: testing \"Set Alarm Parameters\"...");
#endif

            workParams.AlarmDuration = testValue;
            workParams.AlarmVolume = testValue;
            workParams.AlarmTone = testValue;
            SetAlarmParams(workParams);
            Thread.Sleep(_requestDelay);
            InitAlarmParams(workParams);

            if (workParams.AlarmDuration != testValue || workParams.AlarmVolume != testValue || workParams.AlarmTone != testValue)
            {
#if DEBUG
                Console.WriteLine($"AlarmParamsTest: {workParams.IP}:\t AlarmParams sound test fail!");
#endif
                return false;
            }

            return true;
        }

        private bool ClearPassageTest(WorkParams workParams)
        {
#if DEBUG
            Console.WriteLine($"\nClearPassageTest: testing \"Clear People Count\"...");
#endif

            ClearPassageCount();
            Thread.Sleep(_requestDelay);
            InitPassageCount(workParams);

            if (workParams.ForwardAlarmsCount != 0 || workParams.ForwardPassageCount != 0 || workParams.BackwardAlarmsCount != 0 || workParams.BackwardPassageCount != 0)
            {
#if DEBUG
                Console.WriteLine($"ClearPassageTest: {workParams.IP}:\t ClearPassage test fail!");
#endif
                return false;
            }

            return true;
        }

    }
}
