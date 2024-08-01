using System;
using System.Collections.Generic;
using System.Text;

namespace MqttNetDI.Client.HeartBeat
{
    public interface IDynamicSubManagerService
    {
        /// <summary>
        /// <ClientId,Timestamp>
        /// </summary>
        Dictionary<string, ClientState> HeartBeatList { get; set; }

        IEnumerable<ClientTopic> ClientTopics { get; }
    }
}
