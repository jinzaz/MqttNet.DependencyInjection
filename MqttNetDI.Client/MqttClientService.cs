using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Subscribing;
using MqttNetDI.Client.HeartBeat;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MqttNetDI.Client
{
    public class MqttClientService : BackgroundService, IMqttClientService
    {
        private readonly IMqttClientEventHandler _mqttClientEventHandler;
        private readonly IMqttClientCreate _mqttClientCreate;
        private readonly IDynamicSubManagerService _dynamicSubManagerService;
        private readonly DynamicSubOption _options;
        public MqttClientService(
            IMqttClientEventHandler mqttClientEventHandler,
            IMqttClientCreate mqttClientCreate,
            IDynamicSubManagerService dynamicSubManagerService,
            IOptions<DynamicSubOption> options)
        {
            _mqttClientEventHandler = mqttClientEventHandler;
            _mqttClientCreate = mqttClientCreate;
            _dynamicSubManagerService = dynamicSubManagerService;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ConnetAsync();
        }
        public async Task ConnetAsync()
        {
            try
            {
                // 设置消息接收处理程序
                _mqttClientCreate.mqttClient.UseApplicationMessageReceivedHandler(MessageReceived);

                // 重连机制
                _mqttClientCreate.mqttClient.UseDisconnectedHandler(async e =>
                {
                    Console.WriteLine("与服务器之间的连接断开了，正在尝试重新连接");
                    // 等待 5s 时间
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    try
                    {
                        // 重新连接
                        await _mqttClientCreate.mqttClient.ConnectAsync(_mqttClientCreate.mqttClientOptions);
                        if (_options.EnableDynamicSubcribe)
                            await _mqttClientCreate.mqttClient.SubscribeAsync(new MqttClientSubscribeOptionsBuilder().WithTopicFilter(_options.SubcribeHeartBeatTopic).Build(), CancellationToken.None);
                        else
                            await SubScribe();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"重新连接服务器失败:{ex}");
                    }
                });

                // 连接到服务器
                await _mqttClientCreate.mqttClient.ConnectAsync(_mqttClientCreate.mqttClientOptions);
                if (_options.EnableDynamicSubcribe)
                    await _mqttClientCreate.mqttClient.SubscribeAsync(new MqttClientSubscribeOptionsBuilder().WithTopicFilter(_options.SubcribeHeartBeatTopic).Build(), CancellationToken.None);
                else
                    await SubScribe();
                Console.WriteLine("连接服务器成功！请输入任意内容并回车进入菜单界面");


            }
            catch (Exception ex)
            {
                Console.Write($"连接服务器失败: {ex}");
            }
        }

        private void MessageReceived(MqttApplicationMessageReceivedEventArgs args)
        {
            string topic = args.ApplicationMessage.Topic;
            string ClientId = args.ClientId;
            string payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
            if (topic == _options.SubcribeHeartBeatTopic)
            {
                HeartBeatArgs heartBeatInfo = JsonConvert.DeserializeObject<HeartBeatArgs>(payload);
                if (_dynamicSubManagerService.HeartBeatList.ContainsKey(heartBeatInfo.DeviceNo))
                {
                    _dynamicSubManagerService.HeartBeatList[heartBeatInfo.DeviceNo].Timestamp = heartBeatInfo.Timestamp;
                }
                else
                {
                    _dynamicSubManagerService.HeartBeatList.Add(heartBeatInfo.DeviceNo, new ClientState
                    {
                        Online = false,
                        Timestamp = heartBeatInfo.Timestamp,
                        HeartBeatinterval = heartBeatInfo.HeartBeatinterval,
                    });
                }
                _mqttClientEventHandler.HeartBeatReceived(heartBeatInfo);
                return;
            }
            MessageReceiveArgs messageReceiveArgs = new MessageReceiveArgs(
                topic,
                ClientId,
                payload,
                args.ApplicationMessage.QualityOfServiceLevel,
                args.ApplicationMessage.Retain,
                (CancellationToken) => { args.AcknowledgeAsync(CancellationToken); });
            _mqttClientEventHandler.MessageReceived(messageReceiveArgs);
        }

        private async Task SubScribe()
        {
            List<ClientTopic> clientTopics = new List<ClientTopic>();
            _mqttClientEventHandler.SetTopic(out clientTopics);
            List<string> topiclist = new List<string>();
            foreach (var item in clientTopics)
            {
                topiclist.AddRange(item.TopicList);
            }
            foreach (var item in topiclist)
            {
                await _mqttClientCreate.mqttClient.SubscribeAsync(new MqttClientSubscribeOptionsBuilder().WithTopicFilter(item).Build(), CancellationToken.None);
            }
        }
    }
}
