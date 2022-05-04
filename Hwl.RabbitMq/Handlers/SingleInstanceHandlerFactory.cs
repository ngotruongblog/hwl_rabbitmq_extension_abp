using System.Collections.Generic;
using System.Linq;

namespace Hwl.RabbitMQ
{
    public class SingleInstanceHandlerFactory : IMessageHandlerFactory
    {
        /// <summary>
        /// The message handler instance.
        /// </summary>
        public IHandleMessage HandlerInstance { get; }
        public SingleInstanceHandlerFactory(IHandleMessage handler)
        {
            HandlerInstance = handler;
        }

        public IMessageHandlerDisposeWrapper GetHandler()
        {
            return new MessageHandlerDisposeWrapper(HandlerInstance);
        }

        public bool IsInFactories(List<IMessageHandlerFactory> handlerFactories)
        {
            return handlerFactories
                .OfType<SingleInstanceHandlerFactory>()
                .Any(f => f.HandlerInstance == HandlerInstance);
        }
    }
}
