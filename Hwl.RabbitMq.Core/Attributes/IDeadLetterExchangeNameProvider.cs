using System;

namespace Hwl.RabbitMQ.Core
{
    public interface IDeadLetterExchangeNameProvider
    {
        string GetDeadLetterExchangeName(Type eventType);
    }
}
