using System;

namespace Hwl.RabbitMQ.Core
{
    public interface IDeadLetterQueueNameProvider
    {
        string GetDeadLetterQueueName(Type eventType);
    }
}
