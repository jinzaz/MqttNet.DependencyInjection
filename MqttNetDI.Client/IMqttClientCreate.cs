using MQTTnet.Client;

namespace MqttNetDI.Client
{
    public interface IMqttClientCreate
    {
        IMqttClient mqttClient { get; }
        MqttClientOptions mqttClientOptions { get; set; }
    }
}
