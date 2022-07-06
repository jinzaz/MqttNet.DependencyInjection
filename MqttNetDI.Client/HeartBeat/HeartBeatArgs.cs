using System;
using System.Collections.Generic;
using System.Text;

namespace MqttNetDI.Client.HeartBeat
{
    public class HeartBeatArgs
    {
        /// <summary>
        /// 心跳唯一编码
        /// </summary>
        public string HeartBeatNo { get; set; }
        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceNo { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        public long Timestamp { get; set; }
        /// <summary>
        /// 心跳发送间隔
        /// </summary>
        public TimeSpan HeartBeatinterval { get; set; }
        /// <summary>
        /// 自定义数据
        /// </summary>
        public object CustmData { get; set; }

    }
}
