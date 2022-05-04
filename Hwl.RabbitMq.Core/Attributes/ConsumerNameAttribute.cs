using JetBrains.Annotations;
using System;
using System.Linq;
using Volo.Abp;

namespace Hwl.RabbitMQ.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ConsumerNameAttribute : Attribute, IConsumerNameProvider
    {
        public virtual string ConsumerName { get; }
        public ConsumerNameAttribute([NotNull] string consumerName)
        {
            ConsumerName = Check.NotNullOrWhiteSpace(consumerName, nameof(consumerName)); ;
        }
        public static string GetConsumerNameOrDefault([NotNull] Type eventType)
        {
            Check.NotNull(eventType, nameof(eventType));
            return eventType.GetCustomAttributes(true).OfType<IConsumerNameProvider>()
                       .FirstOrDefault()?.GetConsumerName(eventType) ?? eventType.FullName;
        }
        public string GetConsumerName(Type eventType)
        {
            return ConsumerName;
        }
    }
}
