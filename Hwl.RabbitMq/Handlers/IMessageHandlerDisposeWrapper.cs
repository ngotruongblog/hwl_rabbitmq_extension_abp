using System;

namespace Hwl.RabbitMQ
{
    public interface IMessageHandlerDisposeWrapper : IDisposable
    {
        IHandleMessage MessageHandler { get; }
    }
}
