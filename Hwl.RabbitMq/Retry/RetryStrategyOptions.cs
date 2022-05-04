namespace Hwl.RabbitMQ
{
    public class RetryStrategyOptions
    {
        public int IntervalMillisecond { get; set; } = 3000;

        public int MaxRetryAttempts { get; set; } = 3;
    }
}
