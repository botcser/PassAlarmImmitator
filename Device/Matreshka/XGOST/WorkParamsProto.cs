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
        private readonly int _resetDelay = TimeSpan.FromMilliseconds(12000).Milliseconds;

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

            SetNetworkParams(workParams);
            Thread.Sleep(_requestDelay);

            NetworkProto.Disconnect();

            return true;
        }
        
        public bool StaticTest(WorkParams workParams)
        {
            const byte testValue = 0x02;

            return BaseSensitivityTest(workParams, testValue) && ZonesSensitivityTest(workParams, testValue) && WorkingFreqTest(workParams) &&
                   WorkProgramSceneTest(workParams, testValue) && AlarmParamsTest(workParams, testValue) && TimeTest(workParams, new DateTime(2022,2,2,2,2,2)) 
                   && OperatorPasswordTest(workParams, 287454020) && NetworkTest(workParams) && ClearPassageTest(workParams);
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

        public async Task<bool> DynamicTest(WorkParams workParams, int milliSecondsTimeout)
        {
            Console.WriteLine($"\nYou must make a passage (dirty) through all devices at once. You have 20 seconds to do this!");

            var timer = milliSecondsTimeout;
            MetalDetectorPassage alarmPassage;

            do
            {
                timer -= 1000;

                await Task.Delay(1000);

                //alarmPassage = Validator.FoundDevices.FirstOrDefault(i => i.MAC == workParams.MAC)?.LastPassage;

                //if (alarmPassage != null) break;

            } while (timer > 0);

            //if (alarmPassage == null)
            //{
            //    Console.WriteLine($"DynamicTest: Error: No Alarm Passage message was received from the device {workParams.IP}:{workParams.MAC}.");

            //    return false;
            //}

            //alarmPassage = alarmPassage.Clone();
            timer = milliSecondsTimeout;
            MetalDetectorPassage lastPassage;

            Console.WriteLine($"\nOK. Now you must make a passage (clean) through all devices at once. You have 20 seconds to do this!");
            //Validator.FoundDevices.FirstOrDefault(i => i.MAC == workParams.MAC)!.LastPassage = null;

            do
            {
                timer -= 1000;

                await Task.Delay(1000);

                //lastPassage = Validator.FoundDevices.FirstOrDefault(i => i.MAC == workParams.MAC)?.LastPassage;

                //if (lastPassage != null) break;

            } while (timer > 0);

            //if (lastPassage == null)
            //{
            //    Console.WriteLine($"DynamicTest: Error: No Clean Passage message was received from the device {workParams.IP}:{workParams.MAC}.");

            //    return false;
            //}
            return false;
            return ValidatePassages(alarmPassage, lastPassage);
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
            workParams.Password = ExecuteGetCommand(Constants.GetPassword.code).FirstOrDefault();
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

            var ye = response[0];
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

            workParams.ForwardPassageCount = BitConverter.ToInt32(response, 0);
            workParams.BackwardPassageCount = BitConverter.ToInt32(response, 4);
            workParams.ForwardAlarmsCount = BitConverter.ToInt32(response, 8);
            workParams.BackwardAlarmsCount = BitConverter.ToInt32(response, 12);
        }
        
        public void SetZonesSensitivity(WorkParams workParams)
        {
            ExecuteSetCommandRaw(Constants.SetZonesSensitivity.code, workParams.SensorsSensitivity.SelectMany(BitConverter.GetBytes).ToArray());
        }

        public void SetBaseSensitivity(WorkParams workParams)
        {
            ExecuteSetCommandRaw(Constants.SetBaseSensitivity.code, new [] { (byte)workParams.BaseSensitivity });
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
            ExecuteSetCommandRaw(Constants.ClearPassageCount.code, new byte[] { 0x00 });
            ExecuteSetCommandRaw(Constants.ClearPassageCount.code, new byte[] { 0x01 });
            ExecuteSetCommandRaw(Constants.ClearPassageCount.code, new byte[] { 0x02 });
            ExecuteSetCommandRaw(Constants.ClearPassageCount.code, new byte[] { 0x03 });
        }

        public void SetNetworkParams(WorkParams workParams)
        {
            var args = IPAddress.Parse(workParams.IP).GetAddressBytes().ToList();

            args.AddRange(workParams.Mask.Split('.').Select(byte.Parse));
            args.AddRange(workParams.Gateway.Split('.').Select(byte.Parse));
            args.AddRange(BitConverter.GetBytes((short)workParams.PortTCP));
            args.AddRange(BitConverter.GetBytes((short)workParams.PortUDP));

            //var command = ExecuteCommonCommand(new Command(DatagramProto.MakeRequestDatagram(Constants.SetNetworkParams.deviceCode), Constants.SetNetworkParams.code, workParams.IP, workParams.PortTCP.ToString(), ProtocolType.Tcp));
            //var response = command.Result;

            NetworkProto.Ip = workParams.IP;

            ExecuteSetCommandRaw(Constants.SetNetworkParams.code, args.ToArray());
        }

        public void SetWorkProgramScene(WorkParams workParams)
        {
            ExecuteSetCommandRaw(Constants.SetWorkProgramScene.code, new[] { workParams.ZonesSensorMode, workParams.WorkProgram, workParams.InfraredPassCounterMode });
        }

        private bool WorkProgramSceneTest(WorkParams workParams, byte testValue)
        {
#if DEBUG
            Console.WriteLine($"WorkProgramSceneTest: testing \"Set Working Mode\"...");
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
            Console.WriteLine($"ZonesSensitivityTest: testing \"Set Zone Sensitivity\"...");
#endif
            workParams.SensorsSensitivity = new[]
            {
                (short)testValue, (short)testValue, (short)testValue, (short)testValue, (short)testValue,
                (short)testValue,
                (short)testValue, (short)testValue, (short)testValue, (short)testValue, (short)testValue,
                (short)testValue,
            };
            SetZonesSensitivity(workParams);
            Thread.Sleep(_requestDelay);
            InitZonesSensitivity(workParams);

            if (workParams.SensorsSensitivity[01] != testValue || workParams.SensorsSensitivity[03] != testValue ||
                workParams.SensorsSensitivity[06] != testValue)
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
            Console.WriteLine($"BaseSensitivityTest: testing \"Set Security Level\"...");
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
            Console.WriteLine($"OperatorPasswordTest: testing \"Set Operator Password\"...");
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
            var defaultIp = workParams.IP;

            workParams.BaseSensitivity = 11;
            workParams.IP = "192.168.1.111";
            SetNetworkParams(workParams);
            Thread.Sleep(_requestDelay * 2);
            SetBaseSensitivity(workParams);
            Thread.Sleep(_requestDelay);
            //RestoreSettings(workParams);
            Thread.Sleep(_resetDelay);
            workParams.IP = defaultIp;
            InitBaseSensitivity(workParams);

            if (workParams.BaseSensitivity == 11)
            {
#if DEBUG
                Console.WriteLine($"SelfTest: {workParams.IP}:\t RestoreSettings test fail!");
#endif

                return false;
            }

            return true;
        }

        private bool NetworkTest(WorkParams workParams)
        {
#if DEBUG
            Console.WriteLine($"NetworkTest: testing \"Set Ethernet Parameters\"...");
#endif

            var defaultIp = workParams.IP;

            workParams.BaseSensitivity = 11;
            workParams.IP = "192.168.1.111";
            SetNetworkParams(workParams);
            Thread.Sleep(_requestDelay * 2);
            workParams.BaseSensitivity = 22;
            InitBaseSensitivity(workParams);

            if (workParams.BaseSensitivity != 11)
            {
#if DEBUG
                Console.WriteLine($"NetworkTest: {workParams.IP}:\t RestoreSettings test fail!");
#endif

                return false;
            }

            return true;
        }

        private bool TimeTest(WorkParams workParams, DateTime testValue)
        {
#if DEBUG
            Console.WriteLine($"TimeTest: testing \"Set Time Parameters\"...");
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
            Console.WriteLine($"WorkingFreqTest: testing \"Set Driving Frequency\"...");
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
            Console.WriteLine($"AlarmParamsTest: testing \"Set Alarm Parameters\"...");
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
            Console.WriteLine($"ClearPassageTest: testing \"Clear People Count\"...");
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

        private void InitModelBySensorsSensitivity(WorkParams workParams)
        {
            var sensorsSensitivityLength = workParams.SensorsSensitivity.Length;

            foreach (var model in Constants.Models.Where(model => model.Value.AvailableZonesCount[0] * 2 == sensorsSensitivityLength))
            {
                workParams.ModelId = (byte)model.Value.ModelId;

                Console.Write($"ModelBySensors: guess device is {model.Value.Name}");
                return;
            }

            Console.WriteLine($"Unknown SensorsSensitivity.Length {sensorsSensitivityLength} for identify PCV");
        }
    }
}
