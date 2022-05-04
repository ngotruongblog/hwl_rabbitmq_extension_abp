using JetBrains.Annotations;
using System;
using System.Linq;
using Volo.Abp;

namespace Hwl.RabbitMQ.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoBindingAttribute : Attribute, IAutoBindingProvider
    {
        public virtual bool AutoBinding { get; }
        public AutoBindingAttribute(bool autoBinding)
        {
            AutoBinding = autoBinding;
        }
        public static bool GetAutoBindingOrDefault([NotNull] Type eventType)
        {
            Check.NotNull(eventType, nameof(eventType));
            return eventType.GetCustomAttributes(true).OfType<IAutoBindingProvider>()
                       .FirstOrDefault()?.GetAutoBinding(eventType) ?? true;
        }
        public bool GetAutoBinding(Type eventType)
        {
            return AutoBinding;
        }
    }
}
