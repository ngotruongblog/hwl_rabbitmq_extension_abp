using System;

namespace Hwl.RabbitMQ
{
    public class RetryMessageOptions
    {
        public bool EnabledErrorHandle { get; set; }
        public RetryStrategyOptions RetryStrategyOptions { get; set; }
        public Func<Type, bool> ErrorHandleSelector { get; set; }
        public void UseRetryStrategy(Action<RetryStrategyOptions> action = null)
        {
            EnabledErrorHandle = true;
            RetryStrategyOptions = new RetryStrategyOptions();
            action?.Invoke(RetryStrategyOptions);
        }
    }
}
