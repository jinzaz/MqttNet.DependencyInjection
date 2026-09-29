using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MqttNetDI.Client.HeartBeat
{
    public interface IDynamicSubManagerService
    {
        /// <summary>
        /// &lt;DeviceNo, ClientState&gt;，由 MQTT 接收线程与后台检查线程并发访问
        /// </summary>
        ConcurrentDictionary<string, ClientState> HeartBeatList { get; }

        IEnumerable<ClientTopic> ClientTopics { get; }
    }
}
