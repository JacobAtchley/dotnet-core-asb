using System;
using System.Threading.Tasks;

namespace Playground.Exe
{
    public interface ISubscriber
    {
        Type MessageType { get;}

        Task OnMessageRecieved<T>(T message);
    }
}
