using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MqttNetDI.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace MqttNet.DependencyInjection
{
    public sealed class MqttServerBuilder
    {
        private IServiceCollection Services { get; }
        public MqttServerBuilder(IServiceCollection services)
        {
            Services = services;
        }
        public MqttServerBuilder AddEventHandler<T>() where T : AbsractServerHandler
        {
            Services.TryAddTransient<IMqttServerEventHandler, T>();
            Services.AddSingleton<MqttServerService>();
            Services.AddHostedService(sp => sp.GetRequiredService<MqttServerService>());
            Services.AddSingleton<IMqttServerService>(sp => sp.GetRequiredService<MqttServerService>());
            return this;
        }
    }
}
