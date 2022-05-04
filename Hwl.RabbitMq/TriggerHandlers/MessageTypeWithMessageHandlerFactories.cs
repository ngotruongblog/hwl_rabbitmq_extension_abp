using System;
using System.Collections.Generic;

namespace Hwl.RabbitMQ
{
    public class MessageTypeWithMessageHandlerFactories
    {
        public Type MessageType { get; }

        public List<IMessageHandlerFactory> MessageHandlerFactories { get; }

        public MessageTypeWithMessageHandlerFactories(Type messageType, List<IMessageHandlerFactory> messageHandlerFactories)
        {
            MessageType = messageType;
            MessageHandlerFactories = messageHandlerFactories;
        }
    }
}