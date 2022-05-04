using JetBrains.Annotations;
using System;
using System.Linq;
using Volo.Abp;

namespace Hwl.RabbitMQ.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ConnectionNameAttribute : Attribute, IConnectionNameProvider
    {
        public virtual string ConnectionName { get; }
        public ConnectionNameAttribute([NotNull] string connectionName = null)
        {
            ConnectionName = Check.NotNullOrWhiteSpace(connectionName, nameof(connectionName));
        }
        public static string GetConnectionNameOrDefault([NotNull] Type eventType)
        {
            Check.NotNull(eventType, nameof(eventType));
            return eventType.GetCustomAttributes(true).OfType<IConnectionNameProvider>()
                .FirstOrDefault()?.GetConnectionName(eventType) ?? "Default";
        }
        public string GetConnectionName(Type eventType)
        {
            return ConnectionName;
        }
    }
}
