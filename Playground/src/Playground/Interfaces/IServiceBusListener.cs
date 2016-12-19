using System.Threading.Tasks;

namespace Playground.Exe.Interfaces
{
    public interface IServiceBusListener
    {
        Task StartAsync(string topic, string subscription);
    }
}
