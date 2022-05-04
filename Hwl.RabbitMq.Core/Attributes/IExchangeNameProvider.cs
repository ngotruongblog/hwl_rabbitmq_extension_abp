using System;

namespace Hwl.RabbitMQ.Core
{
    public interface IExchangeNameProvider
    {
        string GetExchangeName(Type eventType);
    }
}
