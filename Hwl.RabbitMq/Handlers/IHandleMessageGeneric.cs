using System.Threading.Tasks;

namespace Hwl.RabbitMQ
{
    public interface IHandleMessageGeneric<in TMessage> : IHandleMessage
    {
        Task ProcessMessageAsync(TMessage eventData);
    }
}
