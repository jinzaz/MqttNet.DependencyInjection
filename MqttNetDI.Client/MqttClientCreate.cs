using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using System;

namespace MqttNetDI.Client
{
    public class MqttClientCreate : IMqttClientCreate
    {
        public IMqttClient mqttClient { get; set; }
        public MqttClientOptions mqttClientOptions { get; set; }
        public MqttClientCreate(IOptions<MqttClientConfig> options)
        {
            var config = options.Value;
            if (string.IsNullOrWhiteSpace(config.Server))
                throw new ArgumentException("MqttClientConfig.Server 不能为空");

            mqttClient = new MqttFactory().CreateMqttClient();

            var optionsBuilder = new MqttClientOptionsBuilder()
                .WithTcpServer(config.Server, config.Port)
                .WithClientId(config.ClientId)
                .WithKeepAlivePeriod(config.KeepAlivePeriod)
                .WithCleanSession(config.CleanSession)
                .WithProtocolVersion(config.ProtocolVersion)
                .WithTimeout(config.Timeout);

            if (!string.IsNullOrEmpty(config.UserName))
                optionsBuilder.WithCredentials(config.UserName, config.Passowrd);

            if (config.UseTls)
                optionsBuilder.WithTlsOptions(o => o.UseTls());

            mqttClientOptions = optionsBuilder.Build();
        }
    }
}
