using JetBrains.Annotations;
using System;
using System.Linq;
using Volo.Abp;

namespace Hwl.RabbitMQ.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class QueueNameAttribute : Attribute, IQueueNameProvider
    {
        public virtual string QueueName { get; }
        public QueueNameAttribute([NotNull] string queueName)
        {
            QueueName = Check.NotNullOrWhiteSpace(queueName, nameof(queueName));
        }
        public static string GetQueueNameOrDefault([NotNull] Type eventType)
        {
            Check.NotNull(eventType, nameof(eventType));
            return eventType.GetCustomAttributes(true).OfType<IQueueNameProvider>()
                       .FirstOrDefault()?.GetQueueName(eventType) ?? eventType.FullName;
        }
        public string GetQueueName(Type eventType)
        {
            return QueueName;
        }
    }
}
