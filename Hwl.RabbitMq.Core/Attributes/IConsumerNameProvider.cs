using System;

namespace Hwl.RabbitMQ.Core
{
    public interface IConsumerNameProvider
    {
        string GetConsumerName(Type eventType);
    }
}
