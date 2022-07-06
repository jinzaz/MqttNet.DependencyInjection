using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using MQTTnet.Server;
using MqttNetDI.Server;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MqttNet.DependencyInjection.Publish
{
    public class MqttServerPublisher : IMqttPublisher
    {
        private readonly IMqttServer mqttServer;
        public MqttServerPublisher(IServiceProvider serviceProvider)
        {
            mqttServer = serviceProvider.GetRequiredService<IMqttServerCreate>().mqttServer;
        }

        public async Task PublishAsync(string topic, string message, CancellationToken cancellationToken = default)
        {
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)       // 主题
                .WithPayload(message)   // 消息
                .WithExactlyOnceQoS()   // qos
                .WithRetainFlag()       // retain
                .Build();

            await mqttServer.PublishAsync(applicationMessage);
        }
    }
}
