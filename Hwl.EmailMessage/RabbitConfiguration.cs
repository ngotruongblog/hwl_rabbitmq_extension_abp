namespace Hwl.EmailMessage
{
    public class RabbitConfiguration
    {
        public const string ExchangeEmailName = "ex_hwl_email";
        public const string QueueEmailName = "queue_email_system";
        public const string ExchangeDeadLetterName = "ex_hwl_email_dead_letter";
        public const string QueueDeadLetterName = "queue_email_system_dead_letter";
        public const string RoutingKey = "Hwl.EmailMessage.EmailMessageEto";
    }
}
