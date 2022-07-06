using MQTTnet;
using MQTTnet.Client;
using MqttNetDI.Client.HeartBeat;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MqttNetDI.Client
{
    public interface IMqttClientEventHandler
    {
        void SetTopic(out List<ClientTopic> topicList);
        void HeartBeatReceived(HeartBeatArgs args);
        void MessageReceived(MessageReceiveArgs args);
    }
}
