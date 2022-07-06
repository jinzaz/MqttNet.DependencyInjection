using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MqttNetDI.Client;
using MqttNetDI.Client.HeartBeat;
using System;
using System.Collections.Generic;
using System.Text;

namespace MqttNet.DependencyInjection
{
    public sealed class MqttClientBuilder
    {

        private IServiceCollection Services { get; }
        public MqttClientBuilder(IServiceCollection services)
        {
            Services = services;
        }
        public MqttClientBuilder AddEventHandler<T>() where T : AbsractClientHandler
        {
            Services.TryAddTransient<IMqttClientEventHandler, T>();

            Services.AddSingleton<DynamicSubManagerService>();
            Services.AddHostedService(sp => sp.GetRequiredService<DynamicSubManagerService>());
            Services.AddSingleton<IDynamicSubManagerService>(sp => sp.GetRequiredService<DynamicSubManagerService>());

            Services.AddSingleton<MqttClientService>();
            Services.AddHostedService(sp => sp.GetRequiredService<MqttClientService>());
            Services.AddSingleton<IMqttClientService>(sp => sp.GetRequiredService<MqttClientService>());


            return this;
        }
        /// <summary>
        /// 启用并设置心跳
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public MqttClientBuilder WithHeartBeat(Action<HeartBeatOption> action)
        {
            var options = new HeartBeatOption();
            action(options);
            Services.Configure(action);
            Services.AddSingleton<HeartBeatService>();
            Services.AddHostedService(sp => sp.GetRequiredService<HeartBeatService>());
            Services.AddSingleton<IHeartBeatService>(sp => sp.GetRequiredService<HeartBeatService>());

            return this;
        }
        /// <summary>
        /// 启动并设置动态订阅
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public MqttClientBuilder WithDynamicScribe(Action<DynamicSubOption> action)
        {
            var options = new DynamicSubOption();
            action(options);
            Services.Configure(action);
            return this;
        }

    }
}
