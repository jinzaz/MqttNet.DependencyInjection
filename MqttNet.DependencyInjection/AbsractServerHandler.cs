using MqttNet.DependencyInjection.Publish;
using MQTTnet;
using MQTTnet.Server;
using MqttNetDI.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace MqttNet.DependencyInjection
{
    public abstract class AbsractServerHandler : IMqttServerEventHandler
    {
        public readonly IMqttPublisher _mqttPublisher;
        public AbsractServerHandler(IMqttPublisher mqttPublisher)
        {
            _mqttPublisher = mqttPublisher;
        }
        /// <summary>
        /// 客户端链接
        /// </summary>
        /// <param name="args"></param>
        public abstract void ClientConnected(MqttServerClientConnectedEventArgs args);
        /// <summary>
        /// 客户端断开链接
        /// </summary>
        /// <param name="args"></param>
        public abstract void ClientDisConnected(MqttServerClientDisconnectedEventArgs args);
        /// <summary>
        /// 信息接收
        /// </summary>
        /// <param name="args"></param>
        public abstract void MessageReceived(MqttApplicationMessageReceivedEventArgs args);
        /// <summary>
        /// 客户端订阅主题
        /// </summary>
        /// <param name="args"></param>
        public abstract void SubScribedTopic(MqttServerClientSubscribedTopicEventArgs args);
        /// <summary>
        /// 客户端取消订阅
        /// </summary>
        /// <param name="args"></param>
        public abstract void UnScribedTopic(MqttServerClientUnsubscribedTopicEventArgs args);
    }
}
