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
        void SetTopic(out IEnumerable<ClientTopic> topicList);
        Task HeartBeatReceivedAsync(HeartBeatArgs args);
        Task MessageReceivedAsync(MessageReceiveArgs args);
    }
}
