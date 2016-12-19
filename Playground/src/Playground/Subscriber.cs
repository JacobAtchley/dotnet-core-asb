using System;
using System.Threading.Tasks;
using Playground.Exe.Interfaces;

namespace Playground.Exe
{
    public class Subscriber : ISubscriber<ServiceBusMessage>
    {
        public Task OnMessageReceivedAsync(ServiceBusMessage model)
        {
            if (model != null)
                Console.WriteLine($"Hello From Consumer: {model.Message}. Message Id {model.Id}");

            return Task.CompletedTask;
        }
    }
}