using Hwl.RabbitMQ.Core;
using System.Collections.Generic;

namespace Hwl.EmailMessage
{
    [ExchangeName(exchangeName: RabbitConfiguration.ExchangeEmailName)]
    [QueueName(RabbitConfiguration.QueueEmailName)]
    [ExchangeDeadLetterName(RabbitConfiguration.ExchangeDeadLetterName)]
    [QueueDeadLetterName(RabbitConfiguration.QueueDeadLetterName)]
    [TypeExchange("direct")]
    [AutoBinding(true)]
    [RoutingKey(RabbitConfiguration.RoutingKey)]
    public class EmailMessageEto: IConfigMessage
    {
        public List<string> To { get; set; }
        public List<string> Cc { get; set; }
        public List<string> Bcc { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string TemplateCode { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ClientCode { get; set; }
        public bool IsBodyHtml { get; set; }
        public Dictionary<string, object> BodyModel { get; set; }
        public Dictionary<string, object> SubjectModel { get; set; }
        public List<string> FileAttachs { get; set; }
    }
}
