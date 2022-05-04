using System;

namespace Hwl.RabbitMQ
{
    public class MessageHandlerFactoryUnregistrar : IDisposable
    {
        private readonly IManagementRabbitMq _management;
        private readonly Type _eventType;
        private readonly IMessageHandlerFactory _factory;

        public MessageHandlerFactoryUnregistrar(
            IManagementRabbitMq management, Type eventType, IMessageHandlerFactory factory)
        {
            _eventType = eventType;
            _factory = factory;
            _management = management;
        }
        public async void Dispose()
        {
           await _management.Unsubscribe(_eventType, _factory);
        }
    }
}
