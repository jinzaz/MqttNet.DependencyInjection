using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace MqttNetDI.Server
{
    public interface IMqttServerCreate
    {
        IMqttServer mqttServer { get; set; }
        IMqttServerOptions mqttServerOptions { get; set; }

    }
}
