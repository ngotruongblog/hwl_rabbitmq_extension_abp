using System;

namespace Hwl.RabbitMQ.Core
{
    public interface IAutoBindingProvider
    {
        bool GetAutoBinding(Type eventType);
    }
}
