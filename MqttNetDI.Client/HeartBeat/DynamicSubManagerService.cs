using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MqttNetDI.Client.HeartBeat
{
    /// <summary>
    /// 动态订阅后台处理服务
    /// </summary>
    public class DynamicSubManagerService : BackgroundService, IDynamicSubManagerService
    {
        public Dictionary<string, ClientState> HeartBeatList { get; set; }
        public List<ClientTopic> ClientTopics { get; set; }
        private readonly IMqttClientCreate _MqttClientCreate;
        private readonly DynamicSubOption _options;
        private readonly IMqttClientEventHandler _mqttClientEventHandler;
        private List<ClientTopic> clientTopics;
        public DynamicSubManagerService(IMqttClientCreate mqttClientCreate, IMqttClientEventHandler mqttClientEventHandler, IOptions<DynamicSubOption> options)
        {
            _MqttClientCreate = mqttClientCreate;
            _mqttClientEventHandler = mqttClientEventHandler;
            _mqttClientEventHandler.SetTopic(out clientTopics);
            _options = options.Value;
            if (HeartBeatList == null)
            {
                HeartBeatList = new Dictionary<string, ClientState>();
            }

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_options.EnableDynamicSubcribe)
            {

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(_options.DynamicSubcribeinterval, stoppingToken);
                        if (HeartBeatList.Count > 0)
                        {
                            for (int i = 0; i < HeartBeatList.Count; i++)
                            {
                                try
                                {
                                    (string key, ClientState value) = HeartBeatList.ElementAt(i);
                                    if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - value.Timestamp <= (_options.DynamicSubcribeinterval + value.HeartBeatinterval).TotalMilliseconds)
                                    {
                                        if (!value.Online)
                                        {
                                            await Subscribe(clientTopics.Where(x => x.DeviceNo == key).Select(s => s.TopicList).First(), stoppingToken);
                                        }
                                        HeartBeatList[key].Online = true;
                                    }
                                    else
                                    {
                                        HeartBeatList[key].Online = false;
                                        await UnSubscribe(clientTopics.Where(x => x.DeviceNo == key).Select(s => s.TopicList).First(), stoppingToken);
                                        HeartBeatList.Remove(key);
                                        i--;
                                    }
                                }
                                catch (Exception)
                                {
                                    continue;
                                }

                            }

                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                }
            }
            await Task.CompletedTask;

        }
        private async Task Subscribe(IEnumerable<string> topiclist, CancellationToken cancellationToken)
        {
            foreach (var item in topiclist)
            {
                await _MqttClientCreate.mqttClient.SubscribeAsync(new MqttClientSubscribeOptionsBuilder().WithTopicFilter(item).Build(), cancellationToken);
            }

        }
        private async Task UnSubscribe(IEnumerable<string> topiclist, CancellationToken cancellationToken)
        {
            foreach (var item in topiclist)
            {
                await _MqttClientCreate.mqttClient.UnsubscribeAsync(new MqttClientUnsubscribeOptionsBuilder().WithTopicFilter(item).Build(), cancellationToken);
            }
        }
    }
}
