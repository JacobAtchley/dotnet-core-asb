using System.Threading.Tasks;

namespace Playground.Exe.Interfaces
{
    public interface ISubscriber<in T>
    {
        Task OnMessageReceivedAsync(T model);
    }
}
