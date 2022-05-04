using Volo.Abp.Collections;

namespace Hwl.RabbitMQ
{
    public class HandleMessageOptions
    {
        public ITypeList<IHandleMessage> Handlers { get; }

        public HandleMessageOptions()
        {
            Handlers = new TypeList<IHandleMessage>();
        }
    }
}
