using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Playground.Exe.Interfaces;

namespace Playground.Exe
{
    public class SubscriberLocator : ISubscriberLocator
    {
        private readonly IServiceProvider _serviceProvider;

        public SubscriberLocator(IServiceProvider provider) { _serviceProvider = provider; }

        public IEnumerable<object> FindSubscribers(Type messagType)
        {
            var subscriberType = typeof(ISubscriber<>).MakeGenericType(messagType);

            var services = _serviceProvider
                .GetServices(subscriberType)
                .DefaultIfEmpty()
                .ToArray();

            return services;
        }
    }
}
