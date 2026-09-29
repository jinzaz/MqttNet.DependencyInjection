using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MqttNetDI.Client.HeartBeat;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MqttNetDI.Client
{
    public class MqttClientService : BackgroundService, IMqttClientService
    {
        private readonly IMqttClientEventHandler _mqttClientEventHandler;
        private readonly IMqttClientCreate _mqttClientCreate;
        private readonly DynamicSubManagerService _dynamicSubManagerService;
        private readonly DynamicSubOption _options;
        private readonly MqttClientConfig _clientConfig;
        private readonly ILogger<MqttClientService> _logger;
        public MqttClientService(
            IMqttClientEventHandler mqttClientEventHandler,
            IMqttClientCreate mqttClientCreate,
            DynamicSubManagerService dynamicSubManagerService,
            IOptions<DynamicSubOption> options,
            IOptions<MqttClientConfig> clientConfig,
            ILogger<MqttClientService> logger)
        {
            _mqttClientEventHandler = mqttClientEventHandler;
            _mqttClientCreate = mqttClientCreate;
            _dynamicSubManagerService = dynamicSubManagerService;
            _options = options.Value;
            _clientConfig = clientConfig.Value;
            _logger = logger;
            if (_options.EnableDynamicSubcribe && string.IsNullOrWhiteSpace(_options.SubcribeHeartBeatTopic))
                throw new ArgumentException("启用动态订阅时 SubcribeHeartBeatTopic 不能为空");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var client = _mqttClientCreate.mqttClient;
            client.ApplicationMessageReceivedAsync += MessageReceivedAsync;
            client.DisconnectedAsync += e =>
            {
                if (e.ClientWasConnected)
                    _logger.LogWarning(e.Exception, "与 MQTT 服务器的连接断开: {Reason}", e.Reason);
                return Task.CompletedTask;
            };

            // 重连循环：MQTTnet 推荐方式，替代在 DisconnectedAsync 中重连（后者失败一次就不再重试，且停机时仍会重连）
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (!client.IsConnected)
                    {
                        await client.ConnectAsync(_mqttClientCreate.mqttClientOptions, stoppingToken);
                        await SubScribe(stoppingToken);
                        _logger.LogInformation("连接 MQTT 服务器成功");
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "连接 MQTT 服务器失败，{Interval} 后重试", _clientConfig.ReconnectInterval);
                }

                try
                {
                    await Task.Delay(_clientConfig.ReconnectInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            if (_mqttClientCreate.mqttClient.IsConnected)
                await _mqttClientCreate.mqttClient.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().Build(), cancellationToken);
        }

        private async Task MessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
        {
            string topic = args.ApplicationMessage.Topic;
            string payload = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);
            try
            {
                if (_options.EnableDynamicSubcribe && topic == _options.SubcribeHeartBeatTopic)
                {
                    HeartBeatArgs heartBeatInfo = JsonConvert.DeserializeObject<HeartBeatArgs>(payload);
                    if (string.IsNullOrEmpty(heartBeatInfo?.DeviceNo))
                    {
                        _logger.LogWarning("收到无效心跳: {Payload}", payload);
                        return;
                    }
                    // 只跟踪已配置 Topic 的设备，防止任意 DeviceNo 让字典无限增长
                    if (_dynamicSubManagerService.ClientTopics.Any(x => x.DeviceNo == heartBeatInfo.DeviceNo))
                    {
                        // 使用本地接收时间，避免设备与服务端时钟不同步导致误判离线
                        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        _dynamicSubManagerService.HeartBeatList.AddOrUpdate(
                            heartBeatInfo.DeviceNo,
                            _ => new ClientState { Online = false, Timestamp = now, HeartBeatinterval = heartBeatInfo.HeartBeatinterval },
                            (_, state) => { state.Timestamp = now; state.HeartBeatinterval = heartBeatInfo.HeartBeatinterval; return state; });
                    }
                    await _mqttClientEventHandler.HeartBeatReceivedAsync(heartBeatInfo);
                    return;
                }
                MessageReceiveArgs messageReceiveArgs = new MessageReceiveArgs(
                    topic,
                    args.ClientId,
                    payload,
                    args.ApplicationMessage.QualityOfServiceLevel,
                    args.ApplicationMessage.Retain,
                    args.ReasonCode,
                    args.ResponseUserProperties,
                    args.AcknowledgeAsync);
                await _mqttClientEventHandler.MessageReceivedAsync(messageReceiveArgs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理主题 {Topic} 的消息失败", topic);
            }
        }

        private async Task SubScribe(CancellationToken cancellationToken)
        {
            var builder = new MqttClientSubscribeOptionsBuilder();
            if (_options.EnableDynamicSubcribe)
            {
                // CleanSession 会清空服务端订阅，重连后让动态订阅服务重新订阅在线设备
                _dynamicSubManagerService.ResetOnline();
                builder.WithTopicFilter(_options.SubcribeHeartBeatTopic);
            }
            else
            {
                _mqttClientEventHandler.SetTopic(out var clientTopics);
                var topics = (clientTopics ?? Enumerable.Empty<ClientTopic>())
                    .Where(x => x.TopicList != null)
                    .SelectMany(x => x.TopicList)
                    .Distinct()
                    .ToList();
                if (topics.Count == 0)
                    return;
                foreach (var item in topics)
                    builder.WithTopicFilter(item);
            }
            await _mqttClientCreate.mqttClient.SubscribeAsync(builder.Build(), cancellationToken);
        }
    }
}
