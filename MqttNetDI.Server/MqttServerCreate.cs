using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace MqttNetDI.Server
{
    public class MqttServerCreate : IMqttServerCreate
    {
        public IMqttServer mqttServer { get; set; }
        public IMqttServerOptions mqttServerOptions { get; set; }
        private readonly MqttServerConfig _mqttServerConfig;
        public MqttServerCreate(IOptions<MqttServerConfig> options)
        {
            _mqttServerConfig = options.Value;
            // 1. 创建 MQTT 连接验证，用于连接鉴权
            MqttServerConnectionValidatorDelegate connectionValidatorDelegate = new MqttServerConnectionValidatorDelegate(
                p =>
                {
                    // p 表示正在发起的一个链接的上下文

                    // 客户端 id 验证
                    // 大部分情况下，我们应该使用设备识别号来验证
                    if (p.ClientId == _mqttServerConfig.ClientId)
                    {
                        // 用户名和密码验证
                        // 大部分情况下，我们应该使用客户端加密 token 验证，也就是可客户端 ID 对应的密钥加密后的 token
                        if (p.Username != _mqttServerConfig.UserName && p.Password != _mqttServerConfig.Passowrd)
                        {
                            // 验证失败，告诉客户端，鉴权失败
                            p.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.BadUserNameOrPassword;
                        }
                    }
                }
            );


            // 2. 创建 MqttServerOptions 的实例，用来定制 MQTT 的各种参数
            MqttServerOptions serverOptions = new MqttServerOptions();

            // 3. 设置各种选项
            // 客户端鉴权
            serverOptions.ConnectionValidator = connectionValidatorDelegate;

            // 设置服务器端地址和端口号
            serverOptions.DefaultEndpointOptions.Port = _mqttServerConfig.Port;

            // 4. 创建 MqttServer 实例
            mqttServer = new MqttFactory().CreateMqttServer();
            mqttServerOptions = serverOptions;
        }
    }
}
