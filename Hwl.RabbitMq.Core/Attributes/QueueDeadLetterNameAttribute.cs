using JetBrains.Annotations;
using System;
using System.Linq;
using Volo.Abp;

namespace Hwl.RabbitMQ.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class QueueDeadLetterNameAttribute : Attribute, IDeadLetterQueueNameProvider
    {
        public virtual string QueueDeadLetterName { get; }
        public QueueDeadLetterNameAttribute([NotNull] string queueDeadLetterName )
        {
            QueueDeadLetterName = Check.NotNullOrWhiteSpace(queueDeadLetterName, nameof(queueDeadLetterName)); 
        }
        public static string GetQueueDeadLetterNameOrDefault([NotNull] Type eventType)
        {
            Check.NotNull(eventType, nameof(eventType));
            return eventType.GetCustomAttributes(true).OfType<IDeadLetterQueueNameProvider>()
                .FirstOrDefault()?.GetDeadLetterQueueName(eventType) ?? String.Empty;
        }
        public string GetDeadLetterQueueName(Type eventType)
        {
            return QueueDeadLetterName;
        }
    }
}
