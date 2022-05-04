using System.Collections.Generic;

namespace Hwl.RabbitMQ.Core
{
    public class ConfigMessageOptions
    {
        public List<ConfigMessage> ConfigMsgs { get; }
        public ConfigMessageOptions()
        {
            ConfigMsgs = new List<ConfigMessage>();
        }
    }
}
