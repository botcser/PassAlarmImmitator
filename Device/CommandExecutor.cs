using Device;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

#nullable disable

namespace CommandTransmitter.Device
{
    public class CommandExecutor
    {
        public static bool IsBusy;

        private static readonly object Lock = new object();

        [JsonProperty]
        protected readonly INetworkProtoDual NetworkProto;
        protected readonly IDatagramProto DatagramProto;

        private readonly Commands _commands;

        public CommandExecutor(IDatagramProto datagramProto, List<(short, int, string)> getCommands, List<(short, int, string)> setCommands)
        {
            DatagramProto = datagramProto;
            _commands = new Commands(datagramProto, getCommands, setCommands);
        }

        public CommandExecutor(INetworkProtoDual networkProto, IDatagramProto datagramProto, List<(short, int, string)> getCommands, List<(short, int, string)> setCommands)
        {
            NetworkProto = networkProto;
            DatagramProto = datagramProto;

            _commands = new Commands(datagramProto, getCommands, setCommands);
        }

        public static Command ExecuteCommonCommand(Command command)
        {
            var timer = new Stopwatch(); timer.Start();

            lock (Lock)
            {
                try
                {
                    IsBusy = true;
                    INetworkProtoDual.SendAndGetCommon(command);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"EX: ExecuteCommonCommand: {e.Message}");
                }
                finally
                {
                    IsBusy = false;
                    timer.Stop();
                    Console.WriteLine($"ExecuteCommonCommand: command {command.GUID} complete for {timer.ElapsedMilliseconds}.ms!\n");
                }
            }

            return command;
        }

        public byte[] ExecuteGetCommand(short commandCode, byte[] args = null)
        {
            try
            {
                return DatagramProto.GetResult(commandCode, ExecuteGetCommandRaw(commandCode, args));
            }
            catch (Exception e)
            {
                Console.WriteLine($"EX: ExecuteGetCommand: parse by command {_commands.GetCommandsList.FirstOrDefault(i => i.CommandCode == commandCode)?.Name}: {e.Message}!");
            }

            return Array.Empty<byte>();
        }

        public byte[] ExecuteSetCommandRaw(short commandCode, byte[] args)
        {
            var command = _commands.SetCommandsList.FirstOrDefault(i => i.CommandCode == commandCode) ?? new Command(commandCode);

            return ExecuteCommand(command, args);
        }

        public byte[] ExecuteGetCommandRaw(short commandCode, byte[] args = null)
        {
            var command = _commands.GetCommandsList.FirstOrDefault(i => i.CommandCode == commandCode) ?? new Command(commandCode);

            return ExecuteCommand(command, args);
        }

        private byte[] ExecuteCommand(Command command, byte[] args = null)
        {
            if (command == null)
            {
                throw new Exception($"EX: ExecuteCommand: I haven't command to do \"unknown command\"!");
            }

            if (args != null)
            {
                command.DatagramRequest = DatagramProto.MakeRequestDatagram(command.CommandCode, args);
            }

            if (command?.DatagramRequest == null || command.DatagramRequest.Length == 0)
            {
                throw new Exception($"EX: ExecuteCommand: I haven't datagram to do {(command?.Name ?? $"unknown command {command.CommandCode}")}!");
            }

            if (command.NeedResponse)
            {
                return NetworkProto.SendAndGet(command.DatagramRequest, command.ResponseLength);
            }
            else
            {
                return NetworkProto.Send(command.DatagramRequest) ? new byte[] { } : null;
            }
        }

        public void ScanCommands(byte startCode, byte endCode)
        {
            for (short code = startCode; code <= endCode; code++)
            {
                var command = new Command(code)
                {
                    NeedResponse = true,
                    DatagramRequest = DatagramProto.MakeRequestDatagram(code)
                };

                ExecuteCommand(command);
                Thread.Sleep(150);
            }
        }
    }
}