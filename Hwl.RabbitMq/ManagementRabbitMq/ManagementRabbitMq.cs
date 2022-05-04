using Hwl.RabbitMQ.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Collections;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.RabbitMQ;
using Volo.Abp.Reflection;
using Volo.Abp.Threading;

namespace Hwl.RabbitMQ
{
    public class ManagementRabbitMq : IManagementRabbitMq, ISingletonDependency
    {
        #region declare

        protected IRabbitMqMessageConsumerFactory MessageConsumerFactory { get; }
        protected ConcurrentDictionary<string, IRabbitMqMessageConsumer> Consumer { get; private set; }
        protected ConcurrentDictionary<string, Type> EventTypes { get; }
        protected IRabbitMqSerializer Serializer { get; }
        protected ConcurrentDictionary<Type, List<IMessageHandlerFactory>> HandlerFactories { get; }
        protected IServiceScopeFactory ServiceScopeFactory { get; }
        protected HandleMessageOptions HandleMessageOptions { get; }
        protected IConnectionPool ConnectionPool { get; }
        protected ConfigMessageOptions ConfigMessageOptions { get; }
        protected IMessageErrorHandler ErrorHandler { get; }
        protected ConcurrentDictionary<string, ConfigMessage> ConfigMessageDynamic { get; }

        #endregion

        #region ctor & Initialize

        public ManagementRabbitMq(
            IRabbitMqMessageConsumerFactory messageConsumerFactory,
            IRabbitMqSerializer serializer,
            IOptions<HandleMessageOptions> handleMessageOptions,
            IServiceScopeFactory serviceScopeFactory,
            IConnectionPool connectionPool,
            IOptions<ConfigMessageOptions> configMessageOptions,
            IMessageErrorHandler errorHandler)
        {
            MessageConsumerFactory = messageConsumerFactory;
            Serializer = serializer;
            HandleMessageOptions = handleMessageOptions.Value;
            ServiceScopeFactory = serviceScopeFactory;
            ConnectionPool = connectionPool;
            ConfigMessageOptions = configMessageOptions.Value;
            ErrorHandler = errorHandler;

            Consumer = new ConcurrentDictionary<string, IRabbitMqMessageConsumer>();
            EventTypes = new ConcurrentDictionary<string, Type>();
            HandlerFactories = new ConcurrentDictionary<Type, List<IMessageHandlerFactory>>();
            ConfigMessageDynamic = new ConcurrentDictionary<string, ConfigMessage>();
        }

        public async void Initialize()
        {
            // khoi tao config rabbitmq
            foreach (var item in ConfigMessageOptions.ConfigMsgs)
            {
                await SetConsumer
                 (
                      consumerName: item.ConsumerName,
                      exchangeName: item.ExchangeName,
                      typeExchange: item.TypeExchange,
                      queueName: item.QueueName,
                      deadLetterExchangeName: item.DeadLetterExchangeName,
                      deadLetterQueueName: item.DeadLetterQueueName,
                      connectionName: item.ConnectionName,
                      routingKey: item.RoutingKey,
                      fullNameMsg: item.FullName,
                      typeMsg: item.MessageType,
                      autoBinding: item.AutoBinding
                 );
            }
            // khoi tao danh sach handler message
            SubscribeHandlers(HandleMessageOptions.Handlers);
        }
        protected virtual void SubscribeHandlers(ITypeList<IHandleMessage> handlers)
        {
            foreach (var handler in handlers)
            {
                var interfaces = handler.GetInterfaces();
                foreach (var @interface in interfaces)
                {
                    if (!typeof(IHandleMessage).GetTypeInfo().IsAssignableFrom(@interface))
                    {
                        continue;
                    }
                    var genericArgs = @interface.GetGenericArguments();
                    if (genericArgs.Length == 1)
                    {
                        Subscribe(genericArgs[0], new IocMessageHandlerFactory(ServiceScopeFactory, handler)); // genericArgs[0] type of message
                    }
                }
            }
        }

        #endregion

        #region function implement

        public Task<List<ConfigMessage>> GetConfigMessageDynamic(Func<ConfigMessage, bool> predicate)
        {
            var res =new List<ConfigMessage>();
            if(predicate != null)
            {
                foreach (var item in ConfigMessageDynamic)
                {
                    var itm = predicate.Invoke(item.Value);
                    if (itm)
                    {
                        res.AddIfNotContains(item.Value);
                    }
                }
            }
            return Task.FromResult(res);
        }
        public Task<ConfigMessage> GetConfigMessageDynamic(string routingKey)
        {
            return Task.FromResult(ConfigMessageDynamic.GetOrDefault(routingKey));
        }
        public Task<bool> SetConfigMessageDynamic(string routingKey, ConfigMessage configMessage)
        {
            var res = ConfigMessageDynamic.AddIfNotContains(new KeyValuePair<string, ConfigMessage>(routingKey, configMessage));
            return Task.FromResult(res);
        }

        public Task<List<string>> GetNamesConsumer()
        {
            var res = Consumer.Keys?.ToList();
            return Task.FromResult(res);
        }
        public virtual async Task Binding(string routingKey, string consumerName)
        {
            if (Consumer.ContainsKey(consumerName))
            {
                var con = Consumer[consumerName];
                await con?.BindAsync(routingKey);
            }
        }
        public virtual async Task UnBinding(string routingKey, string consumerName)
        {
            if (Consumer.ContainsKey(consumerName))
            {
                var con = Consumer[consumerName];
                await con?.UnbindAsync(routingKey);
            }
        }
        public virtual Task SubscribeHandlers(Type _type)
        {
            SubscribeHandlers(HandleMessageOptions.Handlers, _type);
            return Task.CompletedTask;
        }
        protected virtual void SubscribeHandlers(ITypeList<IHandleMessage> handlers, Type _type)
        {
            foreach (var handler in handlers)
            {
                var interfaces = handler.GetInterfaces();
                foreach (var @interface in interfaces)
                {
                    if (!typeof(IHandleMessage).GetTypeInfo().IsAssignableFrom(@interface))
                    {
                        continue;
                    }
                    var genericArgs = @interface.GetGenericArguments();
                    if (genericArgs.Length == 1)
                    {
                        if (genericArgs[0] == _type)
                        {
                            Subscribe(genericArgs[0], new IocMessageHandlerFactory(ServiceScopeFactory, handler)); // genericArgs[0] type of message
                        }
                    }
                }
            }
        }
        public List<ConfigMessage> GetConfigMessage(string routingkey = null)
        {
            return ConfigMessageOptions.ConfigMsgs.WhereIf(!string.IsNullOrEmpty(routingkey), x => x.RoutingKey == routingkey).ToList();
        }
        private IDisposable Subscribe(Type eventType, IocMessageHandlerFactory factory)
        {
            var handlerFactories = GetOrCreateHandlerFactories(eventType);
            if (factory.IsInFactories(handlerFactories))
            {
                return NullDisposable.Instance;
            }
            handlerFactories.Add(factory);
            return new MessageHandlerFactoryUnregistrar(this, eventType, factory);
        }
        private List<IMessageHandlerFactory> GetOrCreateHandlerFactories(Type eventType)
        {
            return HandlerFactories.GetOrAdd(
                eventType,
                type =>
                {
                    var eventName = RoutingKeyAttribute.GetRoutingKeyOrDefault(type);
                    EventTypes[eventName] = type;
                    return new List<IMessageHandlerFactory>();
                }
            );
        }
        public async Task PublishAsync(Type eventType, object eventData)
        {
            await PublishAsync(eventType, eventData, null);
        }
        public Task PublishAsync(Type eventType, object eventData, IBasicProperties properties, Dictionary<string, object> headersArguments = null)
        {
            var routingKey = RoutingKeyAttribute.GetRoutingKeyOrDefault(eventType);
            var body = Serializer.Serialize(eventData);

            var config = ConfigMessageOptions.ConfigMsgs.Where(x => x.RoutingKey == routingKey).FirstOrDefault();
            if (config != null)
            {
                using (var channel = ConnectionPool.Get(config.ConnectionName).CreateModel())
                {
                    channel.ExchangeDeclare(
                        config.ExchangeName,
                        config.TypeExchange,
                        durable: true
                    );

                    if (properties == null)
                    {
                        properties = channel.CreateBasicProperties();
                        properties.DeliveryMode = RabbitMqConsts.DeliveryModes.Persistent;
                        properties.MessageId = Guid.NewGuid().ToString("N");
                    }

                    SetEventMessageHeaders(properties, headersArguments);

                    channel.BasicPublish(
                        exchange: config.ExchangeName,
                        routingKey: routingKey,
                        mandatory: true,
                        basicProperties: properties,
                        body: body
                    );
                }
            }

            return Task.CompletedTask;
        }
        public Task PublishAsync(string routingKey, object eventData, Dictionary<string, object> headersArguments = null)
        {
            var config = ConfigMessageDynamic[routingKey];
            PublishAsync(config, eventData, headersArguments);
            return Task.CompletedTask;
        }

        public Task PublishAsync(ConfigMessage config, object eventData, Dictionary<string, object> headersArguments = null)
        {
            var body = Serializer.Serialize(eventData);
            var isPublish = config != null
                && !string.IsNullOrEmpty(config.ExchangeName)
                && !string.IsNullOrEmpty(config.TypeExchange)
                && !string.IsNullOrEmpty(config.RoutingKey);
            if (isPublish)
            {
                using (var channel = ConnectionPool.Get(config.ConnectionName).CreateModel())
                {
                    channel.ExchangeDeclare(
                        config.ExchangeName,
                        config.TypeExchange,
                        durable: true
                    );

                    IBasicProperties properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = RabbitMqConsts.DeliveryModes.Persistent;
                    properties.MessageId = Guid.NewGuid().ToString("N");

                    SetEventMessageHeaders(properties, headersArguments);

                    channel.BasicPublish(
                        exchange: config.ExchangeName,
                        routingKey: config.RoutingKey,
                        mandatory: true,
                        basicProperties: properties,
                        body: body
                    );
                }
            }

            return Task.CompletedTask;
        }
        private void SetEventMessageHeaders(IBasicProperties properties, Dictionary<string, object> headersArguments)
        {
            if (headersArguments == null)
            {
                return;
            }

            properties.Headers = new Dictionary<string, object>();

            foreach (var header in headersArguments)
            {
                properties.Headers[header.Key] = header.Value;
            }
        }
        public Task Unsubscribe(Type eventType, IMessageHandlerFactory factory)
        {
            GetOrCreateHandlerFactories(eventType).Locking(factories => factories.Remove(factory));
            return Task.CompletedTask;
        }
        public async Task<string> DuplicateConsumer(string routingKey)
        {
            var config = ConfigMessageOptions.ConfigMsgs.Where(x => x.RoutingKey == routingKey).FirstOrDefault();
            var guid = Guid.NewGuid().ToString("N");
            var consumerName = $"{config.ConsumerName}_{guid}";
            return await DuplicateConsumer(config, consumerName);
        }
        public async Task<string> DuplicateConsumer(string routingKey, string consumerName)
        {
            var config = ConfigMessageOptions.ConfigMsgs.Where(x => x.RoutingKey == routingKey).FirstOrDefault();
            return await DuplicateConsumer(config, consumerName);
        }
        private async Task<string> DuplicateConsumer(ConfigMessage config, string consumerName)
        {
            await SetConsumer(
                consumerName,
                config.ExchangeName,
                config.TypeExchange,
                config.QueueName,
                config.DeadLetterExchangeName,
                config.DeadLetterQueueName,
                config.ConnectionName,
                config.RoutingKey,
                config.FullName,
                config.MessageType,
                config.AutoBinding);
            return consumerName;
        }
        public Task Unsubscribe(Type messageType)
        {
            GetOrCreateHandlerFactories(messageType).Locking(factories => factories.Clear());
            return Task.CompletedTask;
        }
        public async Task SetConsumer(
            string consumerName,
            string exchangeName,
            string typeExchange,
            string queueName,
            string deadLetterExchangeName,
            string deadLetterQueueName,
            string connectionName,
            string routingKey,
            string fullNameMsg,
            Type typeMsg,
            bool autoBinding)
        {
            var consumer = MessageConsumerFactory.Create(
                new ExchangeDeclareConfiguration(
                    exchangeName,
                    type: typeExchange,
                    durable: true,
                    deadLetterExchangeName: deadLetterExchangeName
                ),
                new QueueDeclareConfiguration(
                    queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    deadLetterQueueName: deadLetterQueueName
                ), connectionName
            );
            IBonusInfo bonus = consumer as IBonusInfo;
            if (bonus != null)
            {
                bonus.SetFullName(fullNameMsg);
                bonus.SetTypeMessage(typeMsg);
            }
            if (!string.IsNullOrEmpty(routingKey) && autoBinding)
            {
                await consumer.BindAsync(routingKey);
            }
            consumer.OnMessageReceived(ProcessEventAsync);
            GetOrAddConsumer(consumerName, consumer);
        }

        public async Task<string> AddConsumerDynamic(
            string consumerName,
            string exchangeName,
            string typeExchange,
            string queueName,
            string deadLetterExchangeName,
            string deadLetterQueueName,
            string connectionName,
            string routingKey,
            bool autoBinding)
        {
            if (Consumer.ContainsKey(consumerName))
            {
                return "Consumer exist";
            }

            var consumer = MessageConsumerFactory.Create(
                new ExchangeDeclareConfiguration(
                    exchangeName,
                    type: typeExchange,
                    durable: true,
                    deadLetterExchangeName: deadLetterExchangeName
                ),
                new QueueDeclareConfiguration(
                    queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    deadLetterQueueName: deadLetterQueueName
                ), connectionName
            );
            if (!string.IsNullOrEmpty(routingKey) && autoBinding)
            {
                await consumer.BindAsync(routingKey);
            }
            consumer.OnMessageReceived(ProcessEventAsync);
            GetOrAddConsumer(consumerName, consumer);

            var config = new ConfigMessage
            {
                 ConsumerName =consumerName,
                 ExchangeName = exchangeName,
                 TypeExchange = typeExchange,
                 QueueName =  queueName,
                 DeadLetterExchangeName = deadLetterExchangeName,
                 DeadLetterQueueName= deadLetterQueueName,
                 ConnectionName= connectionName,
                 RoutingKey=routingKey,
                 AutoBinding = autoBinding
            };
            GetOrAddConfigMessage(routingKey, config);

            return String.Empty;
        }

        private IRabbitMqMessageConsumer GetOrAddConsumer(string key, IRabbitMqMessageConsumer csm)
        {
            return Consumer.GetOrAdd(key, type => csm);
        }
        private ConfigMessage GetOrAddConfigMessage(string key, ConfigMessage csm)
        {
            return ConfigMessageDynamic.GetOrAdd(key, type => csm);
        }
        public async Task RemoveConsumer(string consumerName, bool isRemoveHander = false)
        {
            if (Consumer.TryRemove(consumerName, out var consumer))
            {
                if (consumer != null)
                {
                    ((IDisposable)consumer).Dispose();
                    IBonusInfo bonus = consumer as IBonusInfo;
                    if (bonus != null && isRemoveHander)
                    {
                        var typeMsg = bonus.GetTypeMessage();
                        await Unsubscribe(typeMsg);
                    }
                }
            }
        }
        public async Task RemoveConsumer(string consumerName, string queueName, bool isRemoveHander = false)
        {
            if (Consumer.TryRemove(consumerName, out var consumer))
            {
                if (consumer != null)
                {
                    IBonusInfo info = consumer as IBonusInfo;
                    info?.DeleteQueue(queueName);
                    ((IDisposable)consumer).Dispose();
                    IBonusInfo bonus = consumer as IBonusInfo;
                    if (bonus != null && isRemoveHander)
                    {
                        var typeMsg = bonus.GetTypeMessage();
                        await Unsubscribe(typeMsg);
                    }
                }
            }
        }
        #endregion

        #region handle message

        private async Task ProcessEventAsync(IModel channel, BasicDeliverEventArgs ea)
        {
            var eventName = ea.RoutingKey;
            var eventType = EventTypes.GetOrDefault(eventName);
            if (eventType == null)
            {
                return;
            }

            var eventData = Serializer.Deserialize(ea.Body.ToArray(), eventType);

            await TriggerHandlersAsync(eventType, eventData, errorContext =>
            {
                var retryAttempt = 0;
                if (ea.BasicProperties.Headers != null &&
                    ea.BasicProperties.Headers.ContainsKey(MessageErrorHandler.RetryAttemptKey))
                {
                    retryAttempt = (int)ea.BasicProperties.Headers[MessageErrorHandler.RetryAttemptKey];
                }

                errorContext.EventData = Serializer.Deserialize(ea.Body.ToArray(), eventType);
                errorContext.SetProperty(MessageErrorHandler.HeadersKey, ea.BasicProperties);
                errorContext.SetProperty(MessageErrorHandler.RetryAttemptKey, retryAttempt);
            });
        }
        public virtual async Task TriggerHandlersAsync(Type eventType, object eventData, Action<MessageExecutionErrorContext> onErrorAction = null)
        {
            var exceptions = new List<Exception>();

            await TriggerHandlersAsync(eventType, eventData, exceptions);

            if (exceptions.Any())
            {
                var context = new MessageExecutionErrorContext(exceptions, eventType, this);
                onErrorAction?.Invoke(context);
                await ErrorHandler.HandleAsync(context);
            }
        }
        protected virtual async Task TriggerHandlersAsync(Type eventType, object eventData, List<Exception> exceptions)
        {
            await new SynchronizationContextRemover();

            foreach (var handlerFactories in GetHandlerFactories(eventType))
            {
                foreach (var handlerFactory in handlerFactories.MessageHandlerFactories)
                {
                    await TriggerHandlerAsync(handlerFactory, handlerFactories.MessageType, eventData, exceptions);
                }
            }
        }
        protected virtual async Task TriggerHandlerAsync(
            IMessageHandlerFactory asyncHandlerFactory, Type eventType, object eventData, List<Exception> exceptions)
        {
            using (var messageHandlerWrapper = asyncHandlerFactory.GetHandler())
            {
                try
                {
                    var handlerType = messageHandlerWrapper.MessageHandler.GetType();

                    if (ReflectionHelper.IsAssignableToGenericType(handlerType, typeof(IHandleMessageGeneric<>)))
                    {
                        var method = typeof(IHandleMessageGeneric<>)
                            .MakeGenericType(eventType)
                            .GetMethod(
                                nameof(IHandleMessageGeneric<object>.ProcessMessageAsync),
                                new[] { eventType }
                            );

                        await ((Task)method.Invoke(messageHandlerWrapper.MessageHandler, new[] { eventData }));
                    }
                    else
                    {
                        throw new AbpException("The object instance is not an message handler. Object type: " + handlerType.AssemblyQualifiedName);
                    }
                }
                catch (TargetInvocationException ex)
                {
                    exceptions.Add(ex.InnerException);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
        }
        protected IEnumerable<MessageTypeWithMessageHandlerFactories> GetHandlerFactories(Type eventType)
        {
            var handlerFactoryList = new List<MessageTypeWithMessageHandlerFactories>();

            foreach (var handlerFactory in
                HandlerFactories.Where(hf => ShouldTriggerEventForHandler(eventType, hf.Key)))
            {
                handlerFactoryList.Add(
                    new MessageTypeWithMessageHandlerFactories(handlerFactory.Key, handlerFactory.Value));
            }

            return handlerFactoryList.ToArray();
        }
        private static bool ShouldTriggerEventForHandler(Type targetEventType, Type handlerEventType)
        {
            //Should trigger same type
            if (handlerEventType == targetEventType)
            {
                return true;
            }

            //TODO: Support inheritance? But it does not support on subscription to RabbitMq!
            //Should trigger for inherited types
            if (handlerEventType.IsAssignableFrom(targetEventType))
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}
