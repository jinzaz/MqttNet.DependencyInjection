using MQTTnet;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace MqttNetDI.Server
{
    public interface IMqttServerEventHandler
    {
        void SubScribedTopic(MqttServerClientSubscribedTopicEventArgs args);
        void UnScribedTopic(MqttServerClientUnsubscribedTopicEventArgs args);
        void MessageReceived(MqttApplicationMessageReceivedEventArgs args);
        void ClientConnected(MqttServerClientConnectedEventArgs args);
        void ClientDisConnected(MqttServerClientDisconnectedEventArgs args);
    }
}
