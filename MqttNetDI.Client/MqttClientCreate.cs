using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace MqttNetDI.Client
{
    public class MqttClientCreate : IMqttClientCreate
    {
        private readonly MqttClientConfig _mqttClientConfig;

        public IMqttClient mqttClient { get; set; }
        public MqttClientOptions mqttClientOptions { get; set; }
        public MqttClientCreate(IOptions<MqttClientConfig> options)
        {
            _mqttClientConfig = options.Value;

            // 1. 创建 MQTT 客户端
            mqttClient = new MqttFactory().CreateMqttClient();
            // 2 . 设置 MQTT 客户端选项
            MqttClientOptionsBuilder optionsBuilder = new MqttClientOptionsBuilder();

            // 设置服务器端地址
            optionsBuilder.WithTcpServer(_mqttClientConfig.Server, _mqttClientConfig.Port);

            // 设置鉴权参数
            optionsBuilder.WithCredentials(_mqttClientConfig.UserName, _mqttClientConfig.Passowrd);

            // 设置客户端序列号
            optionsBuilder.WithClientId(_mqttClientConfig.ClientId);

            optionsBuilder.WithKeepAlivePeriod(TimeSpan.FromSeconds(2000));
            optionsBuilder.WithCleanSession(true);
            optionsBuilder.WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V310);
            optionsBuilder.WithTimeout(TimeSpan.FromHours(1));
            // 创建选项
            mqttClientOptions = optionsBuilder.Build();
        }


    }
}
