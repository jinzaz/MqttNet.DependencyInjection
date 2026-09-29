using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using MqttNetDI.Client;
using System.Threading;
using System.Threading.Tasks;

namespace MqttNet.DependencyInjection.Publish
{
    public class MqttClientPublisher : IMqttPublisher
    {
        private readonly IMqttClient mqttClient;
        public MqttClientPublisher(IMqttClientCreate mqttClientCreate)
        {
            mqttClient = mqttClientCreate.mqttClient;
        }

        public async Task PublishAsync(string topic, string message, CancellationToken cancellationToken = default) =>
            await PublishAsync(topic, message, MqttQualityOfServiceLevel.AtMostOnce, false, cancellationToken);

        public async Task PublishAsync(string topic, string message, MqttQualityOfServiceLevel qos, bool retain = false, CancellationToken cancellationToken = default)
        {
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .WithQualityOfServiceLevel(qos)
                .WithRetainFlag(retain)
                .Build();

            await mqttClient.PublishAsync(applicationMessage, cancellationToken);
        }
    }
}
