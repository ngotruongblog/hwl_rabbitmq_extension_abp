using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hwl.RabbitMQ
{
    public class IocMessageHandlerFactory : IMessageHandlerFactory, IDisposable
    {
        public Type HandlerType { get; }

        protected IServiceScopeFactory ScopeFactory { get; }

        public IocMessageHandlerFactory(IServiceScopeFactory scopeFactory, Type handlerType)
        {
            ScopeFactory = scopeFactory;
            HandlerType = handlerType;
        }

        public void Dispose()
        {
        }

        public IMessageHandlerDisposeWrapper GetHandler()
        {
            var scope = ScopeFactory.CreateScope();
            return new MessageHandlerDisposeWrapper(
                (IHandleMessage)scope.ServiceProvider.GetRequiredService(HandlerType),
                () => scope.Dispose()
            );
        }

        public bool IsInFactories(List<IMessageHandlerFactory> handlerFactories)
        {
            return handlerFactories
              .OfType<IocMessageHandlerFactory>()
              .Any(f => f.HandlerType == HandlerType);
        }
    }
}
