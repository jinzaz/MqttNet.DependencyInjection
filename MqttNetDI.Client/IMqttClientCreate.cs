using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace MqttNetDI.Client
{
    public interface IMqttClientCreate
    {
        IMqttClient mqttClient { get; }
        IMqttClientOptions mqttClientOptions { get; set; }
    }
}
