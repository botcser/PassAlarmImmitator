using IRAPROM.MyCore.Model.WP;
using PassAlarmSimulator.Device;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
//using PassAlarmSimulator.Validator;
// ReSharper disable LocalizableElement

namespace IRAPROM.MyCore.Device.Matreshka
{
    public class WorkParamsProto : CommandExecutor, IWorkParamsProto, ITestsProto
    {
        private readonly int _requestDelay = TimeSpan.FromMilliseconds(150).Milliseconds; 
        private FamilyInfo _familyInfo;

        public WorkParamsProto(INetworkProtoDual networkProto, IDatagramProto datagramProto, List<(short, short, int, string)> getCommands, List<(short, short, int, string)> setCommands, Constants familyInfo) : base(familyInfo, networkProto, datagramProto, getCommands, setCommands)
        {
        }

        public WorkParamsProto(IDatagramProto datagramProto, List<(short, short, int, string)> getCommands, List<(short, short, int, string)> setCommands, Constants familyInfo) : base(familyInfo, datagramProto, getCommands, setCommands)
        {
        }

        public WorkParams GetWorkParams()
        {
            var workParams = new WorkParams
            {
                ModelId = (byte)Constants.Model.PCV900                          // TODO: Старые матрехи еще не имеют эту фичу - поумолчанию
            };

            try
            {
                InitZonesSensitivity(workParams);

                InitModelBySensorsSensitivity(workParams);

                InitNetworkParams(workParams); // TODO: PCV1800 broken proto
                InitBaseSensitivity(workParams);
                InitWorkFrequency(workParams);
                InitAlarmParams(workParams);
                InitWorkProgramScene(workParams);
                InitZonesWorkMode(workParams);
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
            var success = true;

            try
            {
                SetWorkingMode(workParams);
                Thread.Sleep(_requestDelay);

                SetZonesWorkMode(workParams);
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                success = false;
            }
            finally
            {
                NetworkProto.Disconnect();
            }

            return success;
        }
        
        public bool StaticTest(WorkParams workParams)
        {
            const byte testValue = 0x02;

            return WorkProgramTest(workParams, testValue) && ZonesWorkModeTest(workParams, testValue) && ZonesSensitivityTest(workParams, testValue) &&
                   BaseSensitivityTest(workParams, testValue) && WorkingFreqTest(workParams) && AlarmParamsTest(workParams, testValue) && ClearPassageTest(workParams);
        }
        
        public bool BrutePortsTest(WorkParams workParams)
        {
            byte testValue = 0x09;

            workParams.WorkProgram = testValue;
            SetWorkingMode(workParams);
            Thread.Sleep(_requestDelay);

            workParams.SensorsSensitivity = new[]
            {
                testValue, testValue, testValue, testValue, testValue,
                testValue,
                testValue, testValue, testValue, testValue, testValue,
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

            return true;
        }

        public bool DynamicTest(WorkParams workParams, int milliSecondsTimeout, bool alarm)
        {
            Console.WriteLine($@"You must make a passage (dirty) through all devices at once. You have 20 seconds to do this!");

            var timer = milliSecondsTimeout;
            MetalDetectorPassage alarmPassage = null;

            do
            {
                timer -= 1000;

                Thread.Sleep(1000);

                //alarmPassage = Validator.FoundDevices.FirstOrDefault(i => i.MAC == workParams.MAC)?.LastPassage;

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // ReSharper disable once HeuristicUnreachableCode
                if (alarmPassage != null) break;

            } while (timer > 0);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (alarmPassage == null)
            {
                Console.WriteLine($@"DynamicTest: Error: No Alarm Passage message was received from the device {workParams.IP}:{workParams.MAC}.");

                return false;
            }

            // ReSharper disable once HeuristicUnreachableCode
            alarmPassage = alarmPassage.Clone();
            timer = milliSecondsTimeout;
            MetalDetectorPassage lastPassage = null;

            Console.WriteLine($"\nOK. Now you must make a passage (clean) through all devices at once. You have 20 seconds to do this!");
            //Validator.FoundDevices.FirstOrDefault(i => i.MAC == workParams.MAC)!.LastPassage = null;

            do
            {
                timer -= 1000;

                Thread.Sleep(1000);

                //lastPassage = Validator.FoundDevices.FirstOrDefault(i => i.MAC == workParams.MAC)?.LastPassage;

                if (lastPassage != null) break;

            } while (timer > 0);

            if (lastPassage == null)
            {
                Console.WriteLine($"DynamicTest: Error: No Clean Passage message was received from the device {workParams.IP}:{workParams.MAC}.");

                return false;
            }

            return ValidatePassages(alarmPassage, lastPassage);
        }

        private bool ValidatePassages(MetalDetectorPassage alarmPassage, MetalDetectorPassage lastPassage)
        {
            throw new NotImplementedException();
        }

        public void CallPassage()
        {
            ExecuteCommonCommand(new Command(DatagramProto.MakeRequestDatagram(Constants.CallPassage.deviceCode), Constants.CallAlarm.code, "127.0.0.1", Constants.PortTCPDefault.ToString(), ProtocolType.Tcp));
        }

        public void CallAlarm()
        {
            ExecuteCommonCommand(new Command(DatagramProto.MakeRequestDatagram(Constants.CallAlarm.deviceCode), Constants.CallAlarm.code, "127.0.0.1", Constants.PortTCPDefault.ToString(), ProtocolType.Tcp));
        }

        public void InitZonesSensitivity(WorkParams workParams)
        {
            try
            {
                var response = ExecuteGetCommand(Constants.GetZonesSensitivity3.code);
                
                if (response.Length == 0)
                {
                    throw new Exception("");
                }

                workParams.SensorsSensitivity = null;
                workParams.SensorsSensitivity ??= new short[response.Length / 2];

                for (byte i = 0; i < response.Length; i += 2)
                {
                    workParams.SensorsSensitivity[i / 2] = (short)(response[i] + (response[i + 1] << 8));
                }
            }
            catch (Exception)
            {
                try
                {
                    var response = ExecuteGetCommand(Constants.GetZonesSensitivity6.code);
                    
                    if (response.Length == 0)
                    {
                        throw new Exception("");
                    }

                    workParams.SensorsSensitivity = null;
                    workParams.SensorsSensitivity ??= new short[response.Length / 2];

                    for (byte i = 0; i < response.Length; i += 2)
                    {
                        workParams.SensorsSensitivity[i / 2] = (short)(response[i] + (response[i + 1] << 8));
                    }
                }
                catch (Exception)
                {
                    var response = ExecuteGetCommand(Constants.GetZonesSensitivity3.code);
                    
                    if (response.Length == 0)
                    {
                        throw new Exception($"InitZonesSensitivity: EX: no response from {workParams.IP}:{workParams.SerialNumber}:{workParams.FirmwareVersion}!" +
                                            $"\nERROR: InitZonesSensitivity: unknown coils count of device {workParams.MAC}!");
                    }

                    workParams.SensorsSensitivity = null;
                    workParams.SensorsSensitivity ??= new short[response.Length / 2];

                    for (byte i = 0; i < response.Length; i += 2)
                    {
                        workParams.SensorsSensitivity[i / 2] = (short)(response[i] + (response[i + 1] << 8));
                    }

                    Console.WriteLine();
                }
            }
        }

        public void InitNetworkParams(WorkParams workParams)
        {
            var response = ExecuteGetCommand(Constants.GetNetworkParams.code);
            
            if (response.Length == 0)
            {
                throw new Exception($"InitNetworkParams: EX: no response from {workParams.IP}:{workParams.SerialNumber}:{workParams.FirmwareVersion}!");
            }

            using (var ms = new MemoryStream(response))
            {
                using (var br = new BinaryReader(ms))
                {
                    var arIp = br.ReadBytes(4);
                    var arMask = br.ReadBytes(4);
                    var arIpGateway = br.ReadBytes(4);

                    workParams.IP = $"{arIp[0]}.{arIp[1]}.{arIp[2]}.{arIp[3]}";
                    workParams.Mask = $"{arMask[0]}.{arMask[1]}.{arMask[2]}.{arMask[3]}";
                    workParams.Gateway = $"{arIpGateway[0]}.{arIpGateway[1]}.{arIpGateway[2]}.{arIpGateway[3]}";
                    workParams.PortTCP = br.ReadInt16();
                    workParams.PortUDP = br.ReadInt16();
                }
            }
        }

        public void InitBaseSensitivity(WorkParams workParams)
        {
            var response = ExecuteGetCommand(Constants.GetBaseSensitivity.code);

            if (response.Length == 0)
            {
                throw new Exception($"InitNetworkParams: EX: no response from {workParams.IP}:{workParams.SerialNumber}:{workParams.FirmwareVersion}!");
            }

            workParams.BaseSensitivity = response.FirstOrDefault();
        }

        public void InitWorkFrequency(WorkParams workParams)
        {
            var response = ExecuteGetCommand(Constants.GetWorkFrequency.code);

            if (response.Length == 0)
            {
                throw new Exception($"InitWorkFrequency: EX: no response from {workParams.IP}:{workParams.SerialNumber}:{workParams.FirmwareVersion}!");
            }

            workParams.WorkingFreq = response.FirstOrDefault();
        }

        public void InitAlarmParams(WorkParams workParams)
        {
            var response = ExecuteGetCommand(Constants.GetAlarmParams.code);
            
            if (response.Length == 0)
            {
                throw new Exception($"InitAlarmParams: EX: no response from {workParams.IP}:{workParams.SerialNumber}:{workParams.FirmwareVersion}!");
            }

            workParams.AlarmDuration = response[0];
            workParams.AlarmVolume = response[1];
            workParams.AlarmTone = response[2];
        }

        public void InitWorkProgramScene(WorkParams workParams)
        {
            var response = ExecuteGetCommand(Constants.GetWorkProgramScene.code);

            if (response.Length == 0)
            {
                throw new Exception($"InitWorkProgramScene: EX: no response from {workParams.IP}:{workParams.SerialNumber}:{workParams.FirmwareVersion}!");
            }

            workParams.WorkProgram = response.FirstOrDefault();
        }

        public void InitZonesWorkMode(WorkParams workParams)
        {
            var response = ExecuteGetCommand(Constants.GetZonesWorkMode.code);
            
            if (response.Length == 0)
            {
                throw new Exception($"InitZonesWorkMode: EX: no response from {workParams.IP}:{workParams.SerialNumber}:{workParams.FirmwareVersion}!");
            }

            workParams.ZonesSensorMode = response[0];
            workParams.WorkProgram = response[1];
            workParams.InfraredPassCounterMode = response[2];
        }

        public void InitPassageCount(WorkParams workParams)
        {
            var responsePeoplePassing = ExecuteGetCommand(Constants.GetPassageCount.code, new byte[] { 0x00 });
            var responsePeopleReturning = ExecuteGetCommand(Constants.GetPassageCount.code, new byte[] { 0x01 });
            var responsePeoplePassingAlarms = ExecuteGetCommand(Constants.GetPassageCount.code, new byte[] { 0x02 });
            var responsePeopleReturningAlarms = ExecuteGetCommand(Constants.GetPassageCount.code, new byte[] { 0x03 });

            workParams.ForwardPassageCount = BitConverter.ToUInt32(responsePeoplePassing, 1);
            workParams.BackwardPassageCount = BitConverter.ToUInt32(responsePeopleReturning, 1);
            workParams.ForwardAlarmsCount = BitConverter.ToUInt32(responsePeoplePassingAlarms, 1);
            workParams.BackwardAlarmsCount = BitConverter.ToUInt32(responsePeopleReturningAlarms, 1);
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
            ExecuteSetCommandRaw(Constants.SetWorkFrequency.code, new[] { workParams.WorkingFreq });
        }

        public void SetAlarmParams(WorkParams workParams)
        {
            ExecuteSetCommandRaw(Constants.SetAlarmParams.code, new[] { workParams.AlarmDuration, workParams.AlarmVolume, workParams.AlarmTone });
        }

        public void ClearPassageCount()
        {
            ExecuteSetCommandRaw(Constants.ClearPassageCount.code, new byte[] { 0x00 });
            ExecuteSetCommandRaw(Constants.ClearPassageCount.code, new byte[] { 0x01 });
            ExecuteSetCommandRaw(Constants.ClearPassageCount.code, new byte[] { 0x02 });
            ExecuteSetCommandRaw(Constants.ClearPassageCount.code, new byte[] { 0x03 });
        }

        public bool SetWorkingMode(WorkParams workParams)
        {
            var success = true;

            try
            {
                ExecuteSetCommandRaw(Constants.SetWorkProgramScene.code, new byte[] { workParams.WorkProgram });
                Task.Delay(_requestDelay);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                success = false;
            }
            finally
            {
                NetworkProto.Disconnect();
            }

            return success;
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
        }

        private bool WorkProgramTest(WorkParams workParams, byte testValue)
        {
            workParams.WorkProgram = testValue;
            SetWorkingMode(workParams);
            Thread.Sleep(_requestDelay);
            InitWorkProgramScene(workParams);

            if (workParams.WorkProgram != testValue)
            {
#if DEBUG
                Console.WriteLine($"SelfTest: {workParams.IP}:\t WorkProgram test fail!");
#endif
                return false;
            }

            return true;
        }

        private bool ZonesWorkModeTest(WorkParams workParams, byte testValue)
        {
            workParams.ZonesSensorMode = testValue;
            workParams.WorkProgram = testValue; // TODO: PCV1800 не работает
            workParams.AlarmModeAny = testValue;
            SetZonesWorkMode(workParams);
            Thread.Sleep(_requestDelay);
            InitZonesWorkMode(workParams);

            if (workParams.ZonesSensorMode != testValue || workParams.AlarmModeAny != testValue)
            {
#if DEBUG
                Console.WriteLine($"SelfTest: {workParams.IP}:\t ZonesWorkMode test fail!");
#endif
                return false;
            }
            return true;
        }

        private bool ZonesSensitivityTest(WorkParams workParams, byte testValue)
        {
            workParams.SensorsSensitivity = new[]
            {
                testValue, testValue, testValue, testValue, testValue,
                testValue,
                testValue, testValue, testValue, testValue, testValue,
                (short)testValue,
            };
            SetZonesSensitivity(workParams);
            Thread.Sleep(_requestDelay);
            InitZonesSensitivity(workParams);

            if (workParams.SensorsSensitivity[01] != testValue || workParams.SensorsSensitivity[03] != testValue ||
                workParams.SensorsSensitivity[06] != testValue)
            {
#if DEBUG
                Console.WriteLine($"SelfTest: {workParams.IP}:\t ZonesSensitivity test fail!");
#endif
                return false;
            }
            return true;
        }

        private bool BaseSensitivityTest(WorkParams workParams, byte testValue)
        {
            workParams.BaseSensitivity = testValue;
            SetBaseSensitivity(workParams);
            Thread.Sleep(_requestDelay);
            InitBaseSensitivity(workParams);

            if (workParams.BaseSensitivity != testValue)
            {
#if DEBUG
                Console.WriteLine($"SelfTest: {workParams.IP}:\t BaseSensitivity test fail!");
#endif

                return false;
            }

            return true;
        }

        private bool WorkingFreqTest(WorkParams workParams)
        {
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
                Console.WriteLine($"SelfTest: {workParams.IP}:\t WorkingFreq test fail!");
#endif
                return false;
            }

            return true;
        }

        private bool AlarmParamsTest(WorkParams workParams, byte testValue)
        {
            workParams.AlarmDuration = testValue;
            workParams.AlarmVolume = testValue;
            workParams.AlarmTone = testValue;
            SetAlarmParams(workParams);
            Thread.Sleep(_requestDelay);
            InitAlarmParams(workParams);

            if (workParams.AlarmDuration != testValue || workParams.AlarmVolume != testValue || workParams.AlarmTone != testValue)
            {
#if DEBUG
                Console.WriteLine($"SelfTest: {workParams.IP}:\t AlarmParams sound test fail!");
#endif
                return false;
            }

            return true;
        }

        private bool ClearPassageTest(WorkParams workParams)
        {
            ClearPassageCount();
            Thread.Sleep(_requestDelay);
            InitPassageCount(workParams);

            if (workParams.ForwardAlarmsCount != 0 || workParams.ForwardPassageCount != 0 || workParams.BackwardAlarmsCount != 0 || workParams.BackwardPassageCount != 0)
            {
#if DEBUG
                Console.WriteLine($"SelfTest: {workParams.IP}:\t ClearPassage test fail!");
#endif
                return false;
            }

            return true;
        }

        private void SetZonesWorkMode(WorkParams workParams)
        {
            ExecuteSetCommandRaw(Constants.SetZonesWorkMode.code, new[] { workParams.ZonesSensorMode, workParams.WorkProgram, workParams.InfraredPassCounterMode });
        }

        private void InitModelBySensorsSensitivity(WorkParams workParams)
        {
            var sensorsSensitivityLength = workParams.SensorsSensitivity.Length;

            foreach (var model in FamilyInfo.Models.Where(model => model.Value.AvailableZonesCount[0] * 2 == sensorsSensitivityLength))
            {
                workParams.ModelId = (byte)model.Key;

                Console.Write($"ModelBySensors: guess device is {model.Value.Name}");
                return;
            }

            Console.WriteLine($@"Unknown SensorsSensitivity.Length {sensorsSensitivityLength} for identify PCV");
        }
    }
}
