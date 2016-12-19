using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Playground.Exe.Interfaces;

namespace Playground.Exe
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddUserSecrets()
                ;

            var configuration = builder.Build();

            var serviceCollection = new ServiceCollection()
                .AddOptions()
                .Configure<ServiceBusSettings>(configuration.GetSection("ServiceBus"))
                .AddTransient(x => x.GetService<IOptions<ServiceBusSettings>>().Value)
                .AddTransient<IPublisher, ServiceBusClient>()
                .AddTransient<ISubscriberLocator, SubscriberLocator>()
                .AddTransient<IServiceBusListener, ServiceBusClient>()
                .AddTransient<ISubscriber<ServiceBusMessage>, Subscriber>();



            MyExample(serviceCollection.BuildServiceProvider());
        }

        private static void MyExample(IServiceProvider provider)
        {

            var publisher = provider.GetService<IPublisher>();
            var client = provider.GetService<IServiceBusListener>();

            client.StartAsync("core-test", "core-test-subscription");

            Console.WriteLine("Press quit to exit. Any other key sends another service bus message...");
            do
            {
                publisher.PublishAsync(new ServiceBusMessage
                                 {
                                     Message = $"Hello it is {DateTimeOffset.UtcNow}",
                                     Id = Guid.NewGuid().ToString()
                                 }, "core-test").Wait();
            }
            while (Console.ReadLine() != "quit");
        }
    }
}