using System;

namespace Hwl.RabbitMQ.Core
{
    public interface ITypeExchangeProvider
    {
        string GetTypeExchange(Type eventType);
    }
}
