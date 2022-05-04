using System;

namespace Hwl.RabbitMQ.Core
{
    public class ConfigMessage
    {
        public string ExchangeName { get; set; }
        public string QueueName { get; set; }
        public string RoutingKey { get; set; }
        public string DeadLetterExchangeName { get; set; }
        public string DeadLetterQueueName { get; set; }
        public string ConnectionName { get; set; }
        public string ConsumerName { get; set; }
        public string TypeExchange { get; set; }
        public string FullName { get; set; }
        public Type MessageType { get; set; }
        public bool AutoBinding { get; set; }
    }
}
