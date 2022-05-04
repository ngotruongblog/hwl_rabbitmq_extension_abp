using System.Collections.Generic;

namespace Hwl.RabbitMQ
{
    public interface IMessageHandlerFactory
    {
        IMessageHandlerDisposeWrapper GetHandler();
        bool IsInFactories(List<IMessageHandlerFactory> handlerFactories);
    }
}
