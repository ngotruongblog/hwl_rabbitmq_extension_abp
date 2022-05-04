using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;
using Volo.Abp.RabbitMQ;

namespace Hwl.RabbitMQ
{
    [Dependency(ReplaceServices = true)]
    [ExposeServices(typeof(IRabbitMqMessageConsumerFactory), typeof(RabbitMqMessageConsumerFactoryExtension))]
    public class RabbitMqMessageConsumerFactoryExtension : RabbitMqMessageConsumerFactory, IRabbitMqMessageConsumerFactory
    {
        public RabbitMqMessageConsumerFactoryExtension(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
        }
        public new IRabbitMqMessageConsumer Create(
            ExchangeDeclareConfiguration exchange,
            QueueDeclareConfiguration queue,
            string connectionName = null)
        {
            var consumer = ServiceScope.ServiceProvider.GetRequiredService<RabbitMqMessageConsumerExtension>();
            consumer.Initialize(exchange, queue, connectionName);
            return consumer;
        }
    }
}
