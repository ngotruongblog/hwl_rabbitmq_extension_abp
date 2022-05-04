namespace Hwl.EmailMessageSchedule
{
    public class RabbitMessScheConfiguration
    {
        public const string ExchangeEmailName = "ex_hwl_email";
        public const string ExchangeDeadLetterName = "ex_hwl_email_dead_letter";
        public const string QueueDeadLetterName = "queue_sche_email_dead_letter";
    }
}
