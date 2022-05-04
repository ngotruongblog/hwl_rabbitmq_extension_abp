using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace Hwl.RabbitMQ
{
    public class MessageErrorHandler : IMessageErrorHandler, ISingletonDependency
    {
        public const string HeadersKey = "headers";
        public const string RetryAttemptKey = "retryAttempt";

        protected RetryMessageOptions Options { get; }

        public MessageErrorHandler(IOptions<RetryMessageOptions> options)
        {
            Options = options.Value;
        }

        public virtual async Task HandleAsync(MessageExecutionErrorContext context)
        {
            if (!await ShouldHandleAsync(context))
            {
                ThrowOriginalExceptions(context);
            }

            if (await ShouldRetryAsync(context))
            {
                await RetryAsync(context);
                return;
            }

            await MoveToDeadLetterAsync(context);
        }

        protected async Task RetryAsync(MessageExecutionErrorContext context)
        {
            if (Options.RetryStrategyOptions.IntervalMillisecond > 0)
            {
                await Task.Delay(Options.RetryStrategyOptions.IntervalMillisecond);
            }

            context.TryGetRetryAttempt(out var retryAttempt);

            await context.EventBus.As<ManagementRabbitMq>().PublishAsync(
                context.EventType,
                context.EventData,
                context.GetProperty(HeadersKey).As<IBasicProperties>(),
                new Dictionary<string, object>
                {
                    {RetryAttemptKey, ++retryAttempt},
                    {"exceptions", context.Exceptions.Select(x => x.ToString()).ToList()}
                });
        }

        protected Task MoveToDeadLetterAsync(MessageExecutionErrorContext context)
        {
            ThrowOriginalExceptions(context);

            return Task.CompletedTask;
        }

        protected virtual Task<bool> ShouldHandleAsync(MessageExecutionErrorContext context)
        {
            if (!Options.EnabledErrorHandle)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(Options.ErrorHandleSelector == null || Options.ErrorHandleSelector.Invoke(context.EventType));
        }

        protected virtual Task<bool> ShouldRetryAsync(MessageExecutionErrorContext context)
        {
            if (Options.RetryStrategyOptions == null)
            {
                return Task.FromResult(false);
            }

            if (!context.TryGetRetryAttempt(out var retryAttempt))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(Options.RetryStrategyOptions.MaxRetryAttempts > retryAttempt);
        }

        protected virtual void ThrowOriginalExceptions(MessageExecutionErrorContext context)
        {
            if (context.Exceptions.Count == 1)
            {
                context.Exceptions[0].ReThrow();
            }

            throw new AggregateException(
                "More than one error has occurred while triggering the event: " + context.EventType,
                context.Exceptions);
        }
    }
}
