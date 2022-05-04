using Hwl.RabbitMQ.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hwl.RabbitMQ
{
    public interface IManagementRabbitMq
    {
        List<ConfigMessage> GetConfigMessage(string routingkey = null);
        Task<string> DuplicateConsumer(string routingKey);
        Task<string> DuplicateConsumer(string routingKey, string consumerName);
        Task PublishAsync(Type eventType, object eventData);
        Task Unsubscribe(Type eventType, IMessageHandlerFactory factory);
        Task Unsubscribe(Type messageType);
        Task SubscribeHandlers(Type type);
        Task SetConsumer(
            string consumerName,
            string exchangeName,
            string typeExchange,
            string queueName,
            string deadLetterExchangeName,
            string deadLetterQueueName,
            string connectionName,
            string routingKey,
            string fullNameMsg,
            Type typeMsg,
            bool autoBinding);
        Task RemoveConsumer(string consumerName, bool isRemoveHander = false);
        Task<List<string>> GetNamesConsumer();
        Task Binding(string routingKey, string consumerName);
        Task UnBinding(string routingKey, string consumerName);
        Task<ConfigMessage> GetConfigMessageDynamic(string routingKey);
        Task<bool> SetConfigMessageDynamic(string routingKey, ConfigMessage configMessage);
        Task PublishAsync(string routingKey, object eventData, Dictionary<string, object> headersArguments = null);
        Task PublishAsync(ConfigMessage configMessage, object eventData, Dictionary<string, object> headersArguments = null);
        Task<List<ConfigMessage>> GetConfigMessageDynamic(Func<ConfigMessage, bool> predicate);
        Task<string> AddConsumerDynamic(
            string consumerName,
            string exchangeName,
            string typeExchange,
            string queueName,
            string deadLetterExchangeName,
            string deadLetterQueueName,
            string connectionName,
            string routingKey,
            bool autoBinding);
        Task RemoveConsumer(string consumerName, string queueName, bool isRemoveHander = false);
    }
}
