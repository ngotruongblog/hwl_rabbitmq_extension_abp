using Hwl.RabbitMQ.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Volo.Abp;
using Volo.Abp.Modularity;
using Volo.Abp.RabbitMQ;
using Volo.Abp.Reflection;

namespace Hwl.RabbitMQ
{
    [DependsOn(typeof(AbpRabbitMqModule))]
    public class HwlRabbitMqModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            AddEventHandlers(context.Services);
            AddConfigMessages(context.Services);
        }

        private static void AddEventHandlers(IServiceCollection services)
        {
            var handlers = new List<Type>();

            services.OnRegistred(context =>
            {
                if (ReflectionHelper.IsAssignableToGenericType(context.ImplementationType, typeof(IHandleMessageGeneric<>)))
                {
                    handlers.Add(context.ImplementationType);
                }
            });
            services.Configure<HandleMessageOptions>(options =>
            {
                options.Handlers.AddIfNotContains(handlers);
            });
        }

        private static void AddConfigMessages(IServiceCollection services)
        {
            var res = new List<ConfigMessage>();
            services.OnRegistred(context =>
            {
                if (IsInterfaceConfigMessage(context.ImplementationType))
                {
                    var routingKey = RoutingKeyAttribute.GetRoutingKeyOrDefault(context.ImplementationType);
                    var queueName = QueueNameAttribute.GetQueueNameOrDefault(context.ImplementationType);
                    var exchangeName = ExchangeNameAttribute.GetExchangeNameOrDefault(context.ImplementationType);
                    var deadLetterExchangeName = ExchangeDeadLetterNameAttribute.GetExchangeDeadLetterNameOrDefault(context.ImplementationType);
                    var deadLetterQueueName = QueueDeadLetterNameAttribute.GetQueueDeadLetterNameOrDefault(context.ImplementationType);
                    var connectionName = ConnectionNameAttribute.GetConnectionNameOrDefault(context.ImplementationType);
                    var consumerName = ConsumerNameAttribute.GetConsumerNameOrDefault(context.ImplementationType);
                    var typeExchange = TypeExchangeAttribute.GetTypeExchangeOrDefault(context.ImplementationType);
                    var fullName = context.ImplementationType.FullName;
                    var autoBinding = AutoBindingAttribute.GetAutoBindingOrDefault(context.ImplementationType);

                    var cfg = new ConfigMessage
                    {
                        RoutingKey = routingKey,
                        QueueName = queueName,
                        ExchangeName = exchangeName,
                        ConnectionName = connectionName,
                        DeadLetterExchangeName = deadLetterExchangeName,
                        DeadLetterQueueName = deadLetterQueueName,
                        ConsumerName = consumerName,
                        TypeExchange = typeExchange,
                        FullName = fullName,
                        MessageType = context.ImplementationType,
                        AutoBinding = autoBinding
                    };
                    res.AddIfNotContains(cfg);
                }
            });
            services.Configure<ConfigMessageOptions>(options =>
            {
                var tmp = new List<ConfigMessage>();
                foreach (var item in res)
                {
                    var flag = tmp.Where(x => x.RoutingKey == item.RoutingKey && x.ConsumerName == item.ConsumerName).Any();
                    if (!flag)
                    {
                        tmp.Add(item);
                    }
                }
                options.ConfigMsgs.AddIfNotContains(tmp);
            });
        }

        private static bool IsInterfaceConfigMessage(Type givenType)
        {
            var interfaces = givenType.GetInterfaces();
            foreach (var @interface in interfaces)
            {
                if (typeof(IConfigMessage).GetTypeInfo().IsAssignableFrom(@interface))
                {
                    return true;
                }
            }
            return false;
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            context.ServiceProvider.GetRequiredService<ManagementRabbitMq>().Initialize();
        }
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<RetryMessageOptions>(options =>
            {
                context.Services.ExecutePreConfiguredActions(options);
            });
        }
    }
}
