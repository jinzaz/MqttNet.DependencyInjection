using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Server;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MqttNetDI.Server
{
    public class MqttServerService : BackgroundService, IMqttServerService
    {
        private readonly IMqttServerEventHandler _mqttServerEventHandler;
        private readonly IMqttServerCreate _mqttServerCreate;

        public MqttServerService(IServiceProvider serviceProvider)
        {
            _mqttServerEventHandler = serviceProvider.GetRequiredService<IMqttServerEventHandler>();
            _mqttServerCreate = serviceProvider.GetRequiredService<IMqttServerCreate>();
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ServerStart();
        }
        public async Task ServerStart()
        {
            try
            {
                // 5. 设置 MqttServer 的属性
                // 设置消息订阅通知
                _mqttServerCreate.mqttServer.ClientSubscribedTopicHandler = new MqttServerClientSubscribedTopicHandlerDelegate(_mqttServerEventHandler.SubScribedTopic);
                // 设置消息退订通知
                _mqttServerCreate.mqttServer.ClientUnsubscribedTopicHandler = new MqttServerClientUnsubscribedTopicHandlerDelegate(_mqttServerEventHandler.UnScribedTopic);
                // 设置消息处理程序
                _mqttServerCreate.mqttServer.UseApplicationMessageReceivedHandler(_mqttServerEventHandler.MessageReceived);
                // 设置客户端连接成功后的处理程序
                _mqttServerCreate.mqttServer.UseClientConnectedHandler(_mqttServerEventHandler.ClientConnected);
                // 设置客户端断开后的处理程序
                _mqttServerCreate.mqttServer.UseClientDisconnectedHandler(_mqttServerEventHandler.ClientDisConnected);

                // 启动服务器
                await _mqttServerCreate.mqttServer.StartAsync(_mqttServerCreate.mqttServerOptions);


                Console.WriteLine("服务器启动成功！直接按回车停止服务");

            }
            catch (Exception ex)
            {
                Console.Write($"服务器启动失败:{ex}");
            }
        }
    }
}
