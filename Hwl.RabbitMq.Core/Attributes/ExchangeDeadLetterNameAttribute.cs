using JetBrains.Annotations;
using System;
using System.Linq;
using Volo.Abp;

namespace Hwl.RabbitMQ.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ExchangeDeadLetterNameAttribute : Attribute, IDeadLetterExchangeNameProvider
    {
        public virtual string ExchangeDeadLetterName { get; }
        public ExchangeDeadLetterNameAttribute([NotNull] string exchangeDeadLetterName)
        {
            ExchangeDeadLetterName = Check.NotNullOrWhiteSpace(exchangeDeadLetterName, nameof(exchangeDeadLetterName)); 
        }
        public static string GetExchangeDeadLetterNameOrDefault([NotNull] Type eventType)
        {
            Check.NotNull(eventType, nameof(eventType));
            return eventType.GetCustomAttributes(true).OfType<IDeadLetterExchangeNameProvider>()
                .FirstOrDefault()?.GetDeadLetterExchangeName(eventType) ?? String.Empty;
        }
        public string GetDeadLetterExchangeName(Type eventType)
        {
            return ExchangeDeadLetterName;
        }
       
    }
}
