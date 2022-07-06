using MqttNet.DependencyInjection.Publish;
using MQTTnet;
using MQTTnet.Client;
using MqttNetDI.Client;
using MqttNetDI.Client.HeartBeat;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MqttNet.DependencyInjection
{
    public abstract class AbsractClientHandler : IMqttClientEventHandler
    {

        public readonly IMqttPublisher _mqttPublisher;

        public AbsractClientHandler(IMqttPublisher mqttPublisher)
        {
            _mqttPublisher = mqttPublisher;
        }
        /// <summary>
        /// 心跳接收处理
        /// </summary>
        /// <param name="heartBeatArgs"></param>
        public abstract void HeartBeatReceived(HeartBeatArgs heartBeatArgs);
        /// <summary>
        /// 信息接收处理
        /// </summary>
        /// <param name="args"></param>
        public abstract void MessageReceived(MessageReceiveArgs args);
        /// <summary>
        /// 主题订阅
        /// </summary>
        /// <param name="mqttClient"></param>
        /// <returns></returns>
        public abstract void SetTopic(out List<ClientTopic> mqttClient);

    }
}
