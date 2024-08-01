using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;
using MqttNetDI.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MqttNetDI.Client.HeartBeat
{
    /// <summary>
    /// 心跳后台处理服务
    /// </summary>
    public class HeartBeatService : BackgroundService, IHeartBeatService
    {
        private readonly IMqttClientCreate _mqttClientCreate;
        private readonly HeartBeatOption _options;
        public HeartBeatService(IMqttClientCreate mqttClientCreate, IOptions<HeartBeatOption> options)
        {
            _mqttClientCreate = mqttClientCreate;
            _options = options.Value;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_options.HeartBeatinterval, stoppingToken);
                HeartBeatArgs heartBeatInfo = new HeartBeatArgs()
                {
                    DeviceNo = _options.DeviceNo,
                    HeartBeatNo = Guid.NewGuid().ToString("N"),
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    HeartBeatinterval = _options.HeartBeatinterval,
                    CustmData = _options.CustmData,
                };
                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(_options.PubHeartBeatTopic)       // 主题
                    .WithPayload(JsonConvert.SerializeObject(heartBeatInfo))   // 消息
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)  // qos
                    .Build();
                await _mqttClientCreate.mqttClient.PublishAsync(applicationMessage, stoppingToken);
            }
        }
    }
}
