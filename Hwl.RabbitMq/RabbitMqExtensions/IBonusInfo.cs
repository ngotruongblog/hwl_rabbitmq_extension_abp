using System;
using System.Threading.Tasks;

namespace Hwl.RabbitMQ
{
    public interface IBonusInfo
    {
        string GetFullName();
        void SetFullName(string name);
        Type GetTypeMessage();
        void SetTypeMessage(Type messageType);
        Task DeleteQueue(string queueName);
    }
}
