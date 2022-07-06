using System;
using System.Collections.Generic;
using System.Text;

namespace MqttNetDI.Client.HeartBeat
{
    public class ClientState
    {
        public long Timestamp { get; set; }
        public bool Online { get; set; }
        public TimeSpan HeartBeatinterval { get; set; }

    }
}
