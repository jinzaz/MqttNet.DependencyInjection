using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MqttNetDI.Client.HeartBeat
{
    /// <summary>
    /// 动态订阅后台处理服务
    /// </summary>
    public class DynamicSubManagerService : BackgroundService, IDynamicSubManagerService
    {
        public ConcurrentDictionary<string, ClientState> HeartBeatList { get; } = new ConcurrentDictionary<string, ClientState>();
        public IEnumerable<ClientTopic> ClientTopics => _clientTopics;
        private readonly IMqttClientCreate _MqttClientCreate;
        private readonly DynamicSubOption _options;
        private readonly ILogger<DynamicSubManagerService> _logger;
        private readonly IEnumerable<ClientTopic> _clientTopics;
        public DynamicSubManagerService(IMqttClientCreate mqttClientCreate, IMqttClientEventHandler mqttClientEventHandler, IOptions<DynamicSubOption> options, ILogger<DynamicSubManagerService> logger)
        {
            _MqttClientCreate = mqttClientCreate;
            mqttClientEventHandler.SetTopic(out _clientTopics);
            _clientTopics = _clientTopics ?? Enumerable.Empty<ClientTopic>();
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.EnableDynamicSubcribe)
                return;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_options.DynamicSubcribeinterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                if (!_MqttClientCreate.mqttClient.IsConnected)
                    continue;

                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                foreach (var (deviceNo, state) in HeartBeatList)
                {
                    var topics = GetTopics(deviceNo);
                    try
                    {
                        if (now - state.Timestamp <= (_options.DynamicSubcribeinterval + state.HeartBeatinterval).TotalMilliseconds)
                        {
                            if (!state.Online && topics.Count > 0)
                                await Subscribe(topics, stoppingToken);
                            state.Online = true;
                        }
                        else
                        {
                            HeartBeatList.TryRemove(deviceNo, out _);
                            if (state.Online && topics.Count > 0)
                                await UnSubscribe(topics, stoppingToken);
                        }
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "设备 {DeviceNo} 动态订阅/退订失败", deviceNo);
                    }
                }
            }
        }

        /// <summary>
        /// 重连后（CleanSession 会清空订阅）将所有设备标记为离线，以便下一轮重新订阅
        /// </summary>
        internal void ResetOnline()
        {
            foreach (var state in HeartBeatList.Values)
                state.Online = false;
        }

        private List<string> GetTopics(string deviceNo) =>
            _clientTopics.Where(x => x.DeviceNo == deviceNo && x.TopicList != null).SelectMany(x => x.TopicList).ToList();

        private async Task Subscribe(IEnumerable<string> topiclist, CancellationToken cancellationToken)
        {
            var builder = new MqttClientSubscribeOptionsBuilder();
            foreach (var item in topiclist)
                builder.WithTopicFilter(item);
            await _MqttClientCreate.mqttClient.SubscribeAsync(builder.Build(), cancellationToken);
        }
        private async Task UnSubscribe(IEnumerable<string> topiclist, CancellationToken cancellationToken)
        {
            var builder = new MqttClientUnsubscribeOptionsBuilder();
            foreach (var item in topiclist)
                builder.WithTopicFilter(item);
            await _MqttClientCreate.mqttClient.UnsubscribeAsync(builder.Build(), cancellationToken);
        }
    }
}
