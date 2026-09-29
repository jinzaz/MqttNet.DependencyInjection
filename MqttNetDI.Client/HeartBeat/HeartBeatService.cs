using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using System;
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
        private readonly ILogger<HeartBeatService> _logger;
        public HeartBeatService(IMqttClientCreate mqttClientCreate, IOptions<HeartBeatOption> options, ILogger<HeartBeatService> logger)
        {
            _mqttClientCreate = mqttClientCreate;
            _options = options.Value;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.EnableHeartBeat)
                return;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_options.HeartBeatinterval, stoppingToken);
                    // 未连接时跳过，避免异常导致 BackgroundService 终止宿主
                    if (!_mqttClientCreate.mqttClient.IsConnected)
                        continue;

                    HeartBeatArgs heartBeatInfo = new HeartBeatArgs()
                    {
                        DeviceNo = _options.DeviceNo,
                        HeartBeatNo = Guid.NewGuid().ToString("N"),
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        HeartBeatinterval = _options.HeartBeatinterval,
                        CustmData = _options.CustmData,
                    };
                    var applicationMessage = new MqttApplicationMessageBuilder()
                        .WithTopic(_options.PubHeartBeatTopic)
                        .WithPayload(JsonConvert.SerializeObject(heartBeatInfo))
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                        .Build();
                    await _mqttClientCreate.mqttClient.PublishAsync(applicationMessage, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "发送心跳失败");
                }
            }
        }
    }
}
