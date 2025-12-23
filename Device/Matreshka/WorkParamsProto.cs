using IRAPROM.MyCore.Model.MD;
using IRAPROM.MyCore.Model.WP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CommandTransmitter.Device;

namespace Device.Matreshka
{
    public class WorkParamsProto : CommandExecutor, IWorkParamsProto
    {
        private readonly int _requestDelay = TimeSpan.FromMilliseconds(150).Milliseconds;
        
        public WorkParamsProto(INetworkProtoDual networkProto, IDatagramProto datagramProto, List<(short, int, string)> getCommands, List<(short, int, string)> setCommands) : base(networkProto, datagramProto, getCommands, setCommands)
        {
        }

        public WorkParamsProto(IDatagramProto datagramProto, List<(short, int, string)> getCommands, List<(short, int, string)> setCommands) : base(datagramProto, getCommands, setCommands)
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
            SetWorkProgramScene(workParams);
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

            NetworkProto.Disconnect();

            return true;
        }

        public bool SelfTest(WorkParams workParams)
        {
            byte testValue = 0x02;
            var result = true;

            WorkProgramTest();
            ZonesWorkModeTest();
            ZonesSensitivityTest();
            BaseSensitivityTest();
            WorkingFreqTest();
            AlarmParamsTest();
            ClearPassageTest();
            HandTest();

            return result;


            void WorkProgramTest()
            {
                workParams.WorkProgram = testValue;
                SetWorkProgramScene(workParams);
                Thread.Sleep(_requestDelay);
                InitWorkProgramScene(workParams);

                if (workParams.WorkProgram != testValue)
                {
#if DEBUG
                    Console.WriteLine($"SelfTest: {workParams.IP}:\t WorkProgram test fail!");
#endif
                    result = false;
                }
            }
            void ZonesWorkModeTest()
            {
                workParams.ZonesSensorMode = testValue;
                workParams.SceneMode = testValue; // TODO: PCV1800 не работает
                workParams.AlarmInfraMode = testValue;
                SetZonesWorkMode(workParams);
                Thread.Sleep(_requestDelay);
                InitZonesWorkMode(workParams);

                if (workParams.ZonesSensorMode != testValue || /*workParams.SceneMode != testValue ||*/workParams.AlarmInfraMode != testValue)
                {
#if DEBUG
                    Console.WriteLine($"SelfTest: {workParams.IP}:\t ZonesWorkMode test fail!");
#endif
                    result = false;
                }
            }

            void ZonesSensitivityTest()
            {
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
                    Console.WriteLine($"SelfTest: {workParams.IP}:\t ZonesSensitivity test fail!");
#endif
                    result = false;
                }
            }

            void BaseSensitivityTest()
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
                    result = false;
                }
            }

            void WorkingFreqTest()
            {
                byte workingFreq = 2;
                do
                {
                    workingFreq = (byte)new Random().Next(51);
                } while (workingFreq % 2 != 0);

                workParams.WorkingFreq = workingFreq;
                SetWorkFrequency(workParams);
                Thread.Sleep(_requestDelay);
                InitWorkFrequency(workParams);
                if (workParams.WorkingFreq != workingFreq)
                {
#if DEBUG
                    Console.WriteLine($"SelfTest: {workParams.IP}:\t WorkingFreq test fail!");
#endif
                    result = false;
                }
            }

            void AlarmParamsTest()
            {
                workParams.AlarmDuration = testValue;
                workParams.AlarmVolume = testValue;
                workParams.AlarmTone = testValue;
                SetAlarmParams(workParams);
                Thread.Sleep(_requestDelay);
                InitAlarmParams(workParams);

                if (workParams.AlarmDuration != testValue || workParams.AlarmVolume != testValue ||
                    workParams.AlarmTone != testValue)
                {
#if DEBUG
                    Console.WriteLine($"SelfTest: {workParams.IP}:\t AlarmParams test fail!");
#endif
                    result = false;
                }
            }

            void ClearPassageTest()
            {
                ClearPassageCount();
                Thread.Sleep(_requestDelay);
                InitPassageCount(workParams);

                if (workParams.ForwardAlarmsCount != 0 || workParams.ForwardPassageCount != 0 || workParams.BackwardAlarmsCount != 0 || workParams.BackwardPassageCount != 0)
                {
#if DEBUG
                    Console.WriteLine($"SelfTest: {workParams.IP}:\t ClearPassage test fail!");
#endif
                    result = false;
                }
            }

            void HandTest()
            {
                testValue = 0x09;

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
                } while (workParams.WorkingFreq % 2 != 0);

                SetWorkFrequency(workParams);
                Thread.Sleep(_requestDelay);

                workParams.AlarmDuration = testValue;
                workParams.AlarmVolume = testValue;
                workParams.AlarmTone = testValue;
                SetAlarmParams(workParams);
                Thread.Sleep(_requestDelay);

                ClearPassageCount();
            }
        }

        public void CallPassage()
        {
            ExecuteCommonCommand(new Command(DatagramProto.MakeRequestDatagram(Constants.CallPassage.code), "127.0.0.1", Constants.PortTCPDefault.ToString(), ProtocolType.Tcp));
        }

        public void CallAlarm()
        {
            ExecuteCommonCommand(new Command(DatagramProto.MakeRequestDatagram(Constants.CallAlarm.code), "127.0.0.1", Constants.PortTCPDefault.ToString(), ProtocolType.Tcp));
        }

        public void InitZonesSensitivity(WorkParams workParams)
        {
            var response = ExecuteGetCommand(Constants.GetZonesSensitivity11.code);
            
            workParams.SensorsSensitivity ??= new short[response.Length / 2];

            for (byte i = 0; i < response.Length; i += 2)
            {
                workParams.SensorsSensitivity[i / 2] = (short)(response[i] + (((short)response[i + 1]) << 8));
            }

            //InitModelBySensorsSensitivity(workParams);
        }

        public void InitNetworkParams(WorkParams workParams)
        {
            var response = ExecuteGetCommand(Constants.GetNetworkParams.code);

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

        public void InitWorkProgramScene(WorkParams workParams)
        {
            workParams.WorkProgram = ExecuteGetCommand(Constants.GetWorkProgramScene.code).FirstOrDefault();
        }

        public void InitZonesWorkMode(WorkParams workParams)
        {
            var response = ExecuteGetCommand(Constants.GetZonesWorkMode.code);

            workParams.ZonesSensorMode = response[0];
            workParams.SceneMode = response[1];
            workParams.InfraredPassCounterMode = response[2];
        }

        public void InitPassageCount(WorkParams workParams)
        {
            var responsePeoplePassing = ExecuteGetCommand(Constants.GetPassageCount.code, new byte[] { 0x00 });
            var responsePeopleReturning = ExecuteGetCommand(Constants.GetPassageCount.code, new byte[] { 0x01 });
            var responsePeoplePassingAlarms = ExecuteGetCommand(Constants.GetPassageCount.code, new byte[] { 0x02 });
            var responsePeopleReturningAlarms = ExecuteGetCommand(Constants.GetPassageCount.code, new byte[] { 0x03 });

            workParams.ForwardPassageCount = BitConverter.ToInt32(responsePeoplePassing, 1);
            workParams.BackwardPassageCount = BitConverter.ToInt32(responsePeopleReturning, 1);
            workParams.ForwardAlarmsCount = BitConverter.ToInt32(responsePeoplePassingAlarms, 1);
            workParams.BackwardAlarmsCount = BitConverter.ToInt32(responsePeopleReturningAlarms, 1);
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

        public void ClearPassageCount()
        {
            ExecuteSetCommandRaw(Constants.ClearPassageCount.code, new byte[] { 0x00 });
            ExecuteSetCommandRaw(Constants.ClearPassageCount.code, new byte[] { 0x01 });
            ExecuteSetCommandRaw(Constants.ClearPassageCount.code, new byte[] { 0x02 });
            ExecuteSetCommandRaw(Constants.ClearPassageCount.code, new byte[] { 0x03 });
        }

        public void SetWorkProgramScene(WorkParams workParams)
        {
            ExecuteSetCommandRaw(Constants.SetWorkProgramScene.code, new byte[] { workParams.WorkProgram });
        }

        public void SetNetworkParams(WorkParams workParams)
        {
            var args = IPAddress.Parse(workParams.IP).GetAddressBytes().ToList();

            args.AddRange(workParams.Mask.Split('.').Select(byte.Parse));
            args.AddRange(workParams.Gateway.Split('.').Select(byte.Parse));
            args.AddRange(BitConverter.GetBytes((short)workParams.PortTCP));
            args.AddRange(BitConverter.GetBytes((short)workParams.PortUDP));

            ExecuteSetCommandRaw(Constants.SetNetworkParams.code, args.ToArray());

            NetworkProto.Ip = workParams.IP;
        }

        private void SetZonesWorkMode(WorkParams workParams)
        {
            ExecuteSetCommandRaw(Constants.SetZonesWorkMode.code, new[] { workParams.ZonesSensorMode, workParams.SceneMode, workParams.InfraredPassCounterMode });
        }

        private void InitModelBySensorsSensitivity(WorkParams workParams)
        {
            var sensorsSensitivityLength = workParams.SensorsSensitivity.Length;

            foreach (var model in Constants.Models.Where(model => model.Value.AvailableZonesCount[0] * 2 == sensorsSensitivityLength))
            {
                workParams.ModelId = (byte)model.Value.ModelId;

                return;
            }

            Console.WriteLine($"Unknown SensorsSensitivity.Length {sensorsSensitivityLength} for identify PCV");
        }
    }
}
