using MQTTnet.Protocol;
using System.Threading;
using System.Threading.Tasks;

namespace MqttNet.DependencyInjection.Publish
{
    public interface IMqttPublisher
    {
        /// <summary>
        /// 发送信息（QoS 0，不保留）
        /// </summary>
        Task PublishAsync(string topic, string message, CancellationToken cancellationToken = default);

        /// <summary>
        /// 发送信息
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="message">消息内容</param>
        /// <param name="qos">服务质量等级</param>
        /// <param name="retain">是否保留消息</param>
        /// <param name="cancellationToken"></param>
        Task PublishAsync(string topic, string message, MqttQualityOfServiceLevel qos, bool retain = false, CancellationToken cancellationToken = default);
    }
}
