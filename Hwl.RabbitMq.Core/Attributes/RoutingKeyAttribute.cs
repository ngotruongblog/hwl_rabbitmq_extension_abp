using JetBrains.Annotations;
using System;
using System.Linq;
using Volo.Abp;

namespace Hwl.RabbitMQ.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RoutingKeyAttribute : Attribute, IRoutingKeyProvider 
    {
        public virtual string RoutingKey { get; }
        public RoutingKeyAttribute([NotNull] string routingKey = null)
        {
            RoutingKey = Check.NotNullOrWhiteSpace(routingKey, nameof(routingKey));
        }
        
        public static string GetRoutingKeyOrDefault([NotNull] Type eventType)
        {
            Check.NotNull(eventType, nameof(eventType));
            return eventType.GetCustomAttributes(true).OfType<IRoutingKeyProvider>()
                       .FirstOrDefault()?.GetRoutingKey(eventType) ?? eventType.FullName;
        }
        public string GetRoutingKey(Type eventType)
        {
            return RoutingKey;
        }
    }
}
