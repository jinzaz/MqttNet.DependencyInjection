using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MqttNet.DependencyInjection.Publish;
using MqttNetDI.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace MqttNet.DependencyInjection
{
    public static class MqttServicesExtensions
    {
        public static MqttClientBuilder AddMqttClient(this IServiceCollection services, Action<MqttClientConfig> setupAction)
        {
            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }
            services.Configure(setupAction);
            services.TryAddSingleton<IMqttClientCreate, MqttClientCreate>();
            services.TryAddSingleton<IMqttPublisher, MqttClientPublisher>();
            return new MqttClientBuilder(services);
        }

    }
}
