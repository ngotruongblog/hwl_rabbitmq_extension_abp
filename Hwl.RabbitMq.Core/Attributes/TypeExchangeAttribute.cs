using JetBrains.Annotations;
using System;
using System.Linq;
using Volo.Abp;

namespace Hwl.RabbitMQ.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TypeExchangeAttribute : Attribute, ITypeExchangeProvider
    {
        public virtual string TypeExchange { get; }
        public TypeExchangeAttribute([NotNull] string typeExchange)
        {
            TypeExchange = Check.NotNullOrWhiteSpace(typeExchange, nameof(typeExchange));
        }
        public static string GetTypeExchangeOrDefault([NotNull] Type eventType)
        {
            Check.NotNull(eventType, nameof(eventType));
            return eventType.GetCustomAttributes(true).OfType<ITypeExchangeProvider>()
                       .FirstOrDefault()?.GetTypeExchange(eventType) ?? "direct";
        }
        public string GetTypeExchange(Type eventType)
        {
            return TypeExchange;
        }
    }
}
