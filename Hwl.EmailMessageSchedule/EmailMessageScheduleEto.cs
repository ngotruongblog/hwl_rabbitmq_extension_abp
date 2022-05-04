using Hwl.RabbitMQ.Core;

namespace Hwl.EmailMessageSchedule
{
    [ExchangeName(RabbitMessScheConfiguration.ExchangeEmailName)]
    [ExchangeDeadLetterName(RabbitMessScheConfiguration.ExchangeDeadLetterName)]
    [QueueDeadLetterName(RabbitMessScheConfiguration.QueueDeadLetterName)]
    [TypeExchange("direct")]
    [AutoBinding(false)]
    public class EmailMessageScheduleEto
    {
        public string Code { get; set; }
    }
}
