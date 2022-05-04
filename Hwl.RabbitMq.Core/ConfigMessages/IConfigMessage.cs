using Volo.Abp.DependencyInjection;

namespace Hwl.RabbitMQ.Core
{
    /// <summary>
    /// Moi message co key = routingkey
    /// </summary>
    public interface IConfigMessage: ITransientDependency
    {
    }
}
