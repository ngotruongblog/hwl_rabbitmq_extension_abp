using System;
using System.Collections.Generic;
using Volo.Abp.Data;
using Volo.Abp.ObjectExtending;

namespace Hwl.RabbitMQ
{
    public class MessageExecutionErrorContext : ExtensibleObject
    {
        public IReadOnlyList<Exception> Exceptions { get; }

        public object EventData { get; set; }

        public Type EventType { get; }

        public IManagementRabbitMq EventBus { get; }

        public MessageExecutionErrorContext(List<Exception> exceptions, Type eventType, IManagementRabbitMq eventBus)
        {
            Exceptions = exceptions;
            EventType = eventType;
            EventBus = eventBus;
        }

        public bool TryGetRetryAttempt(out int retryAttempt)
        {
            retryAttempt = 0;
            if (!this.HasProperty(MessageErrorHandler.RetryAttemptKey))
            {
                return false;
            }

            retryAttempt = this.GetProperty<int>(MessageErrorHandler.RetryAttemptKey);
            return true;
        }
    }
}
