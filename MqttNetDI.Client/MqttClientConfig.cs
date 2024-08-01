using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MqttNetDI.Client
{
    public class MqttClientConfig
    {
        /// <summary>
        /// ClientId
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Passowrd { get; set; }
        /// <summary>
        /// 服务器地址
        /// </summary>
        public string Server { get; set; }
        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }
    }

    public class MessageReceiveArgs
    {
        /// <summary>
        /// 主题
        /// </summary>
        public string Topic { get; }
        /// <summary>
        /// 当前服务的ClientId，不是消息源的ClientId
        /// </summary>
        public string ClientId { get; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { get; }
        /// <summary>
        /// 消息等级
        /// </summary>
        public MqttQualityOfServiceLevel QosLevel { get; }
        /// <summary>
        /// Retain
        /// </summary>
        public bool Retain { get; }
        /// <summary>
        /// 消息确认方法
        /// </summary>
        public Action<CancellationToken> AcknowledgeAsync { get; }

        /// <summary>
        ///     Gets or sets the reason code which will be sent to the server.
        /// </summary>
        public MqttApplicationMessageReceivedReasonCode ReasonCode { get; }

        /// <summary>
        ///     Gets or sets the user properties which will be sent to the server in the ACK packet etc.
        /// </summary>
        public List<MqttUserProperty> ResponseUserProperties { get; }

        public MessageReceiveArgs(
            string topic, 
            string clientId, 
            string message, 
            MqttQualityOfServiceLevel qosLevel, 
            bool retain,
            MqttApplicationMessageReceivedReasonCode reasonCode,
            List<MqttUserProperty> userProperties,
            Action<CancellationToken> action)
        {
            Topic = topic;
            ClientId = clientId;
            Message = message;
            QosLevel = qosLevel;
            Retain = retain;
            ReasonCode = reasonCode;
            ResponseUserProperties = userProperties;
            AcknowledgeAsync = action;
        }
    }
}
