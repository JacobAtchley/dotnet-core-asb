using System;
using System.Collections.Generic;

namespace Playground.Exe.Interfaces
{
    public interface ISubscriberLocator
    {
        IEnumerable<object> FindSubscribers(Type messagType);
    }
}
