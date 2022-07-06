using System;
using System.Collections.Generic;
using System.Text;

namespace MqttNetDI.Client.HeartBeat
{
    public class DynamicSubOption
    {
        /// <summary>
        /// 启用动态订阅和退订
        /// </summary>
        public bool EnableDynamicSubcribe { get; set; } = false;
        /// <summary>
        /// 接收心跳的主题
        /// </summary>
        public string SubcribeHeartBeatTopic { get; set; }
        /// <summary>
        /// 心跳检查间隔
        /// </summary>
        public TimeSpan DynamicSubcribeinterval { get; set; }
    }
}
