using System;
using System.Collections.Generic;
using System.Text;

namespace MqttNetDI.Server
{
    public class MqttServerConfig
    {
        public string ClientId { get; set; }
        public string UserName { get; set; }
        public string Passowrd { get; set; }
        public int Port { get; set; }
    }
}
