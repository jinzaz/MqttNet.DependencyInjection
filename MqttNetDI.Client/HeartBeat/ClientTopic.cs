using System;
using System.Collections.Generic;
using System.Text;

namespace MqttNetDI.Client.HeartBeat
{
    public class ClientTopic
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceNo { get; set; }
        /// <summary>
        /// 订阅主题列表
        /// </summary>
        public List<string> TopicList { get; set; }
    }
}
