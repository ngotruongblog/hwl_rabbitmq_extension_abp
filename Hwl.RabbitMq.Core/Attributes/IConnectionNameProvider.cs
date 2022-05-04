using System;

namespace Hwl.RabbitMQ.Core
{
    public interface IConnectionNameProvider
    {
        string GetConnectionName(Type eventType);
    }
}
