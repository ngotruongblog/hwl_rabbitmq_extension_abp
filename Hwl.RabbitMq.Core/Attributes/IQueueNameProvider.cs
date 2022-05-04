using System;

namespace Hwl.RabbitMQ.Core
{
    public interface IQueueNameProvider
    {
        string GetQueueName(Type eventType);
    }
}
