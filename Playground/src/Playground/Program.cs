using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Configuration;

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

            MyExample(configuration);
        }

        private static void MyExample(IConfiguration configuration)
        {
            var settings = new ServiceBusSettings
            {
                Namespace = configuration["ServiceBus:NamespaceUrl"],
                PolicyName = WebUtility.UrlEncode(configuration["ServiceBus:PolicyName"]),
                Key = WebUtility.UrlEncode(configuration["ServiceBus:Key"])
            };

            //todo: this is where DI would come in and supply the subscribers.
            var client = new ServiceBusClient(settings, new ISubscriber[] {new Subscriber()});

            client.StartListenerAsync("core-test", "core-test-subscription");

            Console.WriteLine("Press quit to exit. Any other key sends another service bus message...");
            do
            {
                client.SendAsync(new ServiceBusMessage
                                 {
                                     Message = $"Hello it is {DateTimeOffset.UtcNow}",
                                     Id = Guid.NewGuid().ToString()
                                 }, "core-test").Wait();
            }
            while (Console.ReadLine() != "quit");
        }
    }
}