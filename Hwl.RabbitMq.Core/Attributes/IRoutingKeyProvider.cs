using System;

namespace Hwl.RabbitMQ.Core
{
    public interface IRoutingKeyProvider
    {
        string GetRoutingKey(Type eventType);
    }
}
