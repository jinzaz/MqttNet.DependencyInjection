using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using MqttNetDI.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MqttNet.DependencyInjection.Publish
{
    public class MqttClientPublisher : IMqttPublisher
    {

        private readonly IMqttClient mqttClient;
        public MqttClientPublisher(IServiceProvider serviceProvider)
        {
            mqttClient = serviceProvider.GetRequiredService<IMqttClientCreate>().mqttClient;
        }
        /// <summary>
        /// 发送信息
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task PublishAsync(string topic, string message, CancellationToken cancellationToken = default)
        {

            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)       // 主题
                .WithPayload(message)   // 消息
                .WithAtMostOnceQoS()  // qos
                .WithRetainFlag()     // retain
                .Build();

            await mqttClient.PublishAsync(applicationMessage);
        }

    }
}
