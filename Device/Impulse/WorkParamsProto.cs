using IRAPROM.MyCore.Model.MD;
using System.Net;
using CommandTransmitter.Device;
using IRAPROM.MyCore.Model.WP;

namespace Device.Impulse
{
    public class WorkParamsProto : CommandExecutor, IWorkParamsProto
    {
        private int WorkParamsLength11 = 82; 
        private readonly int _requestDelay = TimeSpan.FromMilliseconds(150).Milliseconds;

        private byte[] _responseWorkParamsDatagram { get; set; }
        
        public WorkParamsProto(INetworkProtoDual networkProto, IDatagramProto datagramProto, List<(short, int, string)> getCommands, List<(short, int, string)> setCommands) : base(networkProto, datagramProto, getCommands, setCommands)
        {
        }
        
        public WorkParamsProto(IDatagramProto datagramProto, List<(short, int, string)> getCommands, List<(short, int, string)> setCommands) : base(datagramProto, getCommands, setCommands)
        {
        }

        public WorkParams GetWorkParams()
        {
            _responseWorkParamsDatagram = ExecuteGetCommandRaw(Constants.GetWorkParams.code);
            NetworkProto.Disconnect();

            return ParseWorkParams(_responseWorkParamsDatagram);
        }

        public bool SetWorkParams(WorkParams workParams)
        {
            SetWorkParamsDo();
            SetNetworkParams();

            NetworkProto.Disconnect();

            return true;
            
            void SetWorkParamsDo()
            {
                var args = _responseWorkParamsDatagram != null ? new byte[_responseWorkParamsDatagram.Length - Constants.DatagramMetaInfoLength - Constants.ChecksumLength] : new byte[WorkParamsLength11];

                SetPassword();
                SetZonesSensitivity();

                var zoneSensitivityEnd = 75 - 5;        // TODO

                SetBaseSensitivity();

                args[zoneSensitivityEnd + 2] = workParams.WorkingFreq;
                args[zoneSensitivityEnd + 3] = workParams.AlarmDuration;
                args[zoneSensitivityEnd + 4] = workParams.WorkProgram; // Application fields
                args[zoneSensitivityEnd + 5] = workParams.ModelId;
                args[zoneSensitivityEnd + 6] = workParams.AlarmVolume;
                args[zoneSensitivityEnd + 7] = workParams.AlarmTone;
                args[zoneSensitivityEnd + 8] = 0; // Automatic frequency selection
                args[zoneSensitivityEnd + 9] = 1;  // PC online
                args[zoneSensitivityEnd + 10] = (byte)(workParams.InfraredPassCounterMode + (workParams.ExchangeFrontBack ? 1 : 0));
                args[zoneSensitivityEnd + 11] = PackAlarmMode(workParams);

                ExecuteSetCommandRaw(Constants.SetWorkParams.code, args);


                void SetPassword()
                {
                    var password = BitConverter.GetBytes(workParams.Password);

                    args[0] = password[0];
                    args[1] = password[1];
                    args[2] = password[2];
                    args[3] = password[3];
                }

                void SetZonesSensitivity()
                {
                    for (var i = 0; i < workParams.SensorsSensitivity.Length; i++)
                    {
                        var sensitivity = BitConverter.GetBytes(workParams.SensorsSensitivity[i]);

                        if (sensitivity[0] == 0) break;

                        args[i * 2 + Constants.ZonesSensitivityStartIndex - Constants.DatagramMetaInfoLength] = sensitivity[1];
                        args[i * 2 + (Constants.ZonesSensitivityStartIndex - Constants.DatagramMetaInfoLength + 1)] = sensitivity[0];
                    }
                }

                void SetBaseSensitivity()
                {
                    var baseSensitivity = BitConverter.GetBytes(workParams.BaseSensitivity);

                    args[zoneSensitivityEnd] = baseSensitivity[1];
                    args[zoneSensitivityEnd + 1] = baseSensitivity[0];
                }
            }

            void SetNetworkParams()
            {
                var argv = IPAddress.Parse(workParams.IP).GetAddressBytes().ToList();

                argv.AddRange(workParams.Mask.Split('.').Select(byte.Parse));
                argv.AddRange(workParams.Gateway.Split('.').Select(byte.Parse));
                argv.AddRange(Convert.FromHexString(workParams.MAC));
                argv.AddRange(BitConverter.GetBytes((short)workParams.PortUDP).Reverse());
                argv.AddRange(BitConverter.GetBytes((short)workParams.PortTCP).Reverse());

                ExecuteSetCommandRaw(Constants.SetNetworkParams.code, argv.ToArray());
            }
        }
        
        
        public void ClearPassageCount()
        {
            ExecuteSetCommandRaw(Constants.ClearPassageCount.code, new byte[] { });
        }

        public void SetWorkProgramScene(WorkParams workParams)
        {
            ExecuteSetCommandRaw(Constants.SetWorkProgramScene.code, new byte[] { workParams.WorkProgram });
        }

        public WorkParams ParseWorkParams(byte[] response)
        {
            var workParams = new WorkParams();
            var zoneSensitivityEndIndex = response.Length - Constants.AfterZonesSensitivityBytesCountSuppose;
            
            workParams.SensorsSensitivity = GetZonesSensitivity();

            workParams.BaseSensitivity = (short)((response[zoneSensitivityEndIndex] << 8) + response[zoneSensitivityEndIndex + 1]);
            workParams.WorkingFreq = response[zoneSensitivityEndIndex + 2];
            workParams.AlarmDuration = response[zoneSensitivityEndIndex + 3];
            workParams.WorkProgram = response[zoneSensitivityEndIndex + 4];
            workParams.ModelId = response[zoneSensitivityEndIndex + 5];
            workParams.AlarmVolume = response[zoneSensitivityEndIndex + 6];
            workParams.AlarmTone = response[zoneSensitivityEndIndex + 7];
            workParams.ExchangeFrontBack = response[zoneSensitivityEndIndex + 10] >> 4 != 0; //Infrared mode:
                        //High 4 bits: Режим инфракрасных датчиков на направление прохода: - Реверс прохода, вперед или назад, 0x0 - прямой, 0x1 - обратный;
                        //Low 4 bits: Режим инфракрасных датчиков на проходы: 0x04 считать оба направления прохода; 0x03 считать только проход вперед (хз, мб и наоборот);
                        //0x02 считать только проход обратно (хз, мб и наоборот); 0x01 проходы выключены.
            workParams.InfraredPassCounterMode = (byte)(response[zoneSensitivityEndIndex + 10] & 0x0F);
       

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch ((MetalDetectorModel)workParams.ModelId)
            {
                case MetalDetectorModel.PC600MKX:
                case MetalDetectorModel.PC1800MKZ:
                case MetalDetectorModel.PC4400MK:
                case MetalDetectorModel.PC600MKZ:
                case MetalDetectorModel.PC4400MKZ:
                case MetalDetectorModel.PC4400MKX:
                case MetalDetectorModel.PC6300MKZ:
                case MetalDetectorModel.PC6300MKX:
                    ParseAlarmMode(workParams, response[zoneSensitivityEndIndex + 11]);
                    break;
            }

            return workParams;


            short[] GetZonesSensitivity()
            {
                var result = new short[(zoneSensitivityEndIndex - Constants.ZonesSensitivityStartIndex) / 2];
                var i = 0;

                for (i = 0; i < zoneSensitivityEndIndex - Constants.ZonesSensitivityStartIndex; i += 2)
                {
                    if (response[i + 9 + 1] != 0)
                    {
                        result[i / 2] = (short)((response[i + 9] << 8) + response[i + 9 + 1]);
                    }
                    else
                    {
                        break;
                    }
                }

                return result.Take(i / 2).ToArray();
            }
        }

        public bool SelfTest(WorkParams workParams)
        {
            byte testValue = 0x02;
            var result = true;
            
            WorkParamsTest();
            HandTest();

            return result;


            void WorkParamsTest()
            {
                workParams.BaseSensitivity = testValue;
                workParams.AlarmDuration = testValue;
                workParams.AlarmVolume = testValue;
                workParams.AlarmTone = testValue;
                workParams.ZonesSensorMode = testValue;
                workParams.SceneMode = testValue;
                workParams.AlarmInfraMode = (byte)(testValue - testValue);      // TODO: PC1800 UNUSED;
                workParams.SensorsSensitivity = new[]
                {
                    (short)testValue, (short)testValue, (short)testValue, (short)testValue, (short)testValue,
                    (short)testValue,
                    (short)testValue, (short)testValue, (short)testValue, (short)testValue, (short)testValue,
                    (short)testValue,
                };
                byte workingFreq = 2;
                do
                {
                    workingFreq = (byte)new Random().Next(51);
                } while (workingFreq % 2 != 0);
                workParams.WorkingFreq = workingFreq;

                SetWorkParams(workParams);
                Thread.Sleep(_requestDelay);

                workParams = GetWorkParams();

                if (workParams.SensorsSensitivity[01] != testValue || workParams.SensorsSensitivity[03] != testValue ||
                    workParams.SensorsSensitivity[06] != testValue)
                {
#if DEBUG
                    Console.WriteLine($"SelfTest: {workParams.IP}:\t ZonesSensitivity test fail!");
#endif
                    result = false;
                }

                if (workParams.BaseSensitivity != testValue)
                {
#if DEBUG
                    Console.WriteLine($"SelfTest: {workParams.IP}:\t BaseSensitivity test fail!");
#endif
                    result = false;
                }

                workParams.WorkProgram = testValue;
                SetWorkProgramScene(workParams);
                Thread.Sleep(_requestDelay);

                workParams = GetWorkParams();
                
                if (workParams.WorkProgram != testValue)
                {
#if DEBUG
                    Console.WriteLine($"SelfTest: {workParams.IP}:\t WorkProgram test fail!");
#endif
                    result = false;
                }

                if (workParams.ZonesSensorMode != testValue /*|| workParams.SceneMode != testValue || workParams.AlarmInfraMode != testValue*/)      // TODO: PC1800 UNUSED
                {
#if DEBUG
                    Console.WriteLine($"SelfTest: {workParams.IP}:\t ZonesWorkMode test fail!");
#endif
                    result = false;
                }

                if (workParams.WorkingFreq != workingFreq)
                {
#if DEBUG
                    Console.WriteLine($"SelfTest: {workParams.IP}:\t WorkingFreq test fail!");
#endif
                    result = false;
                }

                if (workParams.AlarmDuration != testValue || workParams.AlarmVolume != testValue ||
                    workParams.AlarmTone != testValue)
                {
#if DEBUG
                    Console.WriteLine($"SelfTest: {workParams.IP}:\t AlarmParams test fail!");
#endif
                    result = false;
                }
            }
            
            void HandTest()
            {
                testValue = 0x09;
                
                SetWorkParams(workParams);
                Thread.Sleep(_requestDelay);

                workParams.WorkProgram = testValue;
                SetWorkProgramScene(workParams);
                
                ClearPassageCount();
            }
        }

        private void ParseAlarmMode(WorkParams workParams, byte alarmByte86)
        {
            byte zoneMode;
            byte alarmZoneMode;
            byte maxZoneMode;
            byte alarmLampSwapMode;

            (zoneMode, alarmZoneMode, maxZoneMode, alarmLampSwapMode) = Parse86ByteAlarmMode(alarmByte86);

            workParams.ZonesSensorMode = zoneMode;
            workParams.ZoneMode = ParseZoneMode(zoneMode, workParams.ModelId);
            workParams.AlarmInfraMode = alarmZoneMode;
            workParams.MaxZoneMode = maxZoneMode;
            workParams.AlarmLampSwapMode = alarmLampSwapMode;
        }

        private string ParseZoneMode(byte zoneMode, byte modelId)
        {
            try
            {
                return Constants.Models.Values.FirstOrDefault(i => i.ModelId == modelId).AvailableZonesCount[zoneMode].ToString();

                switch ((Constants.Model)modelId)
                {
                    case Constants.Model.PC1800MKZ:
                    case Constants.Model.PC1800MKX:
                        return Constants.ZoneMode1800[zoneMode];
                    case Constants.Model.PC4400MKZ:
                    case Constants.Model.PC4400MKX:
                        return Constants.ZoneMode4400[zoneMode];
                    case Constants.Model.PC3300M:
                        return Constants.ZoneMode3300[zoneMode];
                    case Constants.Model.PC600MKX:
                    case Constants.Model.PC600MKZ:
                    default:
                        return "6";
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"EX: ParseZoneMode: unknown zone mode {zoneMode} for modelId = {modelId}!");
                return "0";
            }
        }

        private byte ParseMaxZoneMode(byte maxZoneMode, byte modelId)
        {
            try
            {
                switch ((Constants.Model)modelId)
                {
                    case Constants.Model.PC1800MKZ:
                    case Constants.Model.PC1800MKX:
                        return Convert.ToByte(Constants.ZoneMode1800[maxZoneMode]);
                    case Constants.Model.PC4400MKZ:
                    case Constants.Model.PC4400MKX:
                        return Convert.ToByte(Constants.ZoneMode4400[maxZoneMode]);
                    case Constants.Model.PC3300M:
                        return Convert.ToByte(Constants.ZoneMode3300[maxZoneMode]);
                    case Constants.Model.PC600MKX:
                    case Constants.Model.PC600MKZ:
                    default:
                        return Convert.ToByte("6");
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"EX: ParseZoneMode: unknown zone mode {maxZoneMode} for modelId = {modelId}!");
                return Convert.ToByte("0");
            }
        }
        
        private static byte PackAlarmMode(WorkParams workParams)
        {
            return (byte)((workParams.ZonesSensorMode << 6) + (workParams.AlarmInfraMode << 4) + (workParams.MaxZoneMode << 2) + workParams.AlarmLampSwapMode);
        }

        private (byte, byte, byte, byte) Parse86ByteAlarmMode(byte byte86)
        {
            return ((byte, byte, byte, byte))(byte86 >> 6, (byte86 & 0b00110000) >> 4, (byte86 & 0b00001100) >> 2, byte86 & 0b00000011);
        }

        public void TryGetPassageCount()
        {
            var result = ExecuteGetCommand(Constants.GetPassageCountD.code);
        }
    }
}
