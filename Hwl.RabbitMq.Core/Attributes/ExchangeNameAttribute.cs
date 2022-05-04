using JetBrains.Annotations;
using System;
using System.Linq;
using Volo.Abp;

namespace Hwl.RabbitMQ.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ExchangeNameAttribute : Attribute, IExchangeNameProvider
    {
        public virtual string ExchangeName { get; }
        public ExchangeNameAttribute([NotNull]string exchangeName)
        {
            ExchangeName = Check.NotNullOrWhiteSpace(exchangeName, nameof(exchangeName)); ;
        }
        public static string GetExchangeNameOrDefault([NotNull] Type eventType)
        {
            Check.NotNull(eventType, nameof(eventType));
            return eventType.GetCustomAttributes(true).OfType<IExchangeNameProvider>()
                       .FirstOrDefault()?.GetExchangeName(eventType) ?? eventType.FullName;
        }
        public string GetExchangeName(Type eventType)
        {
            return ExchangeName;
        }
    }
}
