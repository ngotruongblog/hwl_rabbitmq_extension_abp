using System;

namespace Hwl.RabbitMQ
{
    public class MessageHandlerDisposeWrapper : IMessageHandlerDisposeWrapper
    {
        public IHandleMessage MessageHandler { get; }

        private readonly Action _disposeAction;

        public MessageHandlerDisposeWrapper(IHandleMessage messHandler, Action disposeAction = null)
        {
            _disposeAction = disposeAction;
            MessageHandler = messHandler;
        }

        public void Dispose()
        {
            _disposeAction?.Invoke();
        }
    }
}
