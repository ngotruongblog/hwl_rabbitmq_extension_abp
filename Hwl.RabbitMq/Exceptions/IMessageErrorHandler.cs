using System.Threading.Tasks;

namespace Hwl.RabbitMQ
{
    public interface IMessageErrorHandler
    {
        Task HandleAsync(MessageExecutionErrorContext context);
    }
}
