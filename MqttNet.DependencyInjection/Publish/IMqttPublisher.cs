using System;
using System.Threading;
using System.Threading.Tasks;

namespace MqttNet.DependencyInjection.Publish
{
    public interface IMqttPublisher
    {
        /// <summary>
        /// 发送信息
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task PublishAsync(string topic, string message, CancellationToken cancellationToken = default);

    }
}
