using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Playground.Exe
{
    public class Subscriber : ISubscriber
    {
        public Type MessageType => typeof(ServiceBusMessage);

        public Task OnMessageRecieved<T>(T message)
        {
            var sbm = message as ServiceBusMessage;

            if (sbm != null)
            {
                Console.WriteLine($"Hello From Consumer: {sbm.Message}. Message Id {sbm.Id}");
            }

            return Task.CompletedTask;
        }
    }
}
