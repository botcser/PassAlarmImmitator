using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.Sockets;
using Extensions;

namespace Device
{
    public class Command
    {
        public short CommandCode;
        public DateTime RequestDateTime;
        public DateTime ResponseDateTime;
        public ProtocolType Protocol;
        public double DelayMilliseconds;
        public int ResponseLength;
        public bool NeedResponse;
        public bool Success = true;
        public string Ip;
        public string Port;
        public string Mac;
        public byte[] Result;
        public string Name = "";
        public string GUID = Guid.NewGuid().ToString();
        public List<string> ResponsePorts = new List<string>();

        private byte[] _datagramRequest;

        [JsonProperty]
        private List<byte[]> _datagramResponses = new List<byte[]>();

        public byte[] DatagramRequest
        {
            get
            {
                RequestDateTime = DateTime.UtcNow;

                return _datagramRequest;
            }
            set => _datagramRequest = value;
        }

        public List<byte[]> DatagramResponses
        {
            set
            {
                ResponseDateTime = DateTime.UtcNow;
                DelayMilliseconds = (ResponseDateTime - RequestDateTime).TotalMilliseconds;
                _datagramResponses = value;
            }
        }

        public Command() { }

        public Command(byte[] datagramRequest, string ip, string port, ProtocolType protocol)
        {
            Ip = ip;
            Port = port;
            DatagramRequest = datagramRequest;
            Protocol = protocol;
        }

        public Command(short commandCode, string name = "")
        {
            CommandCode = commandCode;
            Name = name.IsNullOrEmpty() ? commandCode.ToString() : name;
        }

        public Command(byte[] datagramRequest, short commandCode, string name = "")
        {
            DatagramRequest = datagramRequest;
            CommandCode = commandCode;
            Name = name.IsNullOrEmpty() ? commandCode.ToString() : name;
        }

        public Command(byte[] datagramRequest, int responseLength, short commandCode, string name = "")
        {
            DatagramRequest = datagramRequest;
            ResponseLength = responseLength;
            NeedResponse = responseLength > 0;
            CommandCode = commandCode;
            Name = name.IsNullOrEmpty() ? commandCode.ToString() : name;
        }

        public void AddResponse(byte[] response, string portReceived)
        {
            ResponseDateTime = DateTime.UtcNow;
            DelayMilliseconds = (ResponseDateTime - RequestDateTime).TotalMilliseconds;
            _datagramResponses.Add(response);
            ResponsePorts.Add(portReceived);
        }

        public byte[] GetResponse(int index)
        {
            return _datagramResponses[index];
        }
    }
}
