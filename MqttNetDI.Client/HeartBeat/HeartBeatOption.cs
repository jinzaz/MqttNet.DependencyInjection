using System;
using System.Collections.Generic;
using System.Text;

namespace MqttNetDI.Client.HeartBeat
{
    public class HeartBeatOption
    {
        /// <summary>
        /// /启用心跳
        /// </summary>
        public bool EnableHeartBeat { get; set; } = false;
        /// <summary>
        /// 心跳发送主题
        /// </summary>
        public string PubHeartBeatTopic { get; set; }
        public TimeSpan HeartBeatinterval { get; set; }
        public string DeviceNo { get; set; }
        public object CustmData { get; set; } = null;
    }
}
